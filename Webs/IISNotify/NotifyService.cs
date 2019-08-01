using IISNotify;
using System;
using System.Collections.Concurrent;
using System.ServiceModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace F88.Notify
{
    [ServiceContract(CallbackContract = typeof(INotifyClientCallback))]
    public interface INotifyService
    {
        [OperationContract(IsOneWay = true)]
        Task Subscribe(string sessionId);
        Task Unsubscribe(string sessionId);
    }

    [ServiceContract]
    public interface INotifyClientCallback
    {
        [OperationContract(IsOneWay = true)]
        Task WriteNotifyToClient(string msg);
    }

    public interface INotifyPush
    {
        void Send(string message, long user_id = 0);
    }

    public class NotifyService : INotifyService, INotifyPush
    {
        static ConcurrentDictionary<long, StringBuilder> _userMessage = new ConcurrentDictionary<long, StringBuilder>() { };
        static ConcurrentDictionary<long, ManualResetEvent> _userSignal = new ConcurrentDictionary<long, ManualResetEvent>() { };

        readonly IDataflow _dataflow;
        public NotifyService() { }
        public NotifyService(IDataflow dataflow) : base() => _dataflow = dataflow; 

        public void Send(string message, long user_id = 0)
        {
            StringBuilder buffer;
            if (_userMessage.ContainsKey(user_id)
                && _userMessage.TryGetValue(user_id, out buffer) && buffer != null)
            {
                if (buffer.Length == 0)
                    buffer.Append(message);
                else
                    buffer.Append("|" + message);
            }
            else _userMessage.TryAdd(user_id, new StringBuilder(message));

            ManualResetEvent signal;
            if (_userSignal.ContainsKey(user_id)
                && _userSignal.TryGetValue(user_id, out signal) && signal != null)
                signal.Set();
            else _userSignal.TryAdd(user_id, new ManualResetEvent(false));
        }

        public async Task Subscribe(string sessionId)
        {  
            var callback = OperationContext.Current.GetCallbackChannel<INotifyClientCallback>();
            var random = new Random();
            double price = 29.00;

            while (((System.ServiceModel.Channels.IChannel)callback).State == CommunicationState.Opened)
            {
                await callback.WriteNotifyToClient(Guid.NewGuid().ToString());
                price += random.NextDouble();
                await Task.Delay(1000);
            }
        }

        public Task Unsubscribe(string sessionId)
        { 
            return new Task(() => { });
        }
    }
}