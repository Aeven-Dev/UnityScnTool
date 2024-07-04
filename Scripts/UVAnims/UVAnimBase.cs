using AevenScnTool;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public abstract class UVAnimBase : ParametricAnim
{
    Vector2[] uvs;

    public override void Setup()
	{
        uvs = GetComponent<MeshFilter>().sharedMesh.uv;
    }

	public override void KeyModelAt(TransformKeyData2 keyData, int frame, float factor)
	{
        MorphKey key = new MorphKey();
        key.frame = frame;

        for (int j = 0; j < uvs.Length; j++)
        {
            key.UVs.Add(GetUV(j, factor, uvs));

        }
        keyData.MorphKeys.Add(key);
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
