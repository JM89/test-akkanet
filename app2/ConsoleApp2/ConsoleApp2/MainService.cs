using Akka.Actor;
using Akka.Bootstrap.Docker;
using ConsoleApp2.Messages.Helpers;
using System.Threading.Tasks;
using static Akka.Actor.CoordinatedShutdown;

namespace ConsoleApp2
{
    public class MainService
    {
        protected ActorSystem ClusterSystem;

        public Task WhenTerminated => ClusterSystem.WhenTerminated;


        public bool Start()
        {
            var config = HoconLoader.ParseConfig("config.hocon");
            ClusterSystem = ActorSystem.Create("mycluster", config.BootstrapFromDocker());
            return true;
        }

        public Task Stop()
        {
            return Get(ClusterSystem).Run(ClrExitReason.Instance);
        }
    }
}
