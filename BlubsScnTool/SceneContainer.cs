using BlubLib.IO;
using NetsphereScnTool.Scene.Chunks;
using NetsphereScnTool.Utility;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace NetsphereScnTool.Scene
{
    public class SceneContainer : SortableBindingList<SceneChunk>
    {
        public static bool verbose = false;

        public SceneHeader Header { get; set; }

        public List<BoxChunk> boxes = new List<BoxChunk>();
        public List<ModelChunk> models = new List<ModelChunk>();
        public List<BoneChunk> bones = new List<BoneChunk>();
        public List<BoneSystemChunk> boneSystems = new List<BoneSystemChunk>();
        public List<SkyDirect1Chunk> skyDirect1List = new List<SkyDirect1Chunk>();
        public List<ShapeChunk> shapes = new List<ShapeChunk>();

        public FileInfo fileInfo;

        public SceneContainer()
        {
            Header = new SceneHeader();
        }

        public SceneContainer(IEnumerable<SceneChunk> collection)
            : base(collection)
        {
            Header = new SceneHeader();

			foreach (var chunk in collection)
			{
				switch (chunk.ChunkType)
				{
					case ChunkType.Box:
                        boxes.Add(chunk as BoxChunk);
						break;
					case ChunkType.ModelData:
                        models.Add(chunk as ModelChunk);
                        break;
					case ChunkType.Bone:
                        bones.Add(chunk as BoneChunk);
                        break;
					case ChunkType.SkyDirect1:
                        skyDirect1List.Add(chunk as SkyDirect1Chunk);
                        break;
					case ChunkType.BoneSystem:
                        boneSystems.Add(chunk as BoneSystemChunk);
                        break;
					case ChunkType.Shape:
                        shapes.Add(chunk as ShapeChunk);
                        break;
					default:
						break;
				}
			}
        }

        public object Clone()
        {
            var container = new SceneContainer();
            container.AddRange(this);
            container.Header = Header;

            return container;
        }

        #region ReadFrom

        public static SceneContainer ReadFrom(string fileName)
        {
            using (var fs = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read))
                return ReadFrom(fs);
        }

        public static SceneContainer ReadFrom(byte[] data)
        {
            using (var s = new MemoryStream(data))
                return ReadFrom(s);
        }

        public static SceneContainer ReadFrom(Stream stream)
        {
            var container = new SceneContainer();

            using (var r = new BinaryReader(stream))
            {
                container.Header.Deserialize(stream);

                // CoreLib::Scene::CSceneGroup
                uint chunkCount = r.ReadUInt32();

                if (container.Header.Version2 >= 1045220557)
                    r.ReadByte(); // ToDo ReadString

                for (int i = 0; i < chunkCount; i++)
                {
                    var type = r.ReadEnum<ChunkType>();
                    string name = r.ReadCString();

                    //Debug.Log("Chunk name: " + name);
                    string subName = r.ReadCString();

                    //Debug.Log("Chunk subName: " + subName);
                    switch (type)
                    {
                        case ChunkType.ModelData:
                            ModelChunk model = new ModelChunk(container)
                            {
                                Name = name,
                                SubName = subName
                            };
                            model.Deserialize(stream);
                            container.Add(model);
                            container.models.Add(model);
                            break;

                        case ChunkType.Box:
                            BoxChunk box = new BoxChunk(container)
                            {
                                Name = name,
                                SubName = subName
                            };
                            box.Deserialize(stream);
                            container.Add(box);
                            container.boxes.Add(box);
                            break;

                        case ChunkType.Bone:
                            BoneChunk bone = new BoneChunk(container)
                            {
                                Name = name,
                                SubName = subName
                            };
                            bone.Deserialize(stream);
                            container.Add(bone);
                            container.bones.Add(bone);
                            break;

                        case ChunkType.BoneSystem:
                            BoneSystemChunk boneSys = new BoneSystemChunk(container)
                            {
                                Name = name,
                                SubName = subName
                            };
                            boneSys.Deserialize(stream);
                            container.Add(boneSys);
                            container.boneSystems.Add(boneSys);
                            break;

                        case ChunkType.Shape:
                            ShapeChunk shape = new ShapeChunk(container)
                            {
                                Name = name,
                                SubName = subName
                            };
                            shape.Deserialize(stream);
                            container.Add(shape);
                            container.shapes.Add(shape);
                            break;

                        case ChunkType.SkyDirect1:
                            SkyDirect1Chunk skyDirect1 = new SkyDirect1Chunk(container)
                            {
                                Name = name,
                                SubName = subName
                            };
                            skyDirect1.Deserialize(stream);
                            container.Add(skyDirect1);
                            container.skyDirect1List.Add(skyDirect1);
                            break;

                        default:
                            throw new Exception($"Unknown chunk type: 0x{(int)type:X4} StreamPosition: {r.BaseStream.Position}");
                    }
                }
            }

            return container;
        }

        #endregion

        public void Write(string fileName)
        {
            using (var fs = new FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.None))
                Write(fs);
        }

        public void Write(Stream stream)
        {
            using (var w = new BinaryWriter(stream))
            {
                w.Serialize(Header);

                w.Write(Count);
                if (Header.Version2 >= 1045220557)
                    w.Write((byte)0);

                foreach (var chunk in this)
                {
                    w.WriteEnum(chunk.ChunkType);
                    w.WriteCString(chunk.Name);
                    w.WriteCString(chunk.SubName);

                    w.Serialize(chunk);
                }
            }
        }

        public static void Log(object message)
		{
			if (verbose)
			{
                Debug.Log(message);
			}
		}
    }

    public class SceneHeader : IManualSerializer
    {
        public const uint c_Version = 1;
        public const uint Magic = 0x6278d57a;

        public string Name { get; set; }
        public string SubName { get; set; }
        public int Version { get; set; }
        public int Version2 { get; set; }
        public Matrix4x4 Matrix { get; set; }

        internal SceneHeader()
        {
            Name = "";
            SubName = "";
            Version = 1036831949;
            Version2 = 1045220557;
            Matrix = Matrix4x4.identity;
        }

        public void Serialize(Stream stream)
        {
            using (var w = stream.ToBinaryWriter(true))
            {
                w.Write(c_Version);
                w.Write(Magic);

                w.WriteCString(Name);
                w.WriteCString(SubName);

                w.Write(Version);
                w.Write(Matrix);
                w.Write(Version2);
            }
        }

        public void Deserialize(Stream stream)
        {
            using (var r = stream.ToBinaryReader(true))
            {
                uint value;
                do
                {
                    value = r.ReadUInt32();
                    if (value != Magic)
                        r.BaseStream.Seek(-3, SeekOrigin.Current);
                } while (value != Magic);

                Name = r.ReadCString();
                SubName = r.ReadCString();

                // CoreLib::Scene::CSceneNode
                Version = r.ReadInt32();
                Matrix = r.ReadMatrix();

                // CoreLib::Scene::CSceneGroup
                Version2 = r.ReadInt32();
            }
        }
    }
}
