using System;
using System.Web.Http;

namespace IISNotify
{
    public class testController : ApiController
    {
        public string Get()
        {
            var dic = this.ActionContext.ActionArguments;
            return DateTime.Now.ToString();
        }
    }
}