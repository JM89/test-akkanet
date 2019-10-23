using Akka.Actor;
using ConsoleApp1.Messages;
using System;
using System.IO;

namespace ConsoleApp1.Actors
{
    class ConsoleReaderActor : UntypedActor
    {
        public const string StartCommand = "start";
        public const string ExitCommand = "exit";

        private readonly IActorRef _fileReader;

        public ConsoleReaderActor(IActorRef fileReader)
        {
            _fileReader = fileReader;
        }

        protected override void OnReceive(object message)
        {
            if (message.Equals(StartCommand))
            {
                Console.WriteLine("Please provide file path:\n");
            }

            if (message.Equals(ExitCommand))
            {
                Context.System.Terminate();
            }

            var filePath = Console.ReadLine();
            if (File.Exists(filePath))
            {
                _fileReader.Tell(new TextFileToProcessMessage(filePath));
            }

            // TODO: do something else?
        }
    }

}
