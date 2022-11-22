using BlubLib.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

[Serializable]
public class ModelAnimation : IManualSerializer
{
    public string Name;
    public TransformKeyData2 transformKeyData2;

    public void Serialize(Stream stream)
    {
        using (var w = stream.ToBinaryWriter(true))
        {
            w.Serialize(transformKeyData2);

            NetsphereScnTool.Scene.SceneContainer.Log("MorphKey Count: " + MorphKeys.Count);
            w.Write(MorphKeys.Count);
            w.Serialize(MorphKeys);
        }
    }

	public void Deserialize(Stream stream)
	{
		throw new NotImplementedException();
	}

	public List<MorphKey> MorphKeys { get; set; }
}
[Serializable]
public class TransformKeyData2 : TransformKeyData
{
    public List<MorphKey> MorphKeys { get; set; }

    public TransformKeyData2() : base()
    {
        MorphKeys = new List<MorphKey>();
    }

    public override void Serialize(Stream stream)
    {
        base.Serialize(stream);

        using (var w = stream.ToBinaryWriter(true))
        {
			NetsphereScnTool.Scene.SceneContainer.Log("MorphKey Count: " + MorphKeys.Count);
            w.Write(MorphKeys.Count);
            w.Serialize(MorphKeys);
        }
    }

    public override void Deserialize(Stream stream)
    {
        base.Deserialize(stream);

        using (var r = stream.ToBinaryReader(true))
            MorphKeys = r.DeserializeArray<MorphKey>(r.ReadInt32()).ToList();
        NetsphereScnTool.Scene.SceneContainer.Log("MorphKeys.Count: " + MorphKeys.Count);
    }

    
}

[Serializable]
public class BoneAnimation
{
    public string Name;
    public string Copy;
    public TransformKeyData TransformKeyData;
}

[Serializable]
public class TransformKeyData : IManualSerializer
{
    public int duration;
    public TransformKey TransformKey;
    public List<FloatKey> AlphaKeys;

    public TransformKeyData()
    {
        TransformKey = new TransformKey();
        duration = 0;
        AlphaKeys = new List<FloatKey>();
    }

    public virtual void Serialize(Stream stream)
    {
        using (var w = stream.ToBinaryWriter(true))
        {
            w.Write((uint)duration);
            NetsphereScnTool.Scene.SceneContainer.Log("duration: " + (uint)duration);

            w.Write(TransformKey != null);
            NetsphereScnTool.Scene.SceneContainer.Log("TransformKey: " + TransformKey);
            if (TransformKey != null)
                w.Serialize(TransformKey);

            w.Write(AlphaKeys.Count);
            NetsphereScnTool.Scene.SceneContainer.Log("FloatKeys.Count: " + AlphaKeys.Count);
            w.Serialize(AlphaKeys);
        }
    }

    public virtual void Deserialize(Stream stream)
    {
        using (var r = stream.ToBinaryReader(true))
        {
            duration = (int)r.ReadUInt32();

            bool flag = r.ReadBoolean();
            if (flag)
                TransformKey = r.Deserialize<TransformKey>();

            AlphaKeys = r.DeserializeArray<FloatKey>(r.ReadInt32()).ToList();
        }
        NetsphereScnTool.Scene.SceneContainer.Log("FloatKeys.Count: " + AlphaKeys.Count);
    }

