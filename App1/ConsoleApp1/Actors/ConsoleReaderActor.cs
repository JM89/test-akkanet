using Akka.Actor;
using ConsoleApp1.Errors;
using ConsoleApp1.Messages;
using System;
using System.IO;

namespace ConsoleApp1.Actors
{
    class ConsoleReaderActor : UntypedActor
    {
        public const string StartCommand = "start";
        public const string ExitCommand = "exit";

        private readonly IActorRef _currencyChecker;
        private readonly IActorRef _consoleWriter;

        public ConsoleReaderActor(IActorRef currencyChecker, IActorRef consoleWriter)
        {
            _currencyChecker = currencyChecker;
            _consoleWriter = consoleWriter;
        }

        protected override void OnReceive(object message)
        {
            if (message.Equals(StartCommand))
            {
                _consoleWriter.Tell(new ResultSuccessfulMessage($"Please provide file directory:"));
            }

            if (message.Equals(ExitCommand))
            {
                Context.System.Terminate();
            }

            var directory = Console.ReadLine();
            if (Directory.Exists(directory))
            {
                foreach (var file in Directory.GetFiles(directory))
                {
                    Context.ActorOf(Props.Create(() => new FileReaderActor(_currencyChecker, file)));
                }
            }
        }

        // here we are overriding the default SupervisorStrategy
        // which is a One-For-One strategy w/ a Restart directive
        protected override SupervisorStrategy SupervisorStrategy()
        {
            return new OneForOneStrategy (
                2, 
                TimeSpan.FromSeconds(30), 
                x =>
                {
                    if (x is IOException)
                    {
                        return Directive.Stop;
                    }
                    else if (x is FileExtensionUnhandled)
                    {
                        _consoleWriter.Tell(new ResultErrorMessage($"File extension unhandled: {x.Message}"));
                        return Directive.Restart;
                    }
                    else
                    {
                        return Directive.Restart;
                    }
                });
        }
    }
}
