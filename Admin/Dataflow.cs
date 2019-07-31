using System;

namespace Admin
{
    public interface IDataflow
    {
        string test1(string para);
    }

    public class Dataflow: IDataflow
    {
        public string test1(string para) => DateTime.Now.ToString();

        ////public void Setup() {
        ////    JobStorage storage = new MemoryStorage(new MemoryStorageOptions());
        ////    LogProvider.SetCurrentLogProvider(new ColouredConsoleLogProvider());
        ////    var serverOptions = new BackgroundJobServerOptions() { ShutdownTimeout = TimeSpan.FromSeconds(5) };

        ////    using (var server = new BackgroundJobServer(serverOptions, storage))
        ////    {
        ////        Log("Hangfire Server started. Press any key to exit...", LogLevel.Fatal);

        ////        JobStorage.Current = storage;
        ////        BackgroundJob.Enqueue(() => Log("Hello Hangfire!", LogLevel.Error));

        ////        foreach (var job in _jobsToRetriesMap)
        ////        {
        ////            Log($"Scheduling job ID :{job.Key} in {job.Key} seconds..", LogLevel.Warn);
        ////            BackgroundJob.Schedule(() => DoJob("Hi!", job.Key), TimeSpan.FromSeconds(job.Key));
        ////        }

        ////        System.Console.ReadKey();
        ////        Log("Stopping server...", LogLevel.Fatal);
        ////    }
        ////}

        ////public static void DoJob(string jobMsg, int id)
        ////{
        ////    Log($"Processing Job -> {jobMsg} : {id}  ..", LogLevel.Info);
        ////    Log($"Job DONE! -> {jobMsg} : {id}  ..", LogLevel.Warn);
        ////}

        ////public static void Log(string msg, LogLevel level = LogLevel.Info)
        ////{
        ////    LogProvider.GetLogger("Main").Log(logLevel: level, messageFunc: () => { return msg; });
        ////}


    }
}