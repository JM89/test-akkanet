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
            var consoleWriterActor = MyActorSystem.ActorOf(Props.Create<ConsoleWriterActor>(), "console_writer");
            var currencyCheckerActor = MyActorSystem.ActorOf(Props.Create<CurrencyCheckerActor>(consoleWriterActor), "currency_checker");
            var fileReaderActor = MyActorSystem.ActorOf(Props.Create<FileReaderActor>(currencyCheckerActor), "file_reader");
            var consoleReaderActor = MyActorSystem.ActorOf(Props.Create<ConsoleReaderActor>(fileReaderActor), "console_reader");

            // Start to read the console
            consoleReaderActor.Tell(ConsoleReaderActor.StartCommand);

            MyActorSystem.WhenTerminated.Wait();
        }
    }
}
