using AevenScnTool;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public abstract class UVAnimBase : ParametricAnim
{
    Vector2[] uvs;
    TextureReference tr;

    public override void Setup()
	{
        uvs = GetComponent<MeshFilter>().sharedMesh.uv;
        tr = GetComponent<TextureReference>();

    }

	public override void KeyModelAt(TransformKeyData2 keyData, int frame, float factor)
	{
        MorphKey key = new MorphKey();
        key.frame = frame;

        for (int j = 0; j < uvs.Length; j++)
        {
            MorphKey.UVMorph uv_morph = GetUV(j, factor, uvs);
            if (ScnToolData.Instance.uv_flipVertical_lm ^ tr.flipUvVertical_lm)
				{
                    uv_morph.position = new Vector2(uv_morph.position.x, -uv_morph.position.y);
				}
				if (ScnToolData.Instance.uv_flipHorizontal_lm ^ tr.flipUvHorizontal_lm)
				{
                    uv_morph.position = new Vector2(-uv_morph.position.x, uv_morph.position.y);
				}
            key.UVs.Add(uv_morph);

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
