using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Admin
{
    public interface IDataflow
    {
        string test1(string para);
    }

    public class Dataflow: IDataflow
    {
        public string test1(string para) => DateTime.Now.ToString();
    }
}