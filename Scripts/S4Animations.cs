using AevenScnTool;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
[AddComponentMenu("S4 scn/S4 Animations")]
public class S4Animations : MonoBehaviour
{
    public List<S4Animation> animations = new List<S4Animation>();

    public List<ModelAnimation> ToModelAnimation()
    {
        List<ModelAnimation> anims = new List<ModelAnimation>();
        for (int i = 0; i < animations.Count; i++)
        {
            anims.Add(animations[i].ToModelAnim());
        }
        return anims;
    }
    public List<BoneAnimation> ToBoneAnimation()
    {
        List<BoneAnimation> anims = new List<BoneAnimation>();
        for (int i = 0; i < animations.Count; i++)
        {
            anims.Add(animations[i].ToBoneAnim());
        }
        return anims;
    }

    public void FromModelAnimation(List<ModelAnimation> anims)
    {
		for (int i = 0; i < anims.Count; i++)
		{
            var a = new S4Animation(anims[i]);
            a.TransformKeyData.TransformKey.Translation = a.TransformKeyData.TransformKey.Translation / 100;

            for (int t = 0; t < a.TransformKeyData.TransformKey.TKey.Count; t++)
			{
                a.TransformKeyData.TransformKey.TKey[t] = new TKey(a.TransformKeyData.TransformKey.TKey[t].frame, a.TransformKeyData.TransformKey.TKey[t].Translation / ScnToolData.Instance.scale);
            }

			for (int m = 0; m < a.MorphKeys.Count; m++)
			{
                var ver = a.MorphKeys[m].Vertices;
                for (int v = 0; v < ver.Count; v++)
				{
                    ver[v] = new MorphKey.VertexMorph(ver[v].index, ver[v].position / ScnToolData.Instance.scale);
				}
			}
            animations.Add(a);
		}
    }
    public void FromBoneAnimation(List<BoneAnimation> anims)
    {
        for (int i = 0; i < anims.Count; i++)
        {
            var a = new S4Animation(anims[i]);
			if (a.TransformKeyData != null)
            {
                a.TransformKeyData.TransformKey.Translation = a.TransformKeyData.TransformKey.Translation / ScnToolData.Instance.scale;

                for (int t = 0; t < a.TransformKeyData.TransformKey.TKey.Count; t++)
                {
                    a.TransformKeyData.TransformKey.TKey[t] = new TKey(a.TransformKeyData.TransformKey.TKey[t].frame, a.TransformKeyData.TransformKey.TKey[t].Translation / ScnToolData.Instance.scale);
                }
            }

            animations.Add(a);
        }
    }

    public S4Animation GetAnimation(string name)
	{
		foreach (var item in animations)
		{
			if (item.Name == name)
			{
                return item;
			}
		}
        return null;
	}
}

[System.Serializable]
public class S4Animation
{
    public string Name;
    public string Copy;
    public TransformKeyData TransformKeyData;

    public List<MorphKey> MorphKeys = null;

    public S4Animation() { }

    public S4Animation(ModelAnimation anim)
    {
        Name = anim.Name;
        TransformKeyData = new TransformKeyData();
        TransformKeyData.duration = anim.transformKeyData2.duration;
        TransformKeyData.AlphaKeys = anim.transformKeyData2.AlphaKeys;
        TransformKeyData.TransformKey = anim.transformKeyData2.TransformKey;
        MorphKeys = anim.transformKeyData2.MorphKeys;
    }
    public S4Animation(BoneAnimation anim)
    {
        Name = anim.Name;
        Copy = anim.Copy;
        TransformKeyData = anim.TransformKeyData;
    }

