using Akka.Actor;
using ConsoleApp1.Messages;
using System;

namespace ConsoleApp1.Actors
{
    class ConsoleWriterActor : UntypedActor
    {
        protected override void OnReceive(object message)
        {
            if (message is ResultErrorMessage)
            {
                var msg = message as ResultErrorMessage;
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(msg.Reason);
            }
            else if (message is ResultSuccessfulMessage)
            {
                var msg = message as ResultSuccessfulMessage;
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine(msg.Content);
            }
            else
            {
                Console.WriteLine(message);
            }

            Console.ResetColor();
        }
    }
}
