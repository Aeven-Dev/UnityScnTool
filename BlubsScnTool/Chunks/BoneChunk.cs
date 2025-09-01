using BlubLib.IO;
using System.Collections.Generic;
using System.IO;
using AevenScnTool;

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

            SceneContainer.Log("Animation.Count: " + Animation.Count);
            using (var w = stream.ToBinaryWriter(true))
            {
                w.Write(Animation.Count);
                foreach (var anim in Animation)
                {
                    if (Version2 == VERSION.TWO)
                    {
                        SceneContainer.Log("anim.Name: " + anim.Name);
                        SceneContainer.Log("anim.Copy: " + anim.Copy);
                        w.WriteCString(anim.Name);
                        w.WriteCString(anim.Copy);
                        if (string.IsNullOrWhiteSpace(anim.Copy))
                            w.Serialize(anim.TransformKeyData);
                    }
                    else
                    {
                        w.WriteCString(anim.Name);
                        w.Serialize(anim.TransformKeyData);
                        SceneContainer.Log("anim.Name: " + anim.Name);
                        SceneContainer.Log("anim.TransformKeyData: " + anim.TransformKeyData);
                    }
                }
            }
        }

        public override void Deserialize(Stream stream)
        {
            base.Deserialize(stream);

            using (var r = stream.ToBinaryReader(true))
            {
                uint count = r.ReadUInt32();
                for (int i = 0; i < count; i++)
                {
                    SceneContainer.Log("Version: " + Version);
                    if (Version2 == VERSION.TWO)
                    {
                        string name1 = r.ReadCString();
                        string subName = r.ReadCString();

                        SceneContainer.Log("anim.Name: " + name1);
                        SceneContainer.Log("anim.Copy: " + subName);

                        TransformKeyData transformKeyData = null;

                        if (string.IsNullOrWhiteSpace(subName))
						{
                            transformKeyData = r.Deserialize<TransformKeyData>();
                        }

                        Animation.Add(new BoneAnimation { Name = name1, Copy = subName, TransformKeyData = transformKeyData });



                    }
                    else
                    {

                        string name2 = r.ReadCString();
                        SceneContainer.Log("anim.Name: " + name2 + " - position: " + r.BaseStream.Position);

                        Animation.Add(new BoneAnimation { Name = name2, Copy = default(string), TransformKeyData = r.Deserialize<TransformKeyData>() });
                    }
                }
            }

        }
    }
}
