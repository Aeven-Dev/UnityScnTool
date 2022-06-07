using BlubLib.IO;
using System;
using System.IO;
using UnityEngine;

namespace NetsphereScnTool.Scene.Chunks
{
    public class SkyDirect1Chunk : SceneChunk
    {
        public override ChunkType ChunkType => ChunkType.SkyDirect1;
        public Color color1;
        public Color color2;
        public Color color3;
        public Color color4;
        public Color color5;
        public Color color6;

        public SkyDirect1Chunk(SceneContainer container)
            : base(container)
        {
            color1 = Color.white;
            color2 = Color.white;
            color3 = Color.white;
            color4 = Color.white;
            color5 = Color.white;
            color6 = Color.white;
        }

        public override void Serialize(Stream stream)
        {
            //if (Data.Length != 96)
            //    throw new Exception("SkyDirect1 data must have 164 bytes");

            base.Serialize(stream);

			using var w = stream.ToBinaryWriter(true);
            w.Write(color1);
            w.Write(color2);
            w.Write(color3);
            w.Write(color4);
            w.Write(color5);
            w.Write(color6);
        }

        public override void Deserialize(Stream stream)
        {
            base.Deserialize(stream);

            using var r = stream.ToBinaryReader(true);
            color1 = new Color(r.ReadSingle(), r.ReadSingle(), r.ReadSingle());
            color2 = new Color(r.ReadSingle(), r.ReadSingle(), r.ReadSingle());
            color3 = new Color(r.ReadSingle(), r.ReadSingle(), r.ReadSingle());
            color4 = new Color(r.ReadSingle(), r.ReadSingle(), r.ReadSingle());
            color5 = new Color(r.ReadSingle(), r.ReadSingle(), r.ReadSingle());
            color6 = new Color(r.ReadSingle(), r.ReadSingle(), r.ReadSingle());

        }
    }
}
