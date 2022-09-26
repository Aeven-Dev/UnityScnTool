using AevenScnTool;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class RotateAnimBase : S4Animations
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

        for (int i = 0; i < repetitions; i++)
        {
            ProcessRepetition(i, anim.TransformKeyData2.TransformKey);
        }
        anims.Add(anim);
        return anims;
    }

    public new List<BoneAnimation> ToBoneAnimation()
    {
        var anims = new List<BoneAnimation>();
        BoneAnimation anim = new BoneAnimation();
        anim.Name = ScnToolData.Instance.main_animation_name;
        anim.TransformKeyData = new TransformKeyData();
        anim.TransformKeyData.duration = stepDuration;
        anim.TransformKeyData.FloatKeys = new List<FloatKey>();

        anim.TransformKeyData.TransformKey = new TransformKey();
        anim.TransformKeyData.TransformKey.Translation = transform.position * ScnToolData.Instance.scale;
        anim.TransformKeyData.TransformKey.Rotation = transform.rotation;
        anim.TransformKeyData.TransformKey.Scale = transform.lossyScale;

        for (int i = 0; i < repetitions; i++)
        {
            ProcessRepetition(i, anim.TransformKeyData.TransformKey);
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

    void ProcessRepetition(int step, TransformKey tKeys)
    {
        switch (easing)
        {
            case Easing.Linear:
                LinearEase(step, tKeys);

                break;
            case Easing.EaseIn:
                EaseInEase(step, tKeys);
                break;
            case Easing.EaseOut:
                EaseOutEase(step, tKeys);
                break;
            case Easing.EaseInOut:
                EaseInOutEase(step, tKeys);
                break;
            case Easing.Step:
                StepEase(step, tKeys);
                break;
        }
    }

    void LinearEase(int step, TransformKey mKeys)
    {
        mKeys.RKey.Add(KeyAt(step * stepDuration, step));
        mKeys.RKey.Add(KeyAt((step + 1) * stepDuration, step + 1));
    }

    void StepEase(int step, TransformKey mKeys)
    {
        mKeys.RKey.Add(KeyAt(step * stepDuration, step));
        mKeys.RKey.Add(KeyAt((step + 1) * stepDuration - 1, step));
    }

    void EaseInEase(int step, TransformKey mKeys)
    {
        mKeys.RKey.Add(KeyAt(step * stepDuration, step));
        mKeys.RKey.Add(KeyAt(step * stepDuration + (int)(stepDuration * 0.5f), step + 0.25f));
        mKeys.RKey.Add(KeyAt(step * stepDuration + (int)(stepDuration * 0.75f), step + 0.5f));
        mKeys.RKey.Add(KeyAt(step * stepDuration + (int)(stepDuration * 0.875f), step + 0.75f));
        mKeys.RKey.Add(KeyAt((step + 1) * stepDuration, step + 1));
    }

    void EaseOutEase(int step, TransformKey mKeys)
    {
        mKeys.RKey.Add(KeyAt(step * stepDuration, step));
        mKeys.RKey.Add(KeyAt(step * stepDuration + (int)(stepDuration * 0.125f), step + 0.25f));
        mKeys.RKey.Add(KeyAt(step * stepDuration + (int)(stepDuration * 0.25f), step + 0.5f));
        mKeys.RKey.Add(KeyAt(step * stepDuration + (int)(stepDuration * 0.5f), step + 0.75f));
        mKeys.RKey.Add(KeyAt((step + 1) * stepDuration, step + 1));
    }

    void EaseInOutEase(int step, TransformKey mKeys)
    {
        mKeys.RKey.Add(KeyAt(step * stepDuration, step));
        mKeys.RKey.Add(KeyAt(step * stepDuration + (int)(stepDuration * 0.125f), step + 0.05f));
        mKeys.RKey.Add(KeyAt(step * stepDuration + (int)(stepDuration * 0.25f), step + 0.15f));
        mKeys.RKey.Add(KeyAt(step * stepDuration + (int)(stepDuration * 0.5f), step + 0.5f));
        mKeys.RKey.Add(KeyAt(step * stepDuration + (int)(stepDuration * 0.75f), step + 0.85f));
        mKeys.RKey.Add(KeyAt(step * stepDuration + (int)(stepDuration * 0.875f), step + 0.95f));
        mKeys.RKey.Add(KeyAt((step + 1) * stepDuration, step + 1));
    }

    RKey KeyAt(int frame, float factor)
    {
        RKey key = new RKey();
        key.frame = frame;


        key.Rotation = GetRotation(factor);
        return key;
    }

    public abstract Quaternion GetRotation(float factor);

    public new void FromModelAnimation(List<ModelAnimation> anims)
    {
        Debug.LogError("This shouldnt be posible! :o");
    }
    public new void FromBoneAnimation(List<BoneAnimation> anims)
    {
        Debug.LogError("This shouldnt be posible! :o");
    }

}
