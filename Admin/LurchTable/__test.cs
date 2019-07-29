using CSharpTest.Net.Collections;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Admin.LurchTable
{
    class __test
    {
        void run()
        {
            //LurchTable Example

            //Example producer/consumer queue where producer threads help when queue is full
            using (var queue = new LurchTable<string, int>(LurchTableOrder.Insertion, 100))
            {
                var stop = new ManualResetEvent(false);
                //queue.ItemRemoved += kv => Console.WriteLine("[{0}] - {1}", Thread.CurrentThread.ManagedThreadId, kv.Key);
                //start some threads eating queue:
                var thread = new Thread(() => {
                    while (!stop.WaitOne(0))
                    {
                        queue.Dequeue();
                    }
                }) {
                    Name = "worker",
                    IsBackground = true
                };
                thread.Start();

                var names = Directory.GetFiles(Path.GetTempPath(), "*", SearchOption.AllDirectories);
                if (names.Length <= 100) throw new Exception("Not enough trash in your temp dir.");

                var loops = Math.Max(1, 10000 / names.Length);

                for (int i = 0; i < loops; i++)
                {
                    foreach (var name in names)
                    {
                        queue[name] = i;
                    }
                }

                stop.Set();
                thread.Join();
            }
        }

    }
}
