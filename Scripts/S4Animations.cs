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
            animations.Add(new S4Animation(anims[i]));
		}
    }
    public void FromBoneAnimation(List<BoneAnimation> anims)
    {
        for (int i = 0; i < anims.Count; i++)
        {
            animations.Add(new S4Animation(anims[i]));
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
        TransformKeyData.duration = anim.TransformKeyData2.duration;
        TransformKeyData.FloatKeys = anim.TransformKeyData2.FloatKeys;
        TransformKeyData.TransformKey = anim.TransformKeyData2.TransformKey;
        MorphKeys = anim.TransformKeyData2.MorphKeys;
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
        anim.TransformKeyData2 = new TransformKeyData2();
        anim.TransformKeyData2.duration = TransformKeyData.duration;
        anim.TransformKeyData2.FloatKeys = new List<FloatKey>();

        for (int i = 0; i < TransformKeyData.FloatKeys.Count; i++)
        {
            FloatKey key = new FloatKey();
            key.duration = TransformKeyData.FloatKeys[i].duration;
            key.Alpha = TransformKeyData.FloatKeys[i].Alpha;
            anim.TransformKeyData2.FloatKeys.Add(key);
        }
        anim.TransformKeyData2.TransformKey = new TransformKey();
        anim.TransformKeyData2.TransformKey.Translation = TransformKeyData.TransformKey.Translation;
        anim.TransformKeyData2.TransformKey.Rotation = TransformKeyData.TransformKey.Rotation;
        anim.TransformKeyData2.TransformKey.Scale = TransformKeyData.TransformKey.Scale;
        for (int i = 0; i < TransformKeyData.TransformKey.TKey.Count; i++)
        {
            TKey key = new TKey(TransformKeyData.TransformKey.TKey[i].duration, TransformKeyData.TransformKey.TKey[i].Translation);
            anim.TransformKeyData2.TransformKey.TKey.Add(key);
        }
        for (int i = 0; i < TransformKeyData.TransformKey.RKey.Count; i++)
        {
            RKey key = new RKey(TransformKeyData.TransformKey.RKey[i].duration, TransformKeyData.TransformKey.RKey[i].Rotation);
            anim.TransformKeyData2.TransformKey.RKey.Add(key);
        }
        for (int i = 0; i < TransformKeyData.TransformKey.SKey.Count; i++)
        {
            SKey key = new SKey(TransformKeyData.TransformKey.SKey[i].duration, TransformKeyData.TransformKey.SKey[i].Scale);
            anim.TransformKeyData2.TransformKey.SKey.Add(key);
        }
        anim.TransformKeyData2.MorphKeys = new List<MorphKey>(); 
        for (int i = 0; i < MorphKeys.Count; i++)
        {
            MorphKey key = new MorphKey();
            key.duration = MorphKeys[i].duration;
            key.Vertices = new List<MorphKey.VertexMorph>();
            for (int j = 0; j < MorphKeys[i].Vertices.Count; j++)
            {
                key.Vertices.Add(new MorphKey.VertexMorph(MorphKeys[i].Vertices[j].index, MorphKeys[i].Vertices[j].position));

            }
            for (int j = 0; j < MorphKeys[i].Vertices.Count; j++)
            {
                key.UVs.Add(new MorphKey.UVMorph(MorphKeys[i].UVs[j].index, MorphKeys[i].UVs[j].position));

            }
            anim.TransformKeyData2.MorphKeys.Add(key);
        }
        return anim;
    }
    public BoneAnimation ToBoneAnim()
    {
        BoneAnimation anim = new BoneAnimation();
        anim.Name = Name;
        anim.Copy = Copy;
        anim.TransformKeyData = new TransformKeyData2();
        anim.TransformKeyData.duration = TransformKeyData.duration;
        anim.TransformKeyData.FloatKeys = new List<FloatKey>();

        for (int i = 0; i < TransformKeyData.FloatKeys.Count; i++)
        {
            FloatKey key = new FloatKey();
            key.duration = TransformKeyData.FloatKeys[i].duration;
            key.Alpha = TransformKeyData.FloatKeys[i].Alpha;
            anim.TransformKeyData.FloatKeys.Add(key);
        }
        anim.TransformKeyData.TransformKey = new TransformKey();
        anim.TransformKeyData.TransformKey.Translation = TransformKeyData.TransformKey.Translation;
        anim.TransformKeyData.TransformKey.Rotation = TransformKeyData.TransformKey.Rotation;
        anim.TransformKeyData.TransformKey.Scale = TransformKeyData.TransformKey.Scale;
        for (int i = 0; i < TransformKeyData.TransformKey.TKey.Count; i++)
        {
            TKey key = new TKey(TransformKeyData.TransformKey.TKey[i].duration, TransformKeyData.TransformKey.TKey[i].Translation);
            anim.TransformKeyData.TransformKey.TKey.Add(key);
        }
        for (int i = 0; i < TransformKeyData.TransformKey.RKey.Count; i++)
        {
            RKey key = new RKey(TransformKeyData.TransformKey.RKey[i].duration, TransformKeyData.TransformKey.RKey[i].Rotation);
            anim.TransformKeyData.TransformKey.RKey.Add(key);
        }
        for (int i = 0; i < TransformKeyData.TransformKey.SKey.Count; i++)
        {
            SKey key = new SKey(TransformKeyData.TransformKey.SKey[i].duration, TransformKeyData.TransformKey.SKey[i].Scale);
            anim.TransformKeyData.TransformKey.SKey.Add(key);
        }
        return anim;
    }
}
