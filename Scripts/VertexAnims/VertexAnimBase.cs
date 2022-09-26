using AevenScnTool;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class VertexAnimBase : S4Animations
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


        Vector3[] verts = GetComponent<MeshFilter>().sharedMesh.vertices;

        for (int i = 0; i < repetitions; i++)
        {
            ProcessRepetition(i, verts, anim.TransformKeyData2.MorphKeys);
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

    void ProcessRepetition(int step, Vector3[] verts, List<MorphKey> mKeys)
    {
        switch (easing)
        {
            case Easing.Linear:
                LinearEase(step, verts, mKeys);

                break;
            case Easing.EaseIn:
                EaseInEase(step, verts, mKeys);
                break;
            case Easing.EaseOut:
                EaseOutEase(step, verts, mKeys);
                break;
            case Easing.EaseInOut:
                EaseInOutEase(step, verts, mKeys);
                break;
            case Easing.Step:
                StepEase(step, verts, mKeys);
                break;
        }
    }

    void LinearEase(int step, Vector3[] verts, List<MorphKey> mKeys)
    {
        mKeys.Add(KeyAt(step * stepDuration, step, verts));
        mKeys.Add(KeyAt((step + 1) * stepDuration, step + 1, verts));
    }

    void StepEase(int step, Vector3[] verts, List<MorphKey> mKeys)
    {
        mKeys.Add(KeyAt(step * stepDuration, step, verts));
        mKeys.Add(KeyAt((step + 1) * stepDuration - 1, step, verts));
    }

    void EaseInEase(int step, Vector3[] verts, List<MorphKey> mKeys)
    {
        mKeys.Add(KeyAt(step * stepDuration, step, verts));
        mKeys.Add(KeyAt(step * stepDuration + (int)(stepDuration * 0.5f), step + 0.25f, verts));
        mKeys.Add(KeyAt(step * stepDuration + (int)(stepDuration * 0.75f), step + 0.5f, verts));
        mKeys.Add(KeyAt(step * stepDuration + (int)(stepDuration * 0.875f), step + 0.75f, verts));
        mKeys.Add(KeyAt((step + 1) * stepDuration, step + 1, verts));
    }

    void EaseOutEase(int step, Vector3[] verts, List<MorphKey> mKeys)
    {
        mKeys.Add(KeyAt(step * stepDuration, step, verts));
        mKeys.Add(KeyAt(step * stepDuration + (int)(stepDuration * 0.125f), step + 0.25f, verts));
        mKeys.Add(KeyAt(step * stepDuration + (int)(stepDuration * 0.25f), step + 0.5f, verts));
        mKeys.Add(KeyAt(step * stepDuration + (int)(stepDuration * 0.5f), step + 0.75f, verts));
        mKeys.Add(KeyAt((step + 1) * stepDuration, step + 1, verts));
    }

    void EaseInOutEase(int step, Vector3[] verts, List<MorphKey> mKeys)
    {
        mKeys.Add(KeyAt(step * stepDuration, step, verts));
        mKeys.Add(KeyAt(step * stepDuration + (int)(stepDuration * 0.125f), step + 0.05f, verts));
        mKeys.Add(KeyAt(step * stepDuration + (int)(stepDuration * 0.25f), step + 0.15f, verts));
        mKeys.Add(KeyAt(step * stepDuration + (int)(stepDuration * 0.5f), step + 0.5f, verts));
        mKeys.Add(KeyAt(step * stepDuration + (int)(stepDuration * 0.75f), step + 0.85f, verts));
        mKeys.Add(KeyAt(step * stepDuration + (int)(stepDuration * 0.875f), step + 0.95f, verts));
        mKeys.Add(KeyAt((step + 1) * stepDuration, step + 1, verts));
    }

    MorphKey KeyAt(int frame, float factor, Vector3[] verts)
    {
        MorphKey key = new MorphKey();
        key.frame = frame;

        for (int j = 0; j < verts.Length; j++)
        {
            key.Vertices.Add(GetVert(j, factor, verts));

        }
        return key;
    }

    public abstract MorphKey.VertexMorph GetVert(int index, float factor, Vector3[] verts);


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
