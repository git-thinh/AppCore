using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Microsoft.ServiceModel.WebSockets;

namespace IISNotify
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
}