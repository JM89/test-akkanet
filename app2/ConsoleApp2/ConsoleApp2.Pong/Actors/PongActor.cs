using Akka.Actor;
using System;

namespace ConsoleApp2.Pong.Actors
{
    public class PongActor : UntypedActor
    {
        public const string ExitCommand = "exit";

        private readonly IActorRef _pingRouteServer;

        public PongActor(IActorRef pingRouteServer)
        {
            _pingRouteServer = pingRouteServer;
        }

        protected override void OnReceive(object message)
        {
            if(message.Equals(ExitCommand))
            {
                Context.System.Terminate();
            }

            if (message is Messages.Message)
            {
                var msg = message as Messages.Message;
                Console.WriteLine($"Message {msg.Id} received: {msg.Content}");

                msg = new Messages.Message(Guid.NewGuid(), "Pong");
                _pingRouteServer.Tell(msg);
            }
        }
    }
}
