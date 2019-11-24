using Akka.Actor;
using Akka.Bootstrap.Docker;
using Akka.Routing;
using ConsoleApp2.Messages.Helpers;
using ConsoleApp2.Ping.Actors;
using System.Threading.Tasks;
using static Akka.Actor.CoordinatedShutdown;

namespace ConsoleApp2.Pong
{
    public class PingService
    {
        protected ActorSystem ClusterSystem;

        public Task WhenTerminated => ClusterSystem.WhenTerminated;

        public bool Start()
        {
            var config = HoconLoader.ParseConfig("config.hocon");
            ClusterSystem = ActorSystem.Create("mycluster", config.BootstrapFromDocker());
            var router = ClusterSystem.ActorOf(Props.Empty.WithRouter(FromConfig.Instance), "workers");
            var ping = ClusterSystem.ActorOf(Props.Create(() => new PingActor(router)), "ping");

            ping.Tell("start");

            return true;
        }

        public Task Stop()
        {
            return Get(ClusterSystem).Run(ClrExitReason.Instance);
        }
    }
}
