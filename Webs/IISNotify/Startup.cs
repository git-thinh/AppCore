using Microsoft.Owin;
using Newtonsoft.Json;
using Owin;
using System.Linq;
using System.Web.Http;

[assembly: OwinStartup(typeof(IISNotify.Startup))]

namespace IISNotify
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            HttpConfiguration config = new HttpConfiguration();
            //--------------------------------------------------------------
            // Web API configuration and services
            var appXmlType = config.Formatters.XmlFormatter.SupportedMediaTypes.FirstOrDefault(t => t.MediaType == "application/xml");
            config.Formatters.XmlFormatter.SupportedMediaTypes.Remove(appXmlType);
            config.Formatters.JsonFormatter.SerializerSettings = new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore };

            //--------------------------------------------------------------
            // Routing staic
            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{action}",
                defaults: new { action = RouteParameter.Optional }
            );
            app.UseWebApi(config);
        }
    }
}
