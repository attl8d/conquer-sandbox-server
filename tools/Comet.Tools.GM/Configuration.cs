using Comet.Database;
using Microsoft.Extensions.Configuration;

namespace Comet.Tools.GM
{
    internal class ServerConfiguration
    {
        public bool Valid => Database != null;

        public DatabaseConfiguration Database { get; set; }

        public ServerConfiguration()
        {
            new ConfigurationBuilder()
                .AddJsonFile("Comet.Tools.json")
                .Build()
                .Bind(this);
        }
    }
}