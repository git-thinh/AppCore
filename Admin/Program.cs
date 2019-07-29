using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.Text;
using System.Threading.Tasks;

namespace Admin
{
    class Program
    {
        static void Main(string[] args)
        {
            Dataflow dataflow = new Dataflow();

            //ServiceHost host = new ServiceHost(typeof(StockQuoteService), new Uri("http://localhost:12345/StockQuoteService.svc"));
            //host.Open();
             
            var host = new ServiceHost(typeof(StockQuoteService), new Uri("http://localhost:12345/StockQuoteService.svc"));
            //object instanceService = Activator.CreateInstance(typeof(StockQuoteService), dataflow);
            //IServiceBehavior instanceBehavior = Activator.CreateInstance(typeof(StockQuoteServiceBehavior), instanceService) as IServiceBehavior;
            object instanceService = Activator.CreateInstance(typeof(StockQuoteService), dataflow);
            IServiceBehavior instanceBehavior = Activator.CreateInstance(typeof(StockQuoteServiceBehavior), new StockQuoteService(dataflow)) as IServiceBehavior;
            host.Description.Behaviors.Add(instanceBehavior);
            host.Open();

            SesionSocketRunConsole.Start();

            Console.WriteLine("Enter to exit ...");
            Console.ReadLine();
        }
    }
}
