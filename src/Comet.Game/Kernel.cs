using Comet.Network.Services;
using System.Collections.Generic;
using System.Runtime.Caching;
using System.Threading.Tasks;
using Comet.Game.Database;
using Comet.Shared;
using System.Reflection;
using System.Linq;
using System;
using Comet.Game.World;
using Comet.Game.World.Managers;
using Comet.Game.World.Schedule;
using Comet.Game.World.Threading;
using System.Threading;
using Comet.Game.Internal.Auth;
using System.Collections.Concurrent;
using Comet.Game.Internal.AI;
using Comet.Network.Packets;

namespace Comet.Game
{
    /// <summary>
    /// Kernel for the server, acting as a central core for pools of models and states
    /// initialized by the server. Used in database repositories to load data into memory
    /// from essential tables or tables which require heavy post-processing. Used in the
    /// server packet process methods for tracking client and world states.
    /// </summary>
    public static class Kernel
    {
        public static readonly string Version;

        static Kernel()
        {
            Version = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "Error";
        }

        // State caches
        public readonly static MemoryCache Logins = MemoryCache.Default;
        public readonly static List<uint> Registration = new();

        #region Login
        /// <summary>
        /// The account server client object.
        /// </summary>
        public static AccountServer AccountServer;
        /// <summary>
        /// The account server client socket.
        /// </summary>
        public static AccountClient AccountClient;
        #endregion

        #region AI

        public readonly static ConcurrentDictionary<string, AiClient> AiClients = new ConcurrentDictionary<string, AiClient>();
        
        public static async Task BroadcastWorldMsgAsync(IPacket msg)
        {
            foreach (var server in AiClients.Values)
            {
                await server.SendAsync(msg);
            }
        }

        #endregion

        public static ServerConfiguration.GameNetworkConfiguration GameConfiguration => Configuration.GameNetwork;
        public static ServerConfiguration Configuration { get; set; }

        public readonly static NetworkMonitor NetworkMonitor = new ();

        public readonly static SystemProcessor SystemThread = new ();
        public readonly static UserProcessing UserThread = new ();
        public readonly static EventProcessing EventThread = new ();
        public readonly static RoleProcessing RoleThread = new ();

        private static SchedulerFactory schedulerFactory { get; set; }

        // Background services
        public static class Services
        {
            public readonly static RandomnessService Randomness = new ();
#if DEBUG
            public readonly static ServerProcessor Processor = new ();
#else
            public readonly static ServerProcessor Processor = new ();
#endif
        }

        /// <summary>
        /// Returns the next random number from the generator.
        /// </summary>
        /// <param name="maxValue">One greater than the greatest legal return value.</param>
        public static Task<int> NextAsync(int maxValue)
        {
            return NextAsync(0, maxValue);
        }

        public static Task<double> NextRateAsync(double range)
        {
            return Services.Randomness.NextRateAsync(range);
        }

        /// <summary>Writes random numbers from the generator to a buffer.</summary>
        /// <param name="buffer">Buffer to write bytes to.</param>
        public static Task NextBytesAsync(byte[] buffer) =>
            Services.Randomness.NextBytesAsync(buffer);

        /// <summary>
        ///     Returns the next random number from the generator.
        /// </summary>
        /// <param name="minValue">The least legal value for the Random number.</param>
        /// <param name="maxValue">One greater than the greatest legal return value.</param>
        public static Task<int> NextAsync(int minValue, int maxValue)
        {
            return Services.Randomness.NextAsync(minValue, maxValue);
        }

        public static async Task<bool> ChanceCalcAsync(int chance, int outOf)
        {
            return await NextAsync(outOf) < chance;
        }

        public static async Task<bool> StartupAsync()
        {
            try
            {
                await MapManager.LoadDataAsync().ConfigureAwait(true);
                await MapManager.LoadMapsAsync().ConfigureAwait(true);

                await MagicManager.InitializeAsync();
                await PeerageManager.InitializeAsync();
                await EventManager.InitializeAsync();
                await RoleManager.InitializeAsync();
                await ItemManager.InitializeAsync();
                await PigeonManager.InitializeAsync();
                await PeerageManager.InitializeAsync();
                await TutorManager.InitializeAsync();
                await SyndicateManager.InitializeAsync();
                await FamilyManager.InitializeAsync();
                await FlowerManager.InitializeAsync();
                await MineManager.InitializeAsync();
                await ExperienceManager.InitializeAsync();

                await SystemThread.StartAsync();
                await UserThread.StartAsync();
                await EventThread.StartAsync();
                await RoleThread.StartAsync();

                schedulerFactory = new SchedulerFactory();
                await schedulerFactory.StartAsync();
                await schedulerFactory.ScheduleAsync<AutomaticActionJob>("0 * * * * ?");
                await schedulerFactory.ScheduleAsync<DailyResetJob>("0 0 0 * * ?");
                return true;
            }
            catch (Exception ex)
            {
                await Log.WriteLogAsync(LogLevel.Exception, ex.ToString());
                return false;
            }
        }

        public static async Task<bool> CloseAsync()
        {
            await schedulerFactory.StopAsync();

            await UserThread.CloseAsync();
            await EventThread.CloseAsync();
            await RoleThread.CloseAsync();

            await Services.Processor.StopAsync(new CancellationToken(true)).ConfigureAwait(true);
            await RoleManager.KickOutAllAsync("Server is now closing", true).ConfigureAwait(true);

            for (int i = 0; i < 5; i++)
            {
                await Log.WriteLogAsync(LogLevel.Info, $"Server will shutdown in {5 - i} seconds...");
                await Task.Delay(1000);
            }
            await SystemThread.CloseAsync();
            return true;
        }

        /// <summary>
        /// Calculates the chance of success based in a rate.
        /// </summary>
        /// <param name="chance">Rate in percent.</param>
        /// <returns>True if the rate is successful.</returns>
        public static async Task<bool> ChanceCalcAsync(double chance)
        {
            const int DIVISOR_I = 10_000_000;
            const int MAX_VALUE = 100 * DIVISOR_I;
            try
            {
                return await NextAsync(0, MAX_VALUE) <= chance * DIVISOR_I;
            }
            catch (Exception ex)
            {
                await Log.WriteLogAsync(LogLevel.Error, $"Chance Calc error!");
                await Log.WriteLogAsync(LogLevel.Exception, ex.ToString());
                return false;
            }
        }

        public static bool IsValidName(string szName)
        {
            foreach (var c in szName)
            {
                if (c < ' ')
                    return false;
                switch (c)
                {
                    case ' ':
                    case ';':
                    case ',':
                    case '/':
                    case '\\':
                    case '=':
                    case '%':
                    case '@':
                    case '\'':
                    case '"':
                    case '[':
                    case ']':
                    case '?':
                    case '{':
                    case '}':
                        return false;
                }
            }

            string lower = szName.ToLower();
            return _invalidNameChar.All(part => !lower.Contains(part));
        }

        private static readonly string[] _invalidNameChar =
        {
            "{", "}", "[", "]", "(", ")", "\"", "[gm]", "[pm]", "'", "´", "`", "admin", "helpdesk", " ",
            "bitch", "puta", "whore", "ass", "fuck", "cunt", "fdp", "porra", "poha", "caralho", "caraio"
        };
    }
}