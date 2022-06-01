using BlubLib.IO;
using System.Drawing;
using System.IO;
using UnityEngine;

namespace NetsphereScnTool.Scene.Chunks
{
    public abstract class SceneChunk : IManualSerializer
    {
        public SceneContainer Container { get; private set; }

        public abstract ChunkType ChunkType { get; }
        public string Name { get; set; }
        public string SubName { get; set; }

        public float Version { get; set; }
        public Matrix4x4 Matrix { get; set; }


        protected SceneChunk(SceneContainer container)
        {
            Name = "";
            SubName = "";
            Version = 0.1f;
            Matrix = Matrix4x4.identity;
            Container = container;
        }

        public virtual void Serialize(Stream stream)
        {
            using (var w = stream.ToBinaryWriter(true))
            {
                w.Write(Version);
                w.Write(Matrix);
            }
        }

        public virtual void Deserialize(Stream stream)
        {
            using (var r = stream.ToBinaryReader(true))
            {
                Version = r.ReadSingle();
                Matrix = r.ReadMatrix();
            }
        }

        public override string ToString() => Name + " - " + SubName;
    }
}
