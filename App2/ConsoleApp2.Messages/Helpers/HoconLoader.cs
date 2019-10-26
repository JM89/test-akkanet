using Akka.Configuration;
using System.IO;

namespace ConsoleApp2.Messages.Helpers
{
    public static class HoconLoader
    {
        public static Config ParseConfig(string hoconPath)
        {
            return ConfigurationFactory.ParseString(File.ReadAllText(hoconPath));
        }
    }
}