    public Vector3 SamplePosition(float frame)
    {
        for (int i = 0; i < TransformKey.TKey.Count - 1; i++)
        {
            if (frame >= TransformKey.TKey[i].frame && frame < TransformKey.TKey[i + 1].frame)
            {
                Vector3 pos1 = TransformKey.TKey[i].Translation;
                Vector3 pos2 = TransformKey.TKey[i + 1].Translation;
                float range = TransformKey.TKey[i + 1].frame - TransformKey.TKey[i].frame;
                float t = frame - TransformKey.TKey[i].frame;
                return Vector3.Lerp(pos1, pos2, t / range);
            }
        }
        return TransformKey.Translation;
    }
    public Quaternion SampleRotation(float frame)
    {
        for (int i = 0; i < TransformKey.RKey.Count - 1; i++)
        {
            if (frame >= TransformKey.RKey[i].frame && frame < TransformKey.RKey[i + 1].frame)
            {
                Quaternion rot1 = TransformKey.RKey[i].Rotation;
                Quaternion rot2 = TransformKey.RKey[i + 1].Rotation;
                float range = TransformKey.RKey[i + 1].frame - TransformKey.RKey[i].frame;
                float t = frame - TransformKey.RKey[i].frame;
                return Quaternion.Slerp(rot1, rot2, t / range);
            }
        }
        return TransformKey.Rotation;
    }
    public Vector3 SampleScale(float frame)
    {
        for (int i = 0; i < TransformKey.SKey.Count - 1; i++)
        {
            if (frame >= TransformKey.SKey[i].frame && frame < TransformKey.SKey[i + 1].frame)
            {
                Vector3 sca1 = TransformKey.SKey[i].Scale;
                Vector3 sca2 = TransformKey.SKey[i + 1].Scale;
                float range = TransformKey.SKey[i + 1].frame - TransformKey.SKey[i].frame;
                float t = frame - TransformKey.SKey[i].frame;
                return Vector3.Lerp(sca1, sca2, t / range);
            }
        }
        return TransformKey.Scale;
    }

    public float SampleTransparency(float frame)
    {
		if (AlphaKeys.Count == 0)
            return 1;
        if (frame < AlphaKeys.First().frame) return AlphaKeys.First().Alpha;
       
        for (int i = 0; i < AlphaKeys.Count - 1; i++)
        {
            if (frame >= AlphaKeys[i].frame && frame < AlphaKeys[i + 1].frame)
            {
                float tra1 = AlphaKeys[i].Alpha;
                float tra2 = AlphaKeys[i + 1].Alpha;
                float range = AlphaKeys[i + 1].frame - AlphaKeys[i].frame;
                float t = frame - AlphaKeys[i].frame;
                return Mathf.Lerp(tra1, tra2, t / range);
            }
        }
        return AlphaKeys.Last().Alpha;
    }

    public bool KeyTranslation(Vector3 pos, int frame)
    {
        for (int i = 0; i < TransformKey.TKey.Count; i++)
        {
			if (TransformKey.TKey[i].frame == frame)
			{
                TransformKey.TKey[i] = new TKey(frame, pos);
                return false;
            }
            if (TransformKey.TKey[i].frame > frame)
            {
                TransformKey.TKey.Insert(i, new TKey(frame, pos));
                return true;
            }
        }
        TransformKey.TKey.Add(new TKey(frame, pos));
        return true;
    }
    public bool KeyRotation(Quaternion rot, int frame)
    {
        for (int i = 0; i < TransformKey.RKey.Count; i++)
        {
            if (TransformKey.RKey[i].frame == frame)
            {
                TransformKey.RKey[i] = new RKey(frame, rot);
                return false;
            }
            if (TransformKey.RKey[i].frame > frame)
            {

                TransformKey.RKey.Insert(i, new RKey(frame, rot));
                return true;
            }
        }
        TransformKey.RKey.Add(new RKey(frame, rot));
        return true;
    }
    public bool KeyScale(Vector3 sca, int frame)
    {
        for (int i = 0; i < TransformKey.SKey.Count; i++)
        {
            if (TransformKey.SKey[i].frame == frame)
            {
                TransformKey.SKey[i] = new SKey(frame, sca);
                return false;
            }
            if (TransformKey.SKey[i].frame > frame)
            {
                TransformKey.SKey.Insert(i, new SKey(frame, sca));
                return true;
            }
        }
        var sKey = new SKey(frame, sca);
        TransformKey.SKey.Add(sKey);
        return true;
    }

