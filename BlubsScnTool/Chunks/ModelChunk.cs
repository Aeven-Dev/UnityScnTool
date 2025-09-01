using BlubLib.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace NetsphereScnTool.Scene.Chunks
{
    public class ModelChunk : SceneChunk
    {
        public override ChunkType ChunkType => ChunkType.ModelData;

        public RenderFlag Shader { get; set; }
        public TextureData TextureData { get; set; }
        public MeshData Mesh { get; set; }
        public IList<WeightBone> WeightBone { get; set; }
        public List<ModelAnimation> Animation { get; set; }

        public ModelChunk(SceneContainer container)
            : base(container)
        {
            Shader = RenderFlag.None;
            TextureData = new TextureData(this);
            Mesh = new MeshData(this);
            WeightBone = new List<WeightBone>();
            Animation = new List<ModelAnimation>();
        }

        public override void Serialize(Stream stream)
        {
            base.Serialize(stream);

            using (var w = stream.ToBinaryWriter(true))
            {
                w.WriteEnum(Shader);
                SceneContainer.Log("Shader: " + Shader);
                SceneContainer.Log("WeightBone.Count: " + WeightBone.Count);
                SceneContainer.Log("Animation.Count: " + Animation.Count);

                w.Serialize(TextureData);
                w.Serialize(Mesh);

                w.Write(WeightBone.Count);
                w.Serialize(WeightBone);

                w.Write(Animation.Count);
                foreach (var pair in Animation)
                {
                    SceneContainer.Log("Animation.Name: " + pair.Name);
                    w.WriteCString(pair.Name);
                    w.Serialize(pair.transformKeyData2);
                }
            }
        }

        public override void Deserialize(Stream stream)
        {
            base.Deserialize(stream);

            using (var r = stream.ToBinaryReader(true))
            {
                Shader = r.ReadEnum<RenderFlag>();
                // ## CoreLib::Scene::CRenderable

                TextureData = new TextureData(this);
                TextureData.Deserialize(stream);

                Mesh = new MeshData(this);
                Mesh.Deserialize(stream);


                WeightBone = r.DeserializeArray<WeightBone>(r.ReadInt32()).ToList();

                SceneContainer.Log("Animation Count position " + r.BaseStream.Position);
                int count = r.ReadInt32();
                for (int i = 0; i < count; ++i)
                {
                    Animation.Add(new ModelAnimation { Name = r.ReadCString(), transformKeyData2 = r.Deserialize<TransformKeyData2>() });
                }
            }
        }
    }

    public class MeshData : IManualSerializer
    {
        public ModelChunk ModelChunk { get; }

        public List<Vector3> Vertices { get; set; }
        public List<Vector3Int> Faces { get; set; }
        public List<Vector3> Normals { get; set; }
        public List<Vector2> UV { get; set; }
        public List<Vector2> UV2 { get; set; }

        public List<Vector3> Tangents { get; set; }

        public MeshData(ModelChunk modelChunk)
        {
            Vertices = new List<Vector3>();
            Faces = new List<Vector3Int>();
            Normals = new List<Vector3>();
            UV = new List<Vector2>();
            UV2 = new List<Vector2>();
            Tangents = new List<Vector3>();

            ModelChunk = modelChunk;
        }

        public virtual void Serialize(Stream stream)
        {
            using (var w = stream.ToBinaryWriter(true))
            {
                w.Write(Vertices.Count);
                foreach (var vertex in Vertices)
                {
                    w.Write(vertex.x);
                    w.Write(vertex.y);
                    w.Write(vertex.z);
                }

                w.Write(Faces.Count);
                foreach (var face in Faces)
                {
                    w.Write((short)face.x);
                    w.Write((short)face.y);
                    w.Write((short)face.z);
                }

                w.Write(Normals.Count);
                foreach (var normal in Normals)
                {
                    w.Write(normal.x);
                    w.Write(normal.y);
                    w.Write(normal.z);
                }

                w.Write(UV.Count);
                foreach (var uv in UV)
                {
                    w.Write(uv.x);
                    w.Write(1-uv.y);
                }              

                if (ModelChunk.TextureData.ExtraUV == 1)
                {
                    foreach (var uv in UV2)
                    {
                        w.Write(uv.x);
                        w.Write(1-uv.y);
                    }              
                }

                w.Write(Tangents.Count);
                foreach (var unk in Tangents)
                {
                    w.Write(unk.x);
                    w.Write(unk.y);
                    w.Write(unk.z);
                }
                SceneContainer.Log("Vertices.Count: " + Vertices.Count);
                SceneContainer.Log("Faces.Count: " + Faces.Count);
                SceneContainer.Log("UV.Count: " + UV.Count);
                SceneContainer.Log("UV2.Count: " + UV2.Count);
                SceneContainer.Log("Tangents.Count: " + Tangents.Count);
            }
        }

        public virtual void Deserialize(Stream stream)
        {
            using (var r = stream.ToBinaryReader(true))
            {
                int count = r.ReadInt32();
                for (int i = 0; i < count; i++)
                    Vertices.Add(new Vector3(r.ReadSingle(), r.ReadSingle(), r.ReadSingle()));

                count = r.ReadInt32();
                for (int i = 0; i < count; i++)
                    Faces.Add(new Vector3Int(r.ReadUInt16(), r.ReadUInt16(), r.ReadUInt16()));

                count = r.ReadInt32();
                for (int i = 0; i < count; i++)
                    Normals.Add(new Vector3(r.ReadSingle(), r.ReadSingle(), r.ReadSingle()));

                count = r.ReadInt32();
                for (int i = 0; i < count; i++)
                    UV.Add(new Vector2(r.ReadSingle(), 1-r.ReadSingle()));

                //Debug.Log("UV2 position " + r.BaseStream.Position);
                if (ModelChunk.TextureData.ExtraUV == 1)
                {
                    for (int i = 0; i < count; i++)
                        UV2.Add(new Vector2(r.ReadSingle(), 1-r.ReadSingle()));
                }

                //Debug.Log("Tangent count position " + r.BaseStream.Position);
                count = r.ReadInt32();
                for (int i = 0; i < count; i++)
                    Tangents.Add(new Vector3(r.ReadSingle(), r.ReadSingle(), r.ReadSingle()));
            }
        }

        public int[] Triangles()
        {
            int[] array = new int[Faces.Count * 3];
            //int[] array = new int[3];
            for (int i = 0; i < Faces.Count; i++)
            {
                array[i * 3] = Faces[i].x;
                array[i * 3 + 1] = Faces[i].y;
                array[i * 3 + 2] = Faces[i].z;
            }
            return array;
        }

        public void SetTriangles( int[] indices )
        {
			if (indices.Length % 3 != 0)
			{
                Debug.Log("Face inidices were not a multiple of 3");
			}
            for (int i = 0; i < indices.Length; i+=3)
            {
                Faces.Add(new Vector3Int(indices[i], indices [i+1], indices[i+2]));
            }
        }

        public Vector4[] TangentsArray()
        {
            Vector4[] tangents = new Vector4[Tangents.Count];
            for (int i = 0; i < Tangents.Count; i++)
            {
                tangents[i] = Tangents[i];
            }
            return tangents;
        }

        public void SetTangents(Vector4[] tangents)
        {
            Tangents = new List<Vector3>();
            for (int i = 0; i < tangents.Length; i++)
            {
                Tangents.Add( tangents[i]);
            }
        }
    }

    public class WeightBone : IManualSerializer
    {
        public string Name { get; set; }
        public Matrix4x4 Matrix { get; set; }
        public IList<WeightData> Weight { get; set; }

        public WeightBone()
        {
            Name = "";
            Matrix = Matrix4x4.identity;
            Weight = new List<WeightData>();
        }

        public virtual void Serialize(Stream stream)
        {
            using (var w = stream.ToBinaryWriter(true))
            {
                w.WriteCString(Name);
                w.Write(Matrix);

                w.Write(Weight.Count);
                foreach (var weight in Weight)
                {
                    w.Write(weight.Vertex);
                    w.Write(weight.Weight);
                }
            }
        }

        public virtual void Deserialize(Stream stream)
        {
            using (var r = stream.ToBinaryReader(true))
            {
                Name = r.ReadCString();
                Matrix = r.ReadMatrix();

                uint count = r.ReadUInt32();
                for (int i = 0; i < count; i++)
                    Weight.Add(new WeightData { Vertex = r.ReadUInt32(), Weight = r.ReadSingle() });
            }
        }
    }

    public struct WeightData
    {
        public uint Vertex;
        public float Weight;
    }

    // Game::CActorGeomData
    public class TextureData : IManualSerializer
    {
        public float Version { get; set; }
        public ModelChunk ModelChunk { get; }
        public uint ExtraUV { get; set; }
        public List<TextureEntry> Textures { get; set; }

        public TextureData(ModelChunk modelChunk)
        {
            ModelChunk = modelChunk;
            Textures = new List<TextureEntry>();
            ExtraUV = 0;
            Version = 0.2000000029802322f;
        }

        public virtual void Serialize(Stream stream)
        {
            using (var w = stream.ToBinaryWriter(true))
            {
                w.Write(Version);
                if (Version >= 0.2000000029802322f)
                    w.Write(ExtraUV);

                SceneContainer.Log("Textures.Count: " + Textures.Count);
                w.Write(Textures.Count);
                foreach (var texture in Textures)
                {
                    SceneContainer.Log("texture.FileName: " + texture.main_texture);
                    w.WriteCString(texture.main_texture, 1024);
                    if (Version >= 0.2000000029802322f)
                        w.WriteCString(texture.side_texture, 1024);

                    w.Write(texture.face_offset);
                    w.Write(texture.face_count);
                }
            }
        }

        public virtual void Deserialize(Stream stream)
        {
            using (var r = stream.ToBinaryReader(true))
            {
                Version = r.ReadSingle();

                if (Version >= 0.2000000029802322f)
                    ExtraUV = r.ReadUInt32();

                uint count = r.ReadUInt32();
                for (int i = 0; i < count; i++)
                {
                    var textureData = new TextureEntry
                    {
                        main_texture = ReadString(r,1024),
                        side_texture = ""
                    };

                    if (Version >= 0.2000000029802322f)
                        textureData.side_texture = ReadString(r,1024);

                    textureData.face_offset = r.ReadInt32();
                    textureData.face_count = r.ReadInt32();

                    Textures.Add(textureData);
                }
            }
        }
        string ReadString(BinaryReader r, int length)
		{
            List<char> chars = new();
            long position = r.BaseStream.Position;
			for (int i = 0; i < length; i++)
			{
                var c = r.ReadChar();
				if (Convert.ToByte(c) == byte.Parse("0"))
				{
                    break;
				}
                chars.Add(c);
            }
            r.BaseStream.Seek(position + 1024, SeekOrigin.Begin);
            return new string(chars.ToArray());
		}

    }

    public struct TextureEntry
    {
        public string main_texture;
        public string side_texture;
        public int face_offset;
        public int face_count;
    }
}
