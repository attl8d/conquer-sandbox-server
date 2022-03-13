using Comet.Core.World;
using Comet.Database.Entities;
using Comet.Game.Database.Repositories;
using Comet.Game.World.Maps;
using Comet.Shared;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Comet.Game.World.Managers
{
    public sealed class MapManager
    {
        public static ConcurrentDictionary<uint, GameMap> GameMaps { get; } = new ConcurrentDictionary<uint, GameMap>();

        public static async Task LoadMapsAsync()
        {
            await MapDataManager.LoadDataAsync().ConfigureAwait(true);

            List<DbMap> maps = await MapsRepository.GetAsync();
            foreach (var dbmap in maps)
            {
                GameMap map = new GameMap(dbmap);
                if (await map.InitializeAsync())
                {
#if DEBUG
                    await Log.WriteLogAsync(LogLevel.Debug, $"Map[{map.Identity:000000}] MapDoc[{map.MapDoc:0000}] {map.Name,-32} Partition: {map.Partition:00} loaded...");
#endif
                    GameMaps.TryAdd(map.Identity, map);
                }
                else
                {
                    await Log.WriteLogAsync(LogLevel.Error, $"Could not load ");
                }
            }

            List<DbDynamap> dynaMaps = await MapsRepository.GetDynaAsync();
            foreach (var dbmap in dynaMaps)
            {
                GameMap map = new GameMap(dbmap);
                if (await map.InitializeAsync())
                {
#if DEBUG
                    await Log.WriteLogAsync(LogLevel.Debug, $"Map[{map.Identity:000000}] MapDoc[{map.MapDoc:0000}] {map.Name,-32} Partition: {map.Partition:00} loaded...");
#endif
                    GameMaps.TryAdd(map.Identity, map);
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