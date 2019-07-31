using System;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Admin
{
    [ServiceContract(CallbackContract = typeof(IDuplexClient))]
    public interface IDuplexService
    {
        [OperationContract]
        void Subscribe(string userId);

        [OperationContract]
        void Unsubscribe(string userId);
    }

    [ServiceContract]
    public interface IDuplexClient
    {
        [OperationContract(IsOneWay = true)]
        void PushNotification(string msg);
    }















    [ServiceContract(CallbackContract = typeof(IStockQuoteCallback))]
    public interface IStockQuoteService
    {
        [OperationContract(IsOneWay = true)]
        Task StartSendingQuotes();
    }

    [ServiceContract]
    public interface IStockQuoteCallback
    {
        [OperationContract(IsOneWay = true)]
        Task SendQuote(string code, double value); 
    }

    public interface INotifyService {
        void Push(string message, long user_id = 0);
    }

    public class StockQuoteService : IStockQuoteService, INotifyService
    {
        static ConcurrentDictionary<long, StringBuilder> _userMessage = new ConcurrentDictionary<long, StringBuilder>() { };
        static ConcurrentDictionary<long, ManualResetEvent> _userSignal = new ConcurrentDictionary<long, ManualResetEvent>() { };

        readonly IDataflow _dataflow;
        public StockQuoteService(IDataflow dataflow) : base()
        {
            _dataflow = dataflow;
            string test = dataflow.test1("");
        }

        public void Push(string message, long user_id = 0)
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


        public async Task StartSendingQuotes()
        {
            var callback = OperationContext.Current.GetCallbackChannel<IStockQuoteCallback>();
            var random = new Random();
            double price = 29.00;

            while (((IChannel)callback).State == CommunicationState.Opened)
            {
                await callback.SendQuote(_dataflow.test1(string.Empty), price);
                price += random.NextDouble();
                await Task.Delay(1000);
            }
        }
    }
    
    public class StockQuoteServiceBehavior : IServiceBehavior, IInstanceProvider
    {
        private readonly object _instance;
        public StockQuoteServiceBehavior(object instance)=> _instance = instance;
        public object GetInstance(InstanceContext instanceContext) => _instance;
        //public object GetInstance(InstanceContext instanceContext) { return new oUserService(_dataflow, _cacheFields); }
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        public void AddBindingParameters(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase, Collection<ServiceEndpoint> endpoints, BindingParameterCollection bindingParameters) { }
        public void Validate(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase) { }
        public void ReleaseInstance(InstanceContext instanceContext, object instance) { }
        public object GetInstance(InstanceContext instanceContext, Message message) => this.GetInstance(instanceContext);
        public void ApplyDispatchBehavior(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase)
        {
            foreach (ChannelDispatcher cd in serviceHostBase.ChannelDispatchers)
                foreach (EndpointDispatcher ed in cd.Endpoints)
                    ed.DispatchRuntime.InstanceProvider = this;
        }
    }
}