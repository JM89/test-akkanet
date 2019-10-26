using Akka.Actor;
using ConsoleApp2.Pong;
using System;

namespace ConsoleApp2.Ping
{
    class Program
    {
        protected static ActorSystem ClusterSystem;

        static void Main(string[] args)
        {
            var svc = new PingService();
            svc.Start();

            Console.CancelKeyPress += (sender, eventArgs) =>
            {
                svc.Stop();
                eventArgs.Cancel = true;
            };

            svc.WhenTerminated.Wait();
        }
    }
}
