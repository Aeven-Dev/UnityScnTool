using BlubLib.IO;
using System.IO;

namespace NetsphereScnTool.Scene.Chunks
{
    public class BoneSystemChunk : SceneChunk
    {
        public override ChunkType ChunkType => ChunkType.BoneSystem;

        public BoneSystemChunk(SceneContainer container)
            : base(container)
        { }

        public override void Serialize(Stream stream)
        {
            base.Serialize(stream);

           
        }

        public override void Deserialize(Stream stream)
        {
            base.Deserialize(stream);

        }
    }
}
