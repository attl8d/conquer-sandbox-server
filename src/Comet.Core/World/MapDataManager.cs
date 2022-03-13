using Comet.Core.World.Maps;
using Comet.Shared;
using System.Collections.Concurrent;
using System.IO;
using System.Threading.Tasks;

namespace Comet.Core.World
{
    public static class MapDataManager
    {
        private struct MapData
        {
            public uint ID;
            public int Length;
            public string Name;
            public uint Puzzle;
        }

        private static readonly ConcurrentDictionary<uint, MapData> m_mapData = new ConcurrentDictionary<uint, MapData>();

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

                m_mapData.TryAdd(idMap, new MapData
                {
                    ID = idMap,
                    Length = length,
                    Name = name,
                    Puzzle = puzzle
                });
            }

            reader.Close();
            stream.Close();
            reader.Dispose();
            await stream.DisposeAsync();
        }

        public static GameMapData GetMapData(uint idDoc)
        {
            if (!m_mapData.TryGetValue(idDoc, out var value))
                return null;

            GameMapData mapData = new(idDoc);
            if (mapData.Load(value.Name.Replace("\\", Path.DirectorySeparatorChar.ToString())))
            {
                return mapData;
            }
            return null;
        }
    }
}