    public void RemoveTranslationKey(int index)
    {
        TransformKey.TKey.RemoveAt(index);
    }
    public void RemoveRotationKey(int index)
    {
        TransformKey.RKey.RemoveAt(index);
    }
    public void RemoveScaleKey(int index)
    {
        TransformKey.SKey.RemoveAt(index);
    }
}

[Serializable]
public class TransformKey : IManualSerializer
{
    public Vector3 Translation;
    public Quaternion Rotation;
    public Vector3 Scale;

    public List<TKey> TKey;
    public List<RKey> RKey;
    public List<SKey> SKey;

    public TransformKey()
    {
        Translation = Vector3.zero;
        Rotation = Quaternion.identity;
        Scale = Vector3.one;

        TKey = new List<TKey>();
        RKey = new List<RKey>();
        SKey = new List<SKey>();
    }

    public void Serialize(Stream stream)
    {
		using var w = stream.ToBinaryWriter(true);
		w.Write(Translation.x);
		w.Write(Translation.y);
		w.Write(Translation.z);

		w.Write(Rotation.x);
		w.Write(Rotation.y);
		w.Write(Rotation.z);
		w.Write(Rotation.w);

		w.Write(Scale.x);
		w.Write(Scale.y);
		w.Write(Scale.z);

		w.Write(TKey.Count);
		foreach (var tkey in TKey)
		{
			w.Write((uint)tkey.frame);

			w.Write(tkey.Translation.x);
			w.Write(tkey.Translation.y);
			w.Write(tkey.Translation.z);
		}

		w.Write(RKey.Count);
		foreach (var rkey in RKey)
		{
			w.Write((uint)rkey.frame);

			w.Write(rkey.Rotation.x);
			w.Write(rkey.Rotation.y);
			w.Write(rkey.Rotation.z);
			w.Write(rkey.Rotation.w);
		}

		w.Write(SKey.Count);
		foreach (var skey in SKey)
		{
			w.Write((uint)skey.frame);

			w.Write(skey.Scale.x);
			w.Write(skey.Scale.y);
			w.Write(skey.Scale.z);
		}
        NetsphereScnTool.Scene.SceneContainer.Log("Translation: " + Translation);
        NetsphereScnTool.Scene.SceneContainer.Log("Rotation: " + Rotation);
        NetsphereScnTool.Scene.SceneContainer.Log("Scale: " + Scale);
        NetsphereScnTool.Scene.SceneContainer.Log("TKey.Count: " + TKey.Count);
        NetsphereScnTool.Scene.SceneContainer.Log("RKey.Count: " + RKey.Count);
        NetsphereScnTool.Scene.SceneContainer.Log("SKey.Count: " + SKey.Count);
    }

    public void Deserialize(Stream stream)
    {
		using var r = stream.ToBinaryReader(true);
		Translation = new Vector3(r.ReadSingle(), r.ReadSingle(), r.ReadSingle());
		Rotation = new Quaternion(r.ReadSingle(), r.ReadSingle(), r.ReadSingle(), r.ReadSingle());
		Scale = new Vector3(r.ReadSingle(), r.ReadSingle(), r.ReadSingle());

		uint count = r.ReadUInt32();
		for (int n = 0; n < count; n++)
			TKey.Add(new TKey { frame = (int)r.ReadUInt32(), Translation = new Vector3(r.ReadSingle(), r.ReadSingle(), r.ReadSingle()) });

		count = r.ReadUInt32();
		for (int n = 0; n < count; n++)
			RKey.Add(new RKey { frame = (int)r.ReadUInt32(), Rotation = new Quaternion(r.ReadSingle(), r.ReadSingle(), r.ReadSingle(), r.ReadSingle()) });

		count = r.ReadUInt32();
		for (int n = 0; n < count; n++)
			SKey.Add(new SKey { frame = (int)r.ReadUInt32(), Scale = new Vector3(r.ReadSingle(), r.ReadSingle(), r.ReadSingle()) });

        NetsphereScnTool.Scene.SceneContainer.Log("Translation: " + Translation);
        NetsphereScnTool.Scene.SceneContainer.Log("Rotation: " + Rotation);
        NetsphereScnTool.Scene.SceneContainer.Log("Scale: " + Scale);
        NetsphereScnTool.Scene.SceneContainer.Log("TKey.Count: " + TKey.Count);
        NetsphereScnTool.Scene.SceneContainer.Log("RKey.Count: " + RKey.Count);
        NetsphereScnTool.Scene.SceneContainer.Log("SKey.Count: " + SKey.Count);
    }
}

