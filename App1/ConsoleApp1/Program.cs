using Akka.Actor;
using ConsoleApp1.Actors;
using System;

namespace ConsoleApp1
{
    class Program
    {
        public static ActorSystem MyActorSystem;
        static void Main(string[] args)
        {
            // make actor system 
            MyActorSystem = ActorSystem.Create("MyActorSystem");

            // Create my top level actors
            var consoleWriterActor = MyActorSystem.ActorOf(Props.Create<ConsoleWriterActor>(), "cw");
            var currencyCheckerActor = MyActorSystem.ActorOf(Props.Create<CurrencyCheckerActor>(consoleWriterActor), "cc");
            var consoleReaderActor = MyActorSystem.ActorOf(Props.Create<ConsoleReaderActor>(currencyCheckerActor, consoleWriterActor), "cr");

            // Start to read the console
            consoleReaderActor.Tell(ConsoleReaderActor.StartCommand);

            MyActorSystem.WhenTerminated.Wait();
        }
    }
}
