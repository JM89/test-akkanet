using Akka.Actor;
using Akka.Bootstrap.Docker;
using Akka.Routing;
using ConsoleApp2.Messages.Helpers;
using ConsoleApp2.Pong.Actors;
using System.Threading.Tasks;
using static Akka.Actor.CoordinatedShutdown;

namespace ConsoleApp2.Pong
{
    public class PongService
    {
        protected ActorSystem ClusterSystem;

        public Task WhenTerminated => ClusterSystem.WhenTerminated;

        public bool Start()
        {
            var config = HoconLoader.ParseConfig("config.hocon");
            ClusterSystem = ActorSystem.Create("mycluster", config.BootstrapFromDocker());
            var router = ClusterSystem.ActorOf(Props.Empty.WithRouter(FromConfig.Instance), "workers");
            ClusterSystem.ActorOf(Props.Create(() => new PongActor(router)), "pong");
            return true;
        }

        public Task Stop()
        {
            return Get(ClusterSystem).Run(ClrExitReason.Instance);
        }
    }
}
