using AevenScnTool;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class RotateAnimBase : ParametricAnim
{
    public override void KeyModelAt(TransformKeyData2 keyData, int frame, float factor)
    {
        RKey key = new RKey();
        key.frame = frame;
        key.Rotation = GetRotation(factor);
        keyData.TransformKey.RKey.Add( key);
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
