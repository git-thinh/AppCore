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
        public JobTest() { }

        public JobTest([Unity.Dependency("IDataflow")]IDataflow dataflow) : base()
        {
            _dataflow = dataflow;
        }

        public void Execute(string data)
        {

        }
    }

    // Somewhere in the code
    //BackgroundJob.Enqueue<JobEmail>(x => x.Send(1, "Hello, world!"));
}
