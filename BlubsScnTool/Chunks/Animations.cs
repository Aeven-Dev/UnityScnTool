using BlubLib.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;

namespace NetsphereScnTool.Scene.Chunks.DoNotUse
{
    public class ModelAnimation
    {
        public string Name;
        public TransformKeyData2 TransformKeyData2;
    }

    public class TransformKeyData2 : TransformKeyData
    {
        public IList<MorphKey> MorphKeys { get; set; }

        public TransformKeyData2()
        {
            MorphKeys = new List<MorphKey>();
        }

        public override void Serialize(Stream stream)
        {
            base.Serialize(stream);

            using (var w = stream.ToBinaryWriter(true))
            {
                w.Write(MorphKeys.Count);
                w.Serialize(MorphKeys);
            }
        }

        public override void Deserialize(Stream stream)
        {
            base.Deserialize(stream);

            using (var r = stream.ToBinaryReader(true))
                MorphKeys = r.DeserializeArray<MorphKey>(r.ReadInt32()).ToList();

        }
    }

    public class BoneAnimation
    {
        public string Name;
        public string Copy;
        public TransformKeyData TransformKeyData;
    }

    public class TransformKeyData : IManualSerializer
    {
        public TimeSpan Duration { get; set; }
        public TransformKey TransformKey { get; set; }
        public IList<FloatKey> FloatKeys { get; set; }

        public TransformKeyData()
        {
            Duration = TimeSpan.Zero;
            FloatKeys = new List<FloatKey>();
        }

        public virtual void Serialize(Stream stream)
        {
            using (var w = stream.ToBinaryWriter(true))
            {
                w.Write((uint)Duration.TotalMilliseconds);

                w.Write(TransformKey != null);
                if (TransformKey != null)
                    w.Serialize(TransformKey);

                w.Write(FloatKeys.Count);
                w.Serialize(FloatKeys);
            }
        }

        public virtual void Deserialize(Stream stream)
        {
            using (var r = stream.ToBinaryReader(true))
            {
                Duration = TimeSpan.FromMilliseconds(r.ReadUInt32());

                SceneContainer.Log("Duration: " + Duration);
                bool flag = r.ReadBoolean();
                if (flag)
                    TransformKey = r.Deserialize<TransformKey>();

                SceneContainer.Log("FloatKeys.Count: " + FloatKeys.Count);
                FloatKeys = r.DeserializeArray<FloatKey>(r.ReadInt32()).ToList();
            }
        }
    }

    public class TransformKey : IManualSerializer
    {
        public Vector3 Translation { get; set; }
        public Quaternion Rotation { get; set; }
        public Vector3 Scale { get; set; }

        public IList<TKey> TKey { get; set; }
        public IList<RKey> RKey { get; set; }
        public IList<SKey> SKey { get; set; }

        public TransformKey()
        {
            Translation = Vector3.Zero;
            Rotation = Quaternion.Identity;
            Scale = Vector3.Zero;

            TKey = new List<TKey>();
            RKey = new List<RKey>();
            SKey = new List<SKey>();
        }

        public void Serialize(Stream stream)
        {
            using (var w = stream.ToBinaryWriter(true))
            {
                w.Write(Translation.X);
                w.Write(Translation.Y);
                w.Write(Translation.Z);

                w.Write(Rotation.X);
                w.Write(Rotation.Y);
                w.Write(Rotation.Z);
                w.Write(Rotation.W);

                w.Write(Scale.X);
                w.Write(Scale.Y);
                w.Write(Scale.Z);



                w.Write(TKey.Count);
                foreach (var tkey in TKey)
                {
                    w.Write((uint)tkey.Duration.TotalMilliseconds);

                    w.Write(tkey.Translation.X);
                    w.Write(tkey.Translation.Y);
                    w.Write(tkey.Translation.Z);
                }

                w.Write(RKey.Count);
                foreach (var rkey in RKey)
                {
                    w.Write((uint)rkey.Duration.TotalMilliseconds);

                    w.Write(rkey.Rotation.X);
                    w.Write(rkey.Rotation.Y);
                    w.Write(rkey.Rotation.Z);
                    w.Write(rkey.Rotation.W);
                }

                w.Write(SKey.Count);
                foreach (var skey in SKey)
                {
                    w.Write((uint)skey.Duration.TotalMilliseconds);

                    w.Write(skey.Scale.X);
                    w.Write(skey.Scale.Y);
                    w.Write(skey.Scale.Z);
                }
                
            }
        }

