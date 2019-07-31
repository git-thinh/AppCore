using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Admin
{
    public class JobTest
    {
        readonly IDataflow _dataflow;
        readonly INotifyService _notify;
        public JobTest(IDataflow dataflow, INotifyService notify) { _dataflow = dataflow; _notify = notify; }

        public void Execute(string data)
        {
            string s = _dataflow.test1("");
            Console.WriteLine("TEST_JOB: " + s);
            _notify.Push(s);
        }
    }

    // Somewhere in the code
    //BackgroundJob.Enqueue<JobEmail>(x => x.Send(1, "Hello, world!"));
}
