using AevenScnTool;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public abstract class TranslateAnimBase : ParametricAnim
{
    
    public override void KeyModelAt(TransformKeyData2 keyData, int frame, float factor)
    {
        TKey key = new TKey();
        key.frame = frame;
        key.Translation = GetTranslation(factor);
        keyData.TransformKey.TKey.Add(key);
    }
    public override void KeyBoneAt(TransformKeyData keyData, int frame, float factor)
    {
    }

    public abstract Vector3 GetTranslation(float factor);

    public new void FromModelAnimation(List<ModelAnimation> anims)
    {
        Debug.LogError("This shouldnt be posible! :o");
    }
    public new void FromBoneAnimation(List<BoneAnimation> anims)
    {
        Debug.LogError("This shouldnt be posible! :o");
    }

}

