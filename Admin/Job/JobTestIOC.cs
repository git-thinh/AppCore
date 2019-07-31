using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks; 

namespace Admin
{
    public class JobTestIOC
    {
        readonly IDataflow _dataflow = null; 
        public JobTestIOC() { }

        public JobTestIOC([Unity.Dependency("IDataflow")]IDataflow dataflow)
        {
            _dataflow = dataflow; 
        }

        public void Execute(string message)
        {
        }
    }
}
