using System;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.Threading.Tasks;
using Microsoft.ServiceModel.WebSockets;

namespace Admin
{
    public interface ISocketService {
        bool isOpend { set; get; }
        Task SendMessage(string message);
    }

    public class SessionSocketService : WebSocketService, ISocketService
    {
        private class CallbackHandler : StockQuoteServiceReference.IStockQuoteServiceCallback
        {
            readonly ISocketService socket;

            public CallbackHandler(ISocketService _socket) : base() => this.socket = _socket;

            public async void SendQuote(string code, double value)
            { 
               if(socket.isOpend)
                    await socket.SendMessage(string.Format("------------------> {0}: {1:f2}", code, value));
            }
        }

        public bool isOpend { set; get; }
        InstanceContext context;
        StockQuoteServiceReference.StockQuoteServiceClient client;
        StockQuoteServiceReference.IStockQuoteServiceCallback clientCallback;
        public SessionSocketService()
        {
            isOpend = false;
        }

        public string m_sessionID
        {
            get
            {
                return this.WebSocketContext.SecWebSocketKey;
            }
        }

        public override void OnOpen()
        {
            isOpend = true;

            clientCallback = new CallbackHandler(this);
            context = new InstanceContext(clientCallback);
            client = new StockQuoteServiceReference.StockQuoteServiceClient(context);
            client.StartSendingQuotes();
        }

        public override void OnMessage(string token)
        {
        }

        protected override void OnClose()
        {
            isOpend = false;

            client.Abort();
            context.Abort(); 

            base.OnClose();
        }

        protected override void OnError()
        {
            isOpend = false;
            base.OnError();
        }

        public async Task SendMessage(string message)
        {
            if(isOpend) await this.Send(message);
        }
    }

    public class SesionSocketFactory : ServiceHostFactory
    {
        protected override ServiceHost CreateServiceHost(Type serviceType, Uri[] baseAddresses)
        {
            WebSocketHost host = new WebSocketHost(serviceType, baseAddresses);
            host.AddWebSocketEndpoint();
            return host;
        }
    }
    
    internal static class SesionSocketRunConsole
    {
        static Binding _binding;
        static WebSocketHost<SessionSocketService> _host;

        internal static void Start(string ip = "")
        {
            //string uri = string.Format("ws://{0}/token", ip);
            string uri = "ws://localhost:12345/notify";

            //_host = new WebSocketHost<WebSocketServiceImpl>(new Uri(uri));
            _host = new WebSocketHost<SessionSocketService>(new ServiceThrottlingBehavior()
            {
                MaxConcurrentSessions = int.MaxValue,
                MaxConcurrentCalls = 99,
                MaxConcurrentInstances = 100000
            }, new Uri(uri));

            //_binding = WebSocketHost.CreateWebSocketBinding(false);
            //_binding = WebSocketHost.CreateWebSocketBinding(false, 1024, 1024);
            _binding = WebSocketHost.CreateWebSocketBinding(https: false, sendBufferSize: 2048, receiveBufferSize: 2048);
            _binding.SendTimeout = TimeSpan.FromMilliseconds(500);
            _binding.OpenTimeout = TimeSpan.FromDays(1);

            _host.AddWebSocketEndpoint(_binding);
            _host.Credentials.UseIdentityConfiguration = true;

            _host.Open();
        }

        internal static void Stop()
        {
            _host.Close();
        }
    }
}