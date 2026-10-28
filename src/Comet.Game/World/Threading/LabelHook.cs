using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Comet.Game.Database;
using Comet.Game.States;
using Comet.Game.States.Items;
using Comet.Game.States.Npcs;
using Comet.Game.World.Managers;
using Comet.Game.World.Maps;
using Comet.Shared;

namespace Comet.Game.World.Threading
{
    /// <summary>
    ///     Training aid for the screen bot (attl8d/conquer-bot). Several times a second it
    ///     appends one JSON line per map that has players on it, listing every role near a
    ///     player: where it really is, its HP, what it is. The bot's recorder copies these
    ///     lines next to its screen recording, so the detector can be trained on free labels.
    ///     The bot never reads this while it plays. Read-only: it changes nothing in the game.
    /// </summary>
    public class LabelHook : TimerBase
    {
        public const int FORMAT_VERSION = 1;

        // A little past the client's view so roles walking onto the screen are already listed
        private const int RANGE = Screen.VIEW_SIZE + 2;

        private static readonly long UnixEpochTicks = DateTime.UnixEpoch.Ticks;

        private readonly string mPath;
        private StreamWriter mWriter;

        public LabelHook(string path, int intervalMs)
            : base(intervalMs, "Label Hook")
        {
            mPath = path;
        }

        public static bool Enabled => Kernel.Configuration?.LabelHook?.Enabled ?? true;

        public static LabelHook Create()
        {
            ServerConfiguration.LabelHookConfiguration cfg = Kernel.Configuration?.LabelHook;
            string path = cfg?.File;
            if (string.IsNullOrWhiteSpace(path))
                path = OperatingSystem.IsWindows()
                           ? @"C:\CO\bot-data\truth\server-truth.jsonl"
                           : Path.Combine("bot-data", "truth", "server-truth.jsonl");
            int interval = cfg?.IntervalMs > 0 ? cfg.IntervalMs : 100;
            return new LabelHook(path, interval);
        }

        /// <summary>Unix time in nanoseconds, the same clock the recorder stamps frames with.</summary>
        public static long UnixNanos()
        {
            return (DateTime.UtcNow.Ticks - UnixEpochTicks) * 100;
        }

        protected override async Task OnStartAsync()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(mPath)) ?? ".");
            // Shared read so the recorder can follow the file while we write it
            var stream = new FileStream(mPath, FileMode.Append, FileAccess.Write, FileShare.ReadWrite);
            mWriter = new StreamWriter(stream, new UTF8Encoding(false)) {AutoFlush = true, NewLine = "\n"};
            await Log.WriteLogAsync(LogLevel.Info, $"Label hook writing to {Path.GetFullPath(mPath)}");
            await base.OnStartAsync();
        }

        protected override async Task<bool> OnElapseAsync()
        {
            if (mWriter == null)
                return true;

            foreach (IGrouping<uint, Character> group in RoleManager.QueryUserSet()
                                                                    .GroupBy(x => x.MapIdentity))
            {
                GameMap map = MapManager.GetMap(group.Key);
                if (map == null)
                    continue;

                List<Character> users = group.ToList();
                var roles = new List<Dictionary<string, object>>();
                foreach (Role role in map.QueryRoles())
                {
                    try
                    {
                        if (users.Any(u => Calculations.GetDistance(u.MapX, u.MapY, role.MapX, role.MapY) <= RANGE))
                            roles.Add(Describe(role));
                    }
                    catch
                    {
                        // A role being removed mid-read: skip it this tick
                    }
                }

                var line = new Dictionary<string, object>
                {
                    ["t"] = UnixNanos(),
                    ["v"] = FORMAT_VERSION,
                    ["kind"] = "snap",
                    ["map"] = group.Key,
                    ["map_doc"] = map.MapDoc,
                    ["users"] = users.Select(u => u.Identity).ToArray(),
                    ["roles"] = roles
                };
                await mWriter.WriteLineAsync(JsonSerializer.Serialize(line));
            }

            return true;
        }

        private static Dictionary<string, object> Describe(Role role)
        {
            var d = new Dictionary<string, object>
            {
                ["id"] = role.Identity,
                ["cls"] = ClassOf(role),
                ["name"] = role.Name,
                ["mesh"] = role.Mesh,
                ["x"] = role.MapX,
                ["y"] = role.MapY,
                ["dir"] = (int) role.Direction
            };
            switch (role)
            {
                case Monster monster:
                    d["type"] = monster.Type;
                    d["lvl"] = monster.Level;
                    d["hp"] = monster.Life;
                    d["max_hp"] = monster.MaxLife;
                    d["alive"] = monster.IsAlive;
                    break;
                case Character user:
                    d["lvl"] = user.Level;
                    d["hp"] = user.Life;
                    d["max_hp"] = user.MaxLife;
                    d["alive"] = user.IsAlive;
                    break;
                case DynamicNpc dyna:
                    d["hp"] = dyna.Life;
                    d["max_hp"] = dyna.MaxLife;
                    break;
            }

            return d;
        }

        private static string ClassOf(Role role)
        {
            return role switch
            {
                Character => "player",
                Monster m when m.IsCallPet() => "pet",
                Monster m when m.IsGuard() => "guard",
                Monster => "monster",
                DynamicNpc => "dyna_npc",
                BaseNpc => "npc",
                MapItem => "item",
                _ => role.GetType().Name.ToLowerInvariant()
            };
        }

        protected override async Task OnCloseAsync()
        {
            if (mWriter != null)
                await mWriter.DisposeAsync();
            mWriter = null;
            await base.OnCloseAsync();
        }
    }
}
