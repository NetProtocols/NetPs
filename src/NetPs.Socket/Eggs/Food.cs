namespace NetPs.Socket.Eggs
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Reflection;
    using System.Threading;

    public static class Food
    {
        public const char Delimiter = ';';
        public const string Assemblys = "NetPs.Tcp;NetPs.Udp;";
        public static void Heating(IHeatingWatch watch = null)
        {
            Type[] types_ref;
            int i, j;
            if (watch == null) watch = new NoneWatch();
            var ass = Assemblys.Split(Delimiter);

            var queue = new Queue<Thread>();
            queue.Enqueue(run_heat(typeof(Heat_socket), watch));
            for (int ass_i = 0; ass_i < ass.Length; ass_i++)
            {
                Assembly a = null;
                if (string.Empty == ass[ass_i]) continue;
                try { a = Assembly.Load(ass[ass_i]); } catch (FileNotFoundException) { }
                if (a == null) continue;
                watch.Heat_Progress();
                var types = a.GetTypes();
                for (i = types.Length - 1; i >= 0; i--)
                {
                    if (types[i].IsAbstract) continue;
                    else if (!types[i].IsClass) continue;

                    types_ref = types[i].GetInterfaces();
                    for (j = types_ref.Length - 1; j   >= 0; j--)
                    {
                        if (types_ref[j] == typeof(IHeat))
                        {
                            queue.Enqueue(run_heat(types[i], watch));
                        }
                    }
                }
            }

            while (queue.Count > 0) queue.Dequeue().Join();

            new Thread(new ThreadStart(watch.Heat_End)).Start();
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
