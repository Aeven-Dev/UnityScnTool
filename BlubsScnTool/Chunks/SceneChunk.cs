using BlubLib.IO;
using System.Drawing;
using System.IO;
using UnityEngine;
using AevenScnTool;

namespace NetsphereScnTool.Scene.Chunks
{
    public abstract class SceneChunk : IManualSerializer
    {
        public SceneContainer Container { get; private set; }

        public abstract ChunkType ChunkType { get; }
        public string Name { get; set; }
        public string SubName { get; set; }

        public VERSION Version { get; set; }
        public Matrix4x4 Matrix { get; set; }
        public VERSION Version2 { get; set; }


        protected SceneChunk(SceneContainer container)
        {
            Name = "";
            SubName = "";
            Version = VERSION.TWO;
            Matrix = Matrix4x4.identity;
            Version2 = VERSION.TWO;
            Container = container;
        }

        public virtual void Serialize(Stream stream)
        {
            using (var w = stream.ToBinaryWriter(true))
            {
                w.Write((int)Version);
                w.Write(Matrix);
                w.Write((int)Version2);
            }
        }

        public virtual void Deserialize(Stream stream)
        {
            using (var r = stream.ToBinaryReader(true))
            {
                Version = (VERSION)r.ReadInt32();
                Matrix = r.ReadMatrix();
                Version2 = (VERSION)r.ReadInt32();
            }
        }

        public override string ToString() => Name + " - " + SubName;
    }
}
