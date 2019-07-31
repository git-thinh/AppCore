using System;
using System.Collections.ObjectModel;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using System.Threading.Tasks;

namespace Admin
{
    [ServiceContract(Namespace = "Silverlight", CallbackContract = typeof(IDuplexClient))]
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















    [ServiceContract]
    public interface IStockQuoteCallback
    {
        [OperationContract(IsOneWay = true)]
        Task SendQuote(string code, double value); 
    }

    [ServiceContract(CallbackContract = typeof(IStockQuoteCallback))]
    public interface IStockQuoteService
    {
        [OperationContract(IsOneWay = true)]
        Task StartSendingQuotes();
    }

    public interface INotifyService {
        void Push(string message);
    }

    public class StockQuoteService : IStockQuoteService, INotifyService
    {
        readonly IDataflow _dataflow;
        public StockQuoteService(IDataflow dataflow) : base()
        {
            _dataflow = dataflow;
            string test = dataflow.test1("");
        }

        public void Push(string message)
        {
            
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
        public StockQuoteServiceBehavior(object instance)
        {
            _instance = instance;
        }
        public object GetInstance(InstanceContext instanceContext) { return _instance; }
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