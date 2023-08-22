namespace NetPs.Socket.Eggs
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using System.Threading.Tasks;

    public static class Food
    {
        public const char Delimiter = ';';
        public const string Assemblys = "NetPs.Socket;NetPs.Tcp;NetPs.Udp;NetPs.Webdav;";
        public static void Heating(IHeatingWatch watch = null)
        {
            if (watch == null) watch = new NoneWatch();
            var ass = Assemblys.Split(Delimiter);
            try
            {
                watch.Heat_Progress();
                var queue = new Queue<TaskAwaiter>();
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
                            watch.Heat_Progress();
                            IHeat heat = Activator.CreateInstance(types[i]) as IHeat;
                            queue.Enqueue(Task.Run(() => heat.Start(watch)).GetAwaiter());
                        }
                    }
                }

                while (queue.Count > 0)
                {
                    var w = queue.Dequeue();
                    while (!w.IsCompleted) w.GetResult();
                }
            }
            catch (Exception e){ throw e; }
            finally
            {
                watch.Heat_End();
            }
        }

        private class NoneWatch : IHeatingWatch
        {
            public void Heat_End() { }
            public void Heat_Progress() { }
        }
    }
}
