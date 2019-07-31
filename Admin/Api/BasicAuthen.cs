 
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Principal;
using System.Threading;
using System.Web;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;

namespace Admin
{
    public class BasicAuthenticationAttribute : AuthorizationFilterAttribute
    {
        readonly IDataflow _api;
        public BasicAuthenticationAttribute(IDataflow api) : base() {
            _api = api;
        }

        public override void OnAuthorization(HttpActionContext actionContext)
        {
            string sessionid = string.Empty;
            string path = actionContext.Request.RequestUri.AbsolutePath;

            IEnumerable<string> values;
            actionContext.Request.Headers.TryGetValues("sessionid",out values);
            if (values == null || values.Count() == 0)
            {
                string query = actionContext.Request.RequestUri.Query;
                var nvc = System.Web.HttpUtility.ParseQueryString(query);
                sessionid = nvc["sessionid"];
            }
            else
            {
                sessionid = values.ToArray()[0];
            }

            if (string.IsNullOrEmpty(sessionid)) {
                actionContext.Response = actionContext.Request.CreateResponse(HttpStatusCode.Unauthorized);
                return;
            }

            if (path == "/api/login")
            {
                actionContext.ActionArguments.Add("IWebApi", _api);
                actionContext.ActionArguments.Add("EncryptKey", Dataflow._ENCRYPT_KEY_NOT_LOGIN);
                // setting current principle
                Thread.CurrentPrincipal = new GenericPrincipal(new GenericIdentity(sessionid), null);

                return;
            }

            int encryptKey = _api.f_user_Authentication(sessionid);
            if (encryptKey == 0)
            {
                actionContext.Response = actionContext.Request.CreateResponse(HttpStatusCode.Unauthorized);
                return;
            }

            actionContext.ActionArguments.Add("IWebApi", _api);
            actionContext.ActionArguments.Add("EncryptKey", encryptKey);
            // setting current principle
            Thread.CurrentPrincipal = new GenericPrincipal(new GenericIdentity(sessionid), null);
        }
    }
}