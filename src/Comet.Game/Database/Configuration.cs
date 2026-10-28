using Comet.Database;
using Microsoft.Extensions.Configuration;

namespace Comet.Game.Database
{
    /// <summary>
    ///     Defines the configuration file structure for the Game Server. App Configuration
    ///     files are copied to the build output directory on successful build, containing all
    ///     default configuration settings for the server, only if the file is newer than the
    ///     file bring replaced.
    /// </summary>
    public class ServerConfiguration
    {
        // Properties and fields
        public DatabaseConfiguration Database { get; set; }
        public GameNetworkConfiguration GameNetwork { get; set; }
        public RpcNetworkConfiguration RpcNetwork { get; set; }
        public AiNetworkConfiguration AiNetwork { get; set; }
        public AuthenticationConfiguration Authentication { get; set; }
        public LabelHookConfiguration LabelHook { get; set; }

        /// <summary>
        ///     Encapsulates network configuration for the game server listener.
        /// </summary>
        public class GameNetworkConfiguration
        {
            public string IPAddress { get; set; }
            public int Port { get; set; }
            public int MaxConn { get; set; }
            public string ServerName { get; set; }
            public uint ServerIdentity { get; set; }
            public string Username { get; set; }
            public string Password { get; set; }
        }

        /// <summary>
        ///     Encapsulates network configuration for the RPC server listener.
        /// </summary>
        public class RpcNetworkConfiguration
        {
            public string IPAddress { get; set; }
            public int Port { get; set; }
        }

        /// <summary>
        ///     Encapsulates network configuration for the RPC server listener.
        /// </summary>
        public class AiNetworkConfiguration
        {
            public string IPAddress { get; set; }
            public int Port { get; set; }
            public string Username { get; set; }
            public string Password { get; set; }
        }

        /// <summary>
        ///     Encapsulates authentication settings for client authentication between the
        ///     account server and game server.
        /// </summary>
        public class AuthenticationConfiguration
        {
            public bool StrictAuthPass { get; set; }
        }

        /// <summary>
        ///     Training aid for the screen bot: logs true positions and HP of roles near players
        ///     as JSON lines. Optional section; on by default, writing to
        ///     C:\CO\bot-data\truth\server-truth.jsonl every 100 ms.
        /// </summary>
        public class LabelHookConfiguration
        {
            public bool Enabled { get; set; } = true;
            public string File { get; set; }
            public int IntervalMs { get; set; } = 100;
        }

        /// <summary>
        ///     Instantiates a new instance of <see cref="ServerConfiguration" /> with command-line
        ///     arguments from the user and a configuration file for the application. Builds the
        ///     configuration file and binds to this instance of the ServerConfiguration class.
        /// </summary>
        /// <param name="args">Command-line arguments from the user</param>
        public ServerConfiguration(string[] args)
        {
            new ConfigurationBuilder()
                .AddJsonFile("Comet.Game.config")
                .AddCommandLine(args)
                .Build()
                .Bind(this);
        }

        /// <summary>
        ///     Returns true if the server configuration is valid after reading.
        /// </summary>
        public bool Valid =>
            Database != null &&
            GameNetwork != null &&
            RpcNetwork != null &&
            Authentication != null;
    }
}