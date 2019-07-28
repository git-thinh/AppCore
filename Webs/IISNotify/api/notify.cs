using Libs.IpcChannel;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace IISNotify
{
    public class notifyController : ApiController
    {
        readonly IDataflow _dataflow;
        string _sessionId = string.Empty;
        string _token = string.Empty;

        StreamWriter _streamWriter = null;
        HttpResponseMessage _response = null;

        public notifyController()
        {
        }

        public notifyController(IDataflow dataflow)
        {
            _dataflow = dataflow;
        }

        ~notifyController()
        {
            _streamWriter.Close();
            _streamWriter.Dispose();
        }

        public HttpResponseMessage Get([FromUri]string sessionid = "")
        {
            //this.ActionContext.Response.Headers.Add("Access-Control-Allow-Origin", new[] { "*" });

            //if (string.IsNullOrEmpty(_sessionId))
            //    return new HttpResponseMessage(System.Net.HttpStatusCode.OK) { Content = new StringContent(string.Empty, System.Text.Encoding.UTF8, "text/plain") };

            if (string.IsNullOrEmpty(_sessionId))
            {
                //string header = this.ActionContext.Request.Headers.get_headerValues_forToken();
                //_sessionId = Guid.NewGuid().ToString();
                _sessionId = sessionid;
                _token = Guid.NewGuid().ToString();
                //_token = _dataflow.notify_registerSessionId_returnToken(_sessionId, onIpcSignalMessageEvent, header);
            }

            _response = Request.CreateResponse(HttpStatusCode.OK);
            _response.Content = new PushStreamContent((Action<Stream, HttpContent, TransportContext>)onStreamAvailabe, "text/event-stream");
            return _response;
        }

        void onIpcSignalMessageEvent(object sender, IpcSignalEventArgs e)
        {
            if (e.Arguments[0] == "[CLOSE]")
            {
                _streamWriter.Close();
                _streamWriter.Dispose();
                _response.Dispose();

                //_dataflow.notify_unRegisterSessionId(e.EventName, this.onIpcSignalMessageEvent);

                return;
            }

            string s = string.Join(",", e.Arguments);
            _sendData(s);
        }

        void onStreamAvailabe(Stream stream, HttpContent content, TransportContext context)
        {
            if (_streamWriter == null)
            {
                _streamWriter = new StreamWriter(stream);
                //_sendData(string.Format("{0}{1}", (int)_NOTIFY._100_SESSION_ID, _sessionId));
                _sendData(string.Format("{0}{1}", 100, _token));
            }
        }

        void _sendData(string data)
        {
            try
            {
                //int notifyCode = int.Parse(data.Substring(0, 3));
                //Console.WriteLine(string.Format("===> {0} => {1}", data, Enum.GetName(typeof(_NOTIFY), notifyCode)));

                _streamWriter.WriteLine("data:" + data ?? "");
                _streamWriter.WriteLine();
                _streamWriter.Flush();
            }
            catch { }
        }
    }
}