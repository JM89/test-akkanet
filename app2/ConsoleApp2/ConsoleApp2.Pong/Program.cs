using Akka.Actor;
using System;

namespace ConsoleApp2.Pong
{
    class Program
    {
        protected static ActorSystem ClusterSystem;

        static void Main(string[] args)
        {
            var svc = new PongService();
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
