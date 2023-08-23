namespace NetPs.Socket.Eggs
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Threading;

    public static class Food
    {
        public const char Delimiter = ';';
        public const string Assemblys = "NetPs.Tcp;NetPs.Udp;NetPs.Webdav";
        public static void Heating(IHeatingWatch watch = null)
        {
            if (watch == null) watch = new NoneWatch();
            var ass = Assemblys.Split(Delimiter);
            try
            {
                var queue = new Queue<Thread>();
                queue.Enqueue(run_heat(typeof(Heat_socket), watch));
                for (int ass_i = 0; ass_i < ass.Length; ass_i++)
                {
                    Assembly a = null;
                    if (string.Empty == ass[ass_i]) continue;
                    try { a = Assembly.Load(ass[ass_i]); } catch { }
                    if (a == null) continue;
                    watch.Heat_Progress();
                    var types = a.GetTypes();
                    for (var i = 0; i < types.Length; i++)
                    {
                        if (types[i].IsAbstract) continue;
                        else if (!types[i].IsClass) continue;

                        if (types[i].GetInterfaces().Contains(typeof(IHeat)))
                        {
                            queue.Enqueue(run_heat(types[i], watch));
                        }
                    }
                }

                while (queue.Count > 0) queue.Dequeue().Join();
            }
            catch (Exception e){ throw e; }
            finally
            {
                new Thread(new ThreadStart(watch.Heat_End)).Start();
            }
        }

        private static Thread run_heat(Type type, IHeatingWatch watch)
        {
            watch.Heat_Progress();
            IHeat heat = Activator.CreateInstance(type) as IHeat;
            var thread = new Thread(new ThreadStart(() => heat.Start(watch)));
            thread.IsBackground = true;
            thread.Priority = ThreadPriority.Lowest;
            thread.Start();
            return thread;
        }

        private class NoneWatch : IHeatingWatch
        {
            public void Heat_End() { }
            public void Heat_Progress() { }
        }
    }
}
