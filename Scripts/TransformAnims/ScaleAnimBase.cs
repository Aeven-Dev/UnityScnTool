using AevenScnTool;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ScaleAnimBase : ParametricAnim
{
    public override void KeyModelAt(TransformKeyData2 keyData, int frame, float factor)
    {
        SKey key = new SKey();
        key.frame = frame;
        key.Scale = GetScale(factor);
        keyData.TransformKey.SKey.Add(key);
    }
    public override void KeyBoneAt(TransformKeyData keyData, int frame, float factor)
    {
    }
    public abstract Vector3 GetScale(float factor);

    public new void FromModelAnimation(List<ModelAnimation> anims)
    {
        Debug.LogError("This shouldnt be posible! :o");
    }
    public new void FromBoneAnimation(List<BoneAnimation> anims)
    {
        Debug.LogError("This shouldnt be posible! :o");
    }

}