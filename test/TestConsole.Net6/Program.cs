using NetPs.Tcp;

namespace TestConsole.Net6
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var host = new TcpServer((s, c) =>
            {
                c.StartMirror("172.17.0.161:5244");
            });
            host.Run("0.0.0.0:5244");
            while (true)
            {
                Console.ReadLine();
                Console.WriteLine(TcpRxRepeater.i);
                Console.WriteLine(TcpRxRepeater.j);
            }
        }
    }
}