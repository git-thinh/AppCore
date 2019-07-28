using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.Threading;
using System.Web;
using Microsoft.ServiceModel.WebSockets;

namespace IISNotify
{
    public class SesionSocketService : WebSocketService
    {
        static ConcurrentDictionary<int, string> _timeOuts = new ConcurrentDictionary<int, string>() { };
        static bool isChecking = false;

        public static string[] get_tokens() {
            return _timeOuts.Values.ToArray();
        }

        public static void AutoCheckForTimer() {
            if (isChecking)
                return;

            isChecking = true;

            int[] times = _timeOuts.Keys.ToArray();
            int _timeNow = int.Parse(DateTime.Now.ToString("HHmmssfff"));

            //int[] expireds = times.Where(x => _timeNow - x >= _CONST.TIME_OUT_TOKEN).ToArray();
            //if (expireds.Length > 0) {
            //    foreach(int key in expireds)
            //    {
            //        string token_del;
            //        _timeOuts.TryRemove(key, out token_del);
            //    }
            //}

            isChecking = false;
        }

        //public static bool Validator(string token) => Validator2(token) != null;

        //static oTokenUser Validator2(string token)
        //{
        //    oTokenUser o = oTokenUser.Validitor(token);
        //    int _timeNow = int.Parse(DateTime.Now.ToString("HHmmssfff"));
        //    if (o != null && (_timeNow - o.DateCreated < _CONST.TIME_OUT_TOKEN))
        //        return o;
        //    return null;
        //}


        public string m_sessionID
        {
            get
            {
                return this.WebSocketContext.SecWebSocketKey;
            }
        }

        public override void OnOpen()
        {
            //Thread.Sleep(1);
            //int time = int.Parse(DateTime.Now.ToString("HHmmssfff"));
            //string token = new oTokenUser() { DateCreated = time, Username = "guest" }.getToken();
            //this.Send(token);
            //_timeOuts.TryAdd(time, token);
        }

        public override void OnMessage(string token)
        {
            //var o = SocketTokenService.Validator2(token);
            //if (o != null)
            //{
            //    string tk;
            //    _timeOuts.TryRemove(o.DateCreated, out tk);

            //    int time_new = int.Parse(DateTime.Now.ToString("HHmmssfff"));
            //    string token_new = new oTokenUser() { DateCreated = time_new, Username = "guest" }.getToken();
            //    _timeOuts.TryAdd(time_new, token_new);

            //    this.Send(token_new);
            //}
        }

        protected override void OnClose()
        {
            base.OnClose();
        }

        protected override void OnError()
        {
            base.OnError();
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