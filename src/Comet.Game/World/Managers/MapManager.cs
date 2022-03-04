using Comet.Core.World.Maps;
using Comet.Database.Entities;
using Comet.Game.Database.Repositories;
using Comet.Game.World.Maps;
using Comet.Shared;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Comet.Game.World.Managers
{
    public sealed class MapManager
    {
        private static readonly ConcurrentDictionary<uint, GameMapData> m_mapData =
            new ConcurrentDictionary<uint, GameMapData>();

        public static ConcurrentDictionary<uint, GameMap> GameMaps { get; } = new ConcurrentDictionary<uint, GameMap>();

        public static async Task LoadDataAsync()
        {
            var stream = File.OpenRead(string.Format(".{0}ini{0}GameMap.dat", Path.DirectorySeparatorChar));
            BinaryReader reader = new BinaryReader(stream);

            int mapDataCount = reader.ReadInt32();
            await Log.WriteLogAsync(LogLevel.Debug, $"Loading {mapDataCount} maps...");

            for (int i = 0; i < mapDataCount; i++)
            {
                uint idMap = reader.ReadUInt32();
                int length = reader.ReadInt32();
                string name = new string(reader.ReadChars(length));
                uint puzzle = reader.ReadUInt32();

                GameMapData mapData = new GameMapData(idMap);
                if (mapData.Load(name.Replace("\\", Path.DirectorySeparatorChar.ToString())))
                {
#if DEBUG
                    await Log.WriteLogAsync(LogLevel.Info, $"Map [{idMap},{name}] loaded...");
#endif
                    m_mapData.TryAdd(idMap, mapData);
                }
            }

            reader.Close();
            stream.Close();
            reader.Dispose();
            await stream.DisposeAsync();
        }

        public static async Task LoadMapsAsync()
        {
            List<DbMap> maps = await MapsRepository.GetAsync();
            foreach (var dbmap in maps)
            {
                GameMap map = new GameMap(dbmap);
                if (await map.InitializeAsync())
                {
                    GameMaps.TryAdd(map.Identity, map);
                    await Log.GmLogAsync("map_channel", $"{map.Identity}\t{map.Name}\t\t\tPartition: {map.Partition}");
                }
            }

            List<DbDynamap> dynaMaps = await MapsRepository.GetDynaAsync();
            foreach (var dbmap in dynaMaps)
            {
                GameMap map = new GameMap(dbmap);
                if (await map.InitializeAsync())
                {
                    GameMaps.TryAdd(map.Identity, map);
                    await Log.GmLogAsync("map_channel", $"{map.Identity}\t{map.Name}\t\t\tPartition: {map.Partition}");
                }
            }

            foreach (var map in GameMaps.Values)
            {
                await map.LoadTrapsAsync();
            }
        }

        public static GameMap GetMap(uint idMap)
        {
            return GameMaps.TryGetValue(idMap, out var value) ? value : null;
        }

        public static GameMapData GetMapData(uint idDoc)
        {
            return m_mapData.TryGetValue(idDoc, out var map) ? map : null;
        }

        public static async Task<bool> AddMapAsync(GameMap map)
        {
            if (GameMaps.TryAdd(map.Identity, map))
            {
                await map.SendAddToNpcServerAsync();
                return true;
            }
            return false;
        }

        public static async Task<bool> RemoveMapAsync(uint idMap)
        {
            GameMaps.TryRemove(idMap, out var map);
            await map.SendRemoveToNpcServerAsync();
            return true;
        }

        public static async Task OnTimerAsync()
        {
            foreach (var map in GameMaps.Values.Where(x => x.PlayerCount > 0))
            {
                foreach (var mob in map.QueryRoles(x => !x.IsPlayer()))
                {
                    await mob.OnTimerAsync();
                }
            }
        }
    }
}