        public void Deserialize(Stream stream)
        {
            using (var r = stream.ToBinaryReader(true))
            {
                Translation = new Vector3(r.ReadSingle(), r.ReadSingle(), r.ReadSingle());
                Rotation = new Quaternion(r.ReadSingle(), r.ReadSingle(), r.ReadSingle(), r.ReadSingle());
                Scale = new Vector3(r.ReadSingle(), r.ReadSingle(), r.ReadSingle());

                uint count = r.ReadUInt32();
                for (int n = 0; n < count; n++)
                    TKey.Add(new TKey { Duration = TimeSpan.FromMilliseconds(r.ReadUInt32()), Translation = new Vector3(r.ReadSingle(), r.ReadSingle(), r.ReadSingle()) });

                count = r.ReadUInt32();
                for (int n = 0; n < count; n++)
                    RKey.Add(new RKey { Duration = TimeSpan.FromMilliseconds(r.ReadUInt32()), Rotation = new Quaternion(r.ReadSingle(), r.ReadSingle(), r.ReadSingle(), r.ReadSingle()) });

                count = r.ReadUInt32();
                for (int n = 0; n < count; n++)
                    SKey.Add(new SKey { Duration = TimeSpan.FromMilliseconds(r.ReadUInt32()), Scale = new Vector3(r.ReadSingle(), r.ReadSingle(), r.ReadSingle()) });
            }
        }
    }

    public struct TKey
    {
        public TimeSpan Duration;
        public Vector3 Translation;
    }

    public struct RKey
    {
        public TimeSpan Duration;
        public Quaternion Rotation;
    }

    public struct SKey
    {
        public TimeSpan Duration;
        public Vector3 Scale;
    }

    public struct VKey
    {
        public TimeSpan Duration;
        public float Alpha;
    }

    public class FloatKey : IManualSerializer
    {
        public TimeSpan Duration { get; set; }
        public float Alpha { get; set; }

        public void Serialize(Stream stream)
        {
            using (var w = stream.ToBinaryWriter(true))
            {
                w.Write((uint)Duration.TotalMilliseconds);
                w.Write(Alpha);
            }
        }

        public void Deserialize(Stream stream)
        {
            using (var r = stream.ToBinaryReader(true))
            {
                Duration = TimeSpan.FromMilliseconds(r.ReadUInt32());
                Alpha = r.ReadSingle();
            }
        }
    }

    public class MorphKey : IManualSerializer
    {
        public TimeSpan Duration { get; set; }
        public IList<Quaternion> Rotations { get; set; }
        public IList<Vector3> Positions { get; set; }

        public MorphKey()
        {
            Rotations = new List<Quaternion>();
            Positions = new List<Vector3>();
        }

        public void Serialize(Stream stream)
        {
            using (var w = stream.ToBinaryWriter(true))
            {
                w.Write((uint)Duration.TotalMilliseconds);

                w.Write(Rotations.Count);
                foreach (var rotation in Rotations)
                {
                    w.Write(rotation.X);
                    w.Write(rotation.Y);
                    w.Write(rotation.Z);
                    w.Write(rotation.W);
                }

                w.Write(Positions.Count);
                foreach (var position in Positions)
                {
                    w.Write(position.X);
                    w.Write(position.Y);
                    w.Write(position.Z);
                }
            }
        }

        public void Deserialize(Stream stream)
        {
            using (var r = stream.ToBinaryReader(true))
            {
                Duration = TimeSpan.FromMilliseconds(r.ReadUInt32());

                int count = r.ReadInt32();
                for (int j = 0; j < count; j++)
                    Rotations.Add(new Quaternion(r.ReadSingle(), r.ReadSingle(), r.ReadSingle(), r.ReadSingle()));

                count = r.ReadInt32();
                for (int j = 0; j < count; j++)
                    Positions.Add(new Vector3(r.ReadSingle(), r.ReadSingle(), r.ReadSingle()));
            }
        }
    }
}
