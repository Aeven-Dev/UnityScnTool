using AevenScnTool;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public abstract class UVAnimBase : S4Animations
{
    public enum Easing { Linear, EaseIn, EaseOut, EaseInOut, Step }
    public int stepDuration = 0;
    public int repetitions = 0;
    public Easing easing;

    public new List<ModelAnimation> ToModelAnimation()
    {
        var anims = new List<ModelAnimation>();
        ModelAnimation anim = new ModelAnimation();
        anim.Name = ScnToolData.Instance.main_animation_name;
        anim.TransformKeyData2 = new TransformKeyData2();
        anim.TransformKeyData2.duration = stepDuration;
        anim.TransformKeyData2.FloatKeys = new List<FloatKey>();

        anim.TransformKeyData2.TransformKey = new TransformKey();
        anim.TransformKeyData2.TransformKey.Translation = transform.position * ScnToolData.Instance.scale;
        anim.TransformKeyData2.TransformKey.Rotation = transform.rotation;
        anim.TransformKeyData2.TransformKey.Scale = transform.lossyScale;
        anim.TransformKeyData2.MorphKeys = new List<MorphKey>();


        Vector2[] uvs = GetComponent<MeshFilter>().sharedMesh.uv;

        for (int i = 0; i < repetitions; i++)
        {
            ProcessRepetition(i, uvs, anim.TransformKeyData2.MorphKeys);
        }
        anims.Add(anim);
        return anims;
    }
    public new S4Animation GetAnimation(string name)
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

    void ProcessRepetition(int step, Vector2[] uvs, List<MorphKey> mKeys)
	{
		switch (easing)
		{
			case Easing.Linear:
                LinearEase(step, uvs, mKeys);

                break;
			case Easing.EaseIn:
                EaseInEase(step, uvs, mKeys);
                break;
			case Easing.EaseOut:
                EaseOutEase(step, uvs, mKeys);
                break;
			case Easing.EaseInOut:
                EaseInOutEase(step, uvs, mKeys);
                break;
			case Easing.Step:
                StepEase(step, uvs, mKeys);
                break;
		}
	}

    void LinearEase(int step, Vector2[] uvs, List<MorphKey> mKeys)
    {
        mKeys.Add(KeyAt(step * stepDuration, step, uvs));
        mKeys.Add(KeyAt((step + 1) * stepDuration, step + 1, uvs));
    }

    void StepEase(int step, Vector2[] uvs, List<MorphKey> mKeys)
    {
        mKeys.Add(KeyAt(step * stepDuration, step, uvs));
        mKeys.Add(KeyAt((step + 1) * stepDuration - 1, step, uvs));
    }

    void EaseInEase(int step, Vector2[] uvs, List<MorphKey> mKeys)
    {
        mKeys.Add(KeyAt(step * stepDuration, step, uvs));
        mKeys.Add(KeyAt(step * stepDuration + (int)(stepDuration * 0.5f), step + 0.25f, uvs));
        mKeys.Add(KeyAt(step * stepDuration + (int)(stepDuration * 0.75f), step + 0.5f, uvs));
        mKeys.Add(KeyAt(step * stepDuration + (int)(stepDuration * 0.875f), step + 0.75f, uvs));
        mKeys.Add(KeyAt((step + 1) * stepDuration, step + 1, uvs));
    }

    void EaseOutEase(int step, Vector2[] uvs, List<MorphKey> mKeys)
    {
        mKeys.Add(KeyAt(step * stepDuration, step, uvs));
        mKeys.Add(KeyAt(step * stepDuration + (int)(stepDuration * 0.125f), step + 0.25f, uvs));
        mKeys.Add(KeyAt(step * stepDuration + (int)(stepDuration * 0.25f), step + 0.5f, uvs));
        mKeys.Add(KeyAt(step * stepDuration + (int)(stepDuration * 0.5f), step + 0.75f, uvs));
        mKeys.Add(KeyAt((step + 1) * stepDuration, step + 1, uvs));
    }

    void EaseInOutEase(int step, Vector2[] uvs, List<MorphKey> mKeys)
    {
        mKeys.Add(KeyAt(step * stepDuration, step, uvs));
        mKeys.Add(KeyAt(step * stepDuration + (int)(stepDuration * 0.125f), step + 0.05f, uvs));
        mKeys.Add(KeyAt(step * stepDuration + (int)(stepDuration * 0.25f), step + 0.15f, uvs));
        mKeys.Add(KeyAt(step * stepDuration + (int)(stepDuration * 0.5f), step + 0.5f, uvs));
        mKeys.Add(KeyAt(step * stepDuration + (int)(stepDuration * 0.75f), step + 0.85f, uvs));
        mKeys.Add(KeyAt(step * stepDuration + (int)(stepDuration * 0.875f), step + 0.95f, uvs));
        mKeys.Add(KeyAt((step + 1) * stepDuration, step + 1, uvs));
    }

    MorphKey KeyAt(int frame, float factor,Vector2[] uvs)
	{
        MorphKey key = new MorphKey();
        key.frame = frame;

        for (int j = 0; j < uvs.Length; j++)
        {
            key.UVs.Add(GetUV(j, factor, uvs));

        }
        return key;
    }

    public abstract MorphKey.UVMorph GetUV(int index, float factor, Vector2[] uvs);


    public new List<BoneAnimation> ToBoneAnimation()
    {
        Debug.LogError("This isnt a bone animation silly!");
        return null;
    }
    public new void FromModelAnimation(List<ModelAnimation> anims)
    {
        Debug.LogError("This shouldnt be posible! :o");
    }
    public new void FromBoneAnimation(List<BoneAnimation> anims)
    {
        Debug.LogError("This shouldnt be posible! :o");
    }

}
