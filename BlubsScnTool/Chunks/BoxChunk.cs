using BlubLib.IO;
using System;
using System.IO;
using UnityEngine;

namespace NetsphereScnTool.Scene.Chunks
{
    public class BoxChunk : SceneChunk
    {
        public override ChunkType ChunkType => ChunkType.Box;

        public float Unk { get; set; }
        public float Unk2 { get; set; }
        public int Unk3 { get; set; }
        public Vector3[] Unk4 { get; set; }
        public Vector3 Size { get; set; }

        public BoxChunk(SceneContainer container)
            : base(container)
        {
            Unk = 0.10000000149011612f;
            Unk2 = 0;
            Unk3 = 0;
            Unk4 = new[]
{
                new Vector3(1, 0, 0),
                new Vector3(0, 1, 0),
                new Vector3(0, 0, 1)
            };
            Size = new Vector3(1,1,1);
        }

        public override void Serialize(Stream stream)
        {
            if (Unk4.Length != 3)
                throw new Exception("Unk7 must have a length of 3");

            base.Serialize(stream);

            using (var w = stream.ToBinaryWriter(true))
            {
                w.Write(Version);
                w.Write(Version);
                w.Write(Unk);
                w.Write(Unk2);
                w.Write(Unk3);

                foreach (var vec in Unk4)
                {
                    w.Write(vec.x);
                    w.Write(vec.y);
                    w.Write(vec.z);
                }

                w.Write(Size.x);
                w.Write(Size.y);
                w.Write(Size.z);
            }
        }

        public override void Deserialize(Stream stream)
        {
            base.Deserialize(stream);

            using (var r = stream.ToBinaryReader(true))
            {
                Version = r.ReadSingle();
                Version = r.ReadSingle();
                Unk = r.ReadInt32();
                Unk2 = r.ReadSingle();
                Unk3 = r.ReadInt32();

                for (int i = 0; i < 3; i++)
                    Unk4[i] = new Vector3(r.ReadSingle(), r.ReadSingle(), r.ReadSingle());

                Size = new Vector3(r.ReadSingle(), r.ReadSingle(), r.ReadSingle());
            }
        }
    }
}