    public ModelAnimation ToModelAnim()
    {
        ModelAnimation anim = new ModelAnimation();
        anim.Name = Name;
        anim.transformKeyData2 = new TransformKeyData2();
        anim.transformKeyData2.duration = TransformKeyData.duration;
        anim.transformKeyData2.AlphaKeys = new List<FloatKey>();

        for (int i = 0; i < TransformKeyData.AlphaKeys.Count; i++)
        {
            FloatKey key = new FloatKey();
            key.frame = TransformKeyData.AlphaKeys[i].frame;
            key.Alpha = TransformKeyData.AlphaKeys[i].Alpha;
            anim.transformKeyData2.AlphaKeys.Add(key);
        }
        anim.transformKeyData2.TransformKey = new TransformKey();
        anim.transformKeyData2.TransformKey.Translation = TransformKeyData.TransformKey.Translation * ScnToolData.Instance.scale;
        anim.transformKeyData2.TransformKey.Rotation = TransformKeyData.TransformKey.Rotation;
        anim.transformKeyData2.TransformKey.Scale = TransformKeyData.TransformKey.Scale;
        for (int i = 0; i < TransformKeyData.TransformKey.TKey.Count; i++)
        {
            TKey key = new TKey(TransformKeyData.TransformKey.TKey[i].frame, TransformKeyData.TransformKey.TKey[i].Translation * ScnToolData.Instance.scale);
            anim.transformKeyData2.TransformKey.TKey.Add(key);
        }
        for (int i = 0; i < TransformKeyData.TransformKey.RKey.Count; i++)
        {
            RKey key = new RKey(TransformKeyData.TransformKey.RKey[i].frame, TransformKeyData.TransformKey.RKey[i].Rotation);
            anim.transformKeyData2.TransformKey.RKey.Add(key);
        }
        for (int i = 0; i < TransformKeyData.TransformKey.SKey.Count; i++)
        {
            SKey key = new SKey(TransformKeyData.TransformKey.SKey[i].frame, TransformKeyData.TransformKey.SKey[i].Scale);
            anim.transformKeyData2.TransformKey.SKey.Add(key);
        }
        anim.transformKeyData2.MorphKeys = new List<MorphKey>(); 
        for (int i = 0; i < MorphKeys.Count; i++)
        {
            MorphKey key = new MorphKey();
            key.frame = MorphKeys[i].frame;
            key.Vertices = new List<MorphKey.VertexMorph>();
            for (int j = 0; j < MorphKeys[i].Vertices.Count; j++)
            {
                key.Vertices.Add(new MorphKey.VertexMorph(MorphKeys[i].Vertices[j].index, MorphKeys[i].Vertices[j].position * ScnToolData.Instance.scale));

            }
            for (int j = 0; j < MorphKeys[i].UVs.Count; j++)
            {
                key.UVs.Add(new MorphKey.UVMorph(MorphKeys[i].UVs[j].index, MorphKeys[i].UVs[j].position));

            }
            anim.transformKeyData2.MorphKeys.Add(key);
        }
        return anim;
    }
    public BoneAnimation ToBoneAnim()
    {
        BoneAnimation anim = new BoneAnimation();
        anim.Name = Name;
        anim.Copy = Copy;
        anim.TransformKeyData = new TransformKeyData();
        anim.TransformKeyData.duration = TransformKeyData.duration;
        anim.TransformKeyData.AlphaKeys = new List<FloatKey>();

        for (int i = 0; i < TransformKeyData.AlphaKeys.Count; i++)
        {
            FloatKey key = new FloatKey();
            key.frame = TransformKeyData.AlphaKeys[i].frame;
            key.Alpha = TransformKeyData.AlphaKeys[i].Alpha;
            anim.TransformKeyData.AlphaKeys.Add(key);
        }
        anim.TransformKeyData.TransformKey = new TransformKey();
        anim.TransformKeyData.TransformKey.Translation = TransformKeyData.TransformKey.Translation * ScnToolData.Instance.scale;
        anim.TransformKeyData.TransformKey.Rotation = TransformKeyData.TransformKey.Rotation;
        anim.TransformKeyData.TransformKey.Scale = TransformKeyData.TransformKey.Scale;
        for (int i = 0; i < TransformKeyData.TransformKey.TKey.Count; i++)
        {
            TKey key = new TKey(TransformKeyData.TransformKey.TKey[i].frame, TransformKeyData.TransformKey.TKey[i].Translation * ScnToolData.Instance.scale);
            anim.TransformKeyData.TransformKey.TKey.Add(key);
        }
        for (int i = 0; i < TransformKeyData.TransformKey.RKey.Count; i++)
        {
            RKey key = new RKey(TransformKeyData.TransformKey.RKey[i].frame, TransformKeyData.TransformKey.RKey[i].Rotation);
            anim.TransformKeyData.TransformKey.RKey.Add(key);
        }
        for (int i = 0; i < TransformKeyData.TransformKey.SKey.Count; i++)
        {
            SKey key = new SKey(TransformKeyData.TransformKey.SKey[i].frame, TransformKeyData.TransformKey.SKey[i].Scale);
            anim.TransformKeyData.TransformKey.SKey.Add(key);
        }
        return anim;
    }

    public Vector2[] SampleUVs(int frame)
    {
		if (MorphKeys.Count == 0)
		{
            return null;
		}
        for (int i = 0; i < MorphKeys.Count - 1; i++)
        {
            MorphKey prev = MorphKeys[i];
            MorphKey next = MorphKeys[i + 1];

            if (frame >= prev.frame && frame < next.frame)
            {
                Vector2[] uvs = new Vector2[MorphKeys[i].UVs.Count];
                for (int u = 0; u < uvs.Length; u++)
                {
                    Vector2 pos1 = prev.UVs[u].position;
                    Vector2 pos2 = next.UVs[u].position;
                    float range = next.frame - prev.frame;
                    float t = frame - prev.frame;
                    uvs[u] = Vector2.Lerp(pos1, pos2, t / range);
                }
                return uvs;
            }
        }
        Vector2[] last_uvs = new Vector2[MorphKeys[MorphKeys.Count - 1].UVs.Count];
        for (int u = 0; u < last_uvs.Length; u++)
        {
            last_uvs[u] = MorphKeys[MorphKeys.Count - 1].UVs[u].position;
        }
        return last_uvs;
    }

    public bool KeyUVs(int frame ,Vector2[] data)
	{

        List<MorphKey.UVMorph> uvs = new();

        for (int u = 0; u < data.Length; u++)
        {
            uvs.Add(new MorphKey.UVMorph((uint)u, data[u]));
        }

        MorphKey mk = new MorphKey();
        mk.UVs = uvs;
        mk.frame = frame;

        for (int i = 0; i < MorphKeys.Count; i++)
        {
            if (MorphKeys[i].frame == frame)
            {
                MorphKeys[i] = mk;
                return false;
            }
            if (MorphKeys[i].frame > frame)
            {
                MorphKeys.Insert(i, mk);
                return true;
            }
        }
        MorphKeys.Add(mk);
        return true;
    }
}
