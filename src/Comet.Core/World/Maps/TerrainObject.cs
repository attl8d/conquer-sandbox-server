using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;

namespace Comet.Core.World.Maps
{
    public sealed class TerrainObject : MapObject
    {
        public List<TerrainObjectPart> Parts = new List<TerrainObjectPart>();

        private TerrainObject()
        {
        }

        public uint OwnerIdentity { get; private set; }

        public static TerrainObject CreateNew(BinaryReader scenery, uint idOwner = 0)
        {
            try
            {
                var objTerrain = new TerrainObject()
                {
                    OwnerIdentity = idOwner
                };

                int amount = scenery.ReadInt32();
                for (int parts = 0; parts < amount; parts++)
                {
                    TerrainObjectPart terrainPart = new();
                    terrainPart.AniFile = Encoding.ASCII.GetString(scenery.ReadBytes(256)).Replace('\0', ' ').TrimEnd();
                    terrainPart.AniTitle = Encoding.ASCII.GetString(scenery.ReadBytes(64)).Replace('\0', ' ').TrimEnd();
                    //scenery.BaseStream.Seek(256 + 64, SeekOrigin.Current);

                    terrainPart.PosOffsetX = scenery.ReadInt32();
                    terrainPart.PosOffsetY = scenery.ReadInt32();
                    terrainPart.FrameInterval = scenery.ReadInt32();
                    //scenery.BaseStream.Seek(12, SeekOrigin.Current);

                    terrainPart.SizeBaseCX = scenery.ReadInt32();
                    terrainPart.SizeBaseCY = scenery.ReadInt32();
                    terrainPart.Thickness = scenery.ReadInt32();
                    //scenery.BaseStream.Seek(4, SeekOrigin.Current);

                    terrainPart.PosSceneOffsetX = scenery.ReadInt32();
                    terrainPart.PosSceneOffsetY = scenery.ReadInt32();
                    terrainPart.Height = scenery.ReadInt32();
                    //scenery.BaseStream.Seek(4, SeekOrigin.Current);

                    for (int y = 0; y < terrainPart.SizeBaseCY; y++)
                    {
                        for (int x = 0; x < terrainPart.SizeBaseCX; x++)
                        {
                            int mask = (ushort)scenery.ReadInt32();
                            int terrain = (ushort)scenery.ReadInt32();
                            int altitude = (ushort)scenery.ReadInt32();

                            var layer = new Layer
                            {
                                Altitude = (ushort)altitude,
                                Mask = (ushort)mask,
                                Terrain = (ushort)terrain
                            };
                            terrainPart.Layers.Add(layer);
                        }
                    }

                    objTerrain.AddPart(terrainPart);
                }

                return objTerrain;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return null;
            }
        }

        public void AddPart(TerrainObjectPart part)
        {
            Parts.Add(part);
        }

        public void RemovePart(int identity)
        {
            Parts.RemoveAll(x => x.Identity == identity);
        }

        public void SetPos(Point pos)
        {
            X = pos.X;
            Y = pos.Y;

            foreach (var part in Parts)
            {
                int x = pos.X;
                int y = pos.Y;

                x += part.PosSceneOffsetX;
                y += part.PosSceneOffsetY;

                part.X = x;
                part.Y = y;
            }
        }
    }
}
