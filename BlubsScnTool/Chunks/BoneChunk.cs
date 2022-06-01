using BlubLib.IO;
using System.Collections.Generic;
using System.IO;

namespace NetsphereScnTool.Scene.Chunks
{
    public class BoneChunk : SceneChunk
    {
        public override ChunkType ChunkType => ChunkType.Bone;

        public List<BoneAnimation> Animation { get; set; }

        public BoneChunk(SceneContainer container)
            : base(container)
        {
            Animation = new List<BoneAnimation>();
        }

        public override void Serialize(Stream stream)
        {
            base.Serialize(stream);

            using (var w = stream.ToBinaryWriter(true))
            {
                w.Write(Version);

                w.Write(Animation.Count);
                foreach (var anim in Animation)
                {
                    if (Version >= 0.2000000029802322f)
                    {
                        w.WriteCString(anim.Name);
                        w.WriteCString(anim.Copy);
                        if (string.IsNullOrWhiteSpace(anim.Copy))
                            w.Serialize(anim.TransformKeyData);
                    }
                    else
                    {
                        w.WriteCString(anim.Name);
                        w.Serialize(anim.TransformKeyData);
                    }
                }
            }
        }

        public override void Deserialize(Stream stream)
        {
            base.Deserialize(stream);

            using (var r = stream.ToBinaryReader(true))
            {
                Version = r.ReadSingle();

                uint count = r.ReadUInt32();
                for (int i = 0; i < count; i++)
                {
                    if (Version >= 0.2000000029802322f)
                    {
                        string name = r.ReadCString();
                        string subName = r.ReadCString();
                        TransformKeyData transformKeyData = null;

                        if (string.IsNullOrWhiteSpace(subName))
						{
                            transformKeyData = r.Deserialize<TransformKeyData>();
                        }

                        Animation.Add(new BoneAnimation { Name = name, Copy = subName, TransformKeyData = transformKeyData });
                    }
                    else
                    {

                        Animation.Add(new BoneAnimation { Name = r.ReadCString(), Copy = default(string), TransformKeyData = r.Deserialize<TransformKeyData>() });
                    }
                }
            }
        }
    }
}
