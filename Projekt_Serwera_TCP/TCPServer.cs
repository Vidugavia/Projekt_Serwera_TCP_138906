using Oprogramowanie_Serwera_TCP;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Projekt_Serwera_TCP
{
    class Program
    {
        static object obj = new object();
        static void raidWrite(ConsoleColor color)
        {
            lock (obj) {
                Console.ForegroundColor = color;
                Console.WriteLine("Test!");
                Console.ResetColor();
            }
        }


        static void Main(string[] args)
        {            
            ThreadPool.QueueUserWorkItem(callBack,ConsoleColor.Red);
            ThreadPool.QueueUserWorkItem(callBack,ConsoleColor.Blue);

            PassGenServer server = new PassGenServer(IPAddress.Parse("127.0.0.1"),8000);
            server.Start();
        }

        private static void callBack(object state)
        {
            while (true)
            {
                raidWrite((ConsoleColor)state);
            }
            //throw new NotImplementedException();
        }
    }
}
