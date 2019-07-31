using Hangfire;
using Hangfire.Logging;
using Hangfire.Logging.LogProviders;
using Hangfire.MemoryStorage;
using System;
using System.ServiceModel;
using System.ServiceModel.Description;
using Unity;

namespace Admin
{

    class Program
    {
        static void Log(string msg, LogLevel level = LogLevel.Info)
        {
            LogProvider.GetLogger("Main").Log(logLevel: level, messageFunc: () => { return msg; });
        }

        [MTAThread]
        static void Main(string[] args)
        {
            Dataflow dataflow = new Dataflow();
            var notifyService = new StockQuoteService(dataflow);
            //=======================================================================================================
            // Call this before you initialize a new BackgroundJobServer()
            var container = new Unity.UnityContainer();
            GlobalConfiguration.Configuration.UseActivator(new UnityJobActivator(container));
            container.RegisterInstance<IDataflow>(dataflow);
            container.RegisterInstance<INotifyService>(notifyService);
            //=======================================================================================================
            JobStorage storage = new MemoryStorage(new MemoryStorageOptions());
            LogProvider.SetCurrentLogProvider(new ColouredConsoleLogProvider());
            var serverOptions = new BackgroundJobServerOptions() { ShutdownTimeout = TimeSpan.FromSeconds(5) };
            var server = new BackgroundJobServer(serverOptions, storage);
            JobStorage.Current = storage;

            Log("Hangfire Server started. Press any key to exit...", LogLevel.Fatal);
            //=======================================================================================================
            //ServiceHost host = new ServiceHost(typeof(StockQuoteService), new Uri("http://localhost:12345/StockQuoteService.svc"));
            //host.Open();
            var host = new ServiceHost(typeof(StockQuoteService), new Uri("http://localhost:12345/StockQuoteService.svc"));
            //object instanceService = Activator.CreateInstance(typeof(StockQuoteService), dataflow);
            //IServiceBehavior instanceBehavior = Activator.CreateInstance(typeof(StockQuoteServiceBehavior), instanceService) as IServiceBehavior;
            object instanceService = Activator.CreateInstance(typeof(StockQuoteService), dataflow);
            IServiceBehavior instanceBehavior = Activator.CreateInstance(typeof(StockQuoteServiceBehavior), notifyService) as IServiceBehavior;
            host.Description.Behaviors.Add(instanceBehavior);
            host.Open();
            SesionSocketRunConsole.Start();
            //=======================================================================================================

            BackgroundJob.Enqueue<JobTest>(x => x.Execute("Hello, world!"));
            //=======================================================================================================
            Console.WriteLine("Enter to exit ...");
            Console.ReadLine();
        }
    }
}
