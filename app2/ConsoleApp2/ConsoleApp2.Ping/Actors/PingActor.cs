using Akka.Actor;
using System;

namespace ConsoleApp2.Ping.Actors
{
    public class PingActor : UntypedActor
    {
        public const string StartCommand = "start";
        private int internalCounter = 0;

        private readonly IActorRef _pongRouteServer;

        public PingActor(IActorRef pongRouteServer)
        {
            _pongRouteServer = pongRouteServer;
        }

        protected override void OnReceive(object message)
        {
            if (message.Equals(StartCommand))
            {
                var msg = new Messages.Message(Guid.NewGuid(), "First Message");
                _pongRouteServer.Tell(msg);
            }

            if (internalCounter++ == 10)
            {
                _pongRouteServer.Tell("exit");
            }

            if (message is Messages.Message)
            {
                var msg = message as Messages.Message;
                Console.WriteLine($"Message {msg.Id} received: {msg.Content}");

                msg = new Messages.Message(Guid.NewGuid(), "Ping");
                _pongRouteServer.Tell(msg);
            }
        }
    }
}
