using System;
using System.ServiceModel.Activation;
using System.Web.Routing;

namespace IISNotify
{
    public class Global : System.Web.HttpApplication
    {

        protected void Application_Start(object sender, EventArgs e)
        {
            //RouteTable.Routes.Add(new ServiceRoute("token", new SesionSocketFactory(), typeof(SessionSocketService)));

            // Dynamically create new timer
            System.Timers.Timer timScheduledTask = new System.Timers.Timer();
            // Timer interval is set in miliseconds,
            // In this case, we'll run a task every minute
            //timScheduledTask.Interval = 60 * 1000;
            timScheduledTask.Interval = _CONST.TIME_OUT_TOKEN;
            timScheduledTask.Enabled = true;
            // Add handler for Elapsed event
            timScheduledTask.Elapsed += new System.Timers.ElapsedEventHandler(timScheduledTask_Elapsed);
            //timScheduledTask.Stop();
            timScheduledTask.Start();
        }

        void timScheduledTask_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            //SessionSocketService.AutoCheckForTimer();
        }

        protected void Session_Start(object sender, EventArgs e)
        {

        }

        protected void Application_BeginRequest(object sender, EventArgs e)
        {

        }

        protected void Application_AuthenticateRequest(object sender, EventArgs e)
        {

        }

        protected void Application_Error(object sender, EventArgs e)
        {

        }

        protected void Session_End(object sender, EventArgs e)
        {

        }

        protected void Application_End(object sender, EventArgs e)
        {

        }
    }
}