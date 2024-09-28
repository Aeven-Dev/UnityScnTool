using AevenScnTool;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ParametricAnim : S4Animations
{
    public enum Easing { Linear, EaseIn, EaseOut, EaseInOut, Step }
    public int stepDuration = 0;
    public int repetitions = 0;
    public Easing easing;

    public new List<ModelAnimation> ToModelAnimation()
    {
        Setup();
        var anims = base.ToModelAnimation();
        ModelAnimation anim = new ModelAnimation();
        anim.Name = ScnToolData.Instance.main_animation_name;
        anim.transformKeyData2 = new TransformKeyData2();
        anim.transformKeyData2.duration = stepDuration * repetitions;
        anim.transformKeyData2.AlphaKeys = new List<FloatKey>();

        anim.transformKeyData2.TransformKey = new TransformKey();
        anim.transformKeyData2.TransformKey.Translation = transform.position * ScnToolData.Instance.scale;
        anim.transformKeyData2.TransformKey.Rotation = transform.rotation;
        anim.transformKeyData2.TransformKey.Scale = transform.lossyScale;
        anim.transformKeyData2.MorphKeys = new List<MorphKey>();

        for (int i = 0; i < repetitions; i++)
        {
            ProcessRepetition(i, anim.transformKeyData2, (keyData, frame, factor) => KeyModelAt(keyData as TransformKeyData2, frame, factor));
        }
        anims.Add(anim);
        return anims;
    }

    public virtual void Setup() { }

    public new List<BoneAnimation> ToBoneAnimation()
    {
        Setup();

        var anims = base.ToBoneAnimation();
        BoneAnimation anim = new BoneAnimation();
        anim.Name = ScnToolData.Instance.main_animation_name;
        anim.TransformKeyData = new TransformKeyData();
        anim.TransformKeyData.duration = stepDuration;
        anim.TransformKeyData.AlphaKeys = new List<FloatKey>();

        anim.TransformKeyData.TransformKey = new TransformKey();
        anim.TransformKeyData.TransformKey.Translation = transform.position * ScnToolData.Instance.scale;
        anim.TransformKeyData.TransformKey.Rotation = transform.rotation;
        anim.TransformKeyData.TransformKey.Scale = transform.lossyScale;

        for (int i = 0; i < repetitions; i++)
        {
            ProcessRepetition(i, anim.TransformKeyData, KeyBoneAt);
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

    void ProcessRepetition(int step, TransformKeyData keyData, Action<TransformKeyData, int, float> function)
    {
        switch (easing)
        {
            case Easing.Linear:
                LinearEase(step, keyData, function);

                break;
            case Easing.EaseIn:
                EaseInEase(step, keyData, function);
                break;
            case Easing.EaseOut:
                EaseOutEase(step, keyData, function);
                break;
            case Easing.EaseInOut:
                EaseInOutEase(step, keyData, function);
                break;
            case Easing.Step:
                StepEase(step, keyData, function);
                break;
        }
    }


    void LinearEase(int step, TransformKeyData keyData, Action<TransformKeyData, int, float> function)
    {
        function(keyData, step * stepDuration, step);
        function(keyData, (step + 1) * stepDuration, step + 1);
    }

    void StepEase(int step, TransformKeyData keyData, Action<TransformKeyData, int, float> function)
    {
        function(keyData, step * stepDuration, step);
        function(keyData, (step + 1) * stepDuration - 1, step);
    }

    void EaseInEase(int step, TransformKeyData keyData, Action<TransformKeyData, int, float> function)
    {
        function(keyData, step * stepDuration, step);
        function(keyData, step * stepDuration + (int)(stepDuration * 0.5f), step + 0.25f);
        function(keyData, step * stepDuration + (int)(stepDuration * 0.75f), step + 0.5f);
        function(keyData, step * stepDuration + (int)(stepDuration * 0.875f), step + 0.75f);
        function(keyData, (step + 1) * stepDuration, step + 1);
    }

    void EaseOutEase(int step, TransformKeyData keyData, Action<TransformKeyData, int, float> function)
    {
        function(keyData, step * stepDuration, step);
        function(keyData, step * stepDuration + (int)(stepDuration * 0.125f), step + 0.25f);
        function(keyData, step * stepDuration + (int)(stepDuration * 0.25f), step + 0.5f);
        function(keyData, step * stepDuration + (int)(stepDuration * 0.5f), step + 0.75f);
        function(keyData, (step + 1) * stepDuration, step + 1);
    }

    void EaseInOutEase(int step, TransformKeyData keyData, Action<TransformKeyData, int, float> function)
    {
        function(keyData, step * stepDuration, step);
        function(keyData, step * stepDuration + (int)(stepDuration * 0.125f), step + 0.05f);
        function(keyData, step * stepDuration + (int)(stepDuration * 0.25f), step + 0.15f);
        function(keyData, step * stepDuration + (int)(stepDuration * 0.5f), step + 0.5f);
        function(keyData, step * stepDuration + (int)(stepDuration * 0.75f), step + 0.85f);
        function(keyData, step * stepDuration + (int)(stepDuration * 0.875f), step + 0.95f);
        function(keyData, (step + 1) * stepDuration, step + 1);
    }

    public virtual void KeyModelAt(TransformKeyData2 keyData, int frame, float factor) { }
    public virtual void KeyBoneAt(TransformKeyData keyData, int frame, float factor) { }
}