[Serializable]
public struct TKey
{
    public TKey(int frame, Vector3 Translation) { this.frame = frame; this.Translation = Translation; }
    public int frame;
    public Vector3 Translation;
    public void SetTranslation(Vector3 newTranslation)
	{
        Translation = newTranslation;
	}
}

[Serializable]
public struct RKey
{
    public RKey(int frame, Quaternion Rotation) { this.frame = frame; this.Rotation = Rotation; }
    public int frame;
    public Quaternion Rotation;
    public void SetRotation(Quaternion newRotation)
    {
        Rotation = newRotation;
    }
}

[Serializable]
public struct SKey
{
    public SKey(int frame, Vector3 Scale) { this.frame = frame; this.Scale = Scale; }
    public int frame;
    public Vector3 Scale;
    public void SetScale(Vector3 newScale)
    {
        Scale = newScale;
    }
}

[Serializable]
public class FloatKey : IManualSerializer
{
    public int frame;
    public float Alpha;

    public void Serialize(Stream stream)
    {
        using (var w = stream.ToBinaryWriter(true))
        {
            w.Write((uint)frame);
            w.Write(Alpha);
        }
    }

    public void Deserialize(Stream stream)
    {
        using (var r = stream.ToBinaryReader(true))
        {
            frame = (int)r.ReadUInt32();
            Alpha = r.ReadSingle();
        }
    }
}

[Serializable]
public class MorphKey : IManualSerializer
{
    public int frame;
    public List<VertexMorph> Vertices;
    public List<UVMorph> UVs;

    public MorphKey()
    {
        Vertices = new List<VertexMorph>();
        UVs = new List<UVMorph>();
    }

    public void Serialize(Stream stream)
    {
        using (var w = stream.ToBinaryWriter(true))
        {
            w.Write((uint)frame);

            w.Write(Vertices.Count);
            foreach (var vertexMorph in Vertices)
            {
                w.Write(vertexMorph.index);
                w.Write(vertexMorph.position.x);
                w.Write(vertexMorph.position.y);
                w.Write(vertexMorph.position.z);
            }

            w.Write(UVs.Count);
            foreach (var uvMorph in UVs)
            {
                w.Write(uvMorph.index);
                w.Write(uvMorph.position.x);
                w.Write(uvMorph.position.y);
            }
        }
    }

    public void Deserialize(Stream stream)
    {
        using (var r = stream.ToBinaryReader(true))
        {
            frame = (int)r.ReadUInt32();

            int count = r.ReadInt32();
            for (int j = 0; j < count; j++)
                Vertices.Add(new VertexMorph(r.ReadUInt32(), new Vector3(r.ReadSingle(), r.ReadSingle(), r.ReadSingle())));

            count = r.ReadInt32();
            for (int j = 0; j < count; j++)
                UVs.Add(new UVMorph(r.ReadUInt32(),new Vector2(r.ReadSingle(), r.ReadSingle()) ));
        }
    }
    [System.Serializable]
    public struct VertexMorph
    {
        public uint index;
        public Vector3 position;
        public VertexMorph(uint index, Vector3 position)
        {
            this.index = index;
            this.position = position;
        }
    }
    [System.Serializable]
    public struct UVMorph
    {
        public uint index;
        public Vector2 position;
        public UVMorph(uint index, Vector2 position)
        {
            this.index = index;
            this.position = position;
        }
    }
}