using AevenScnTool;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class VertexAnimBase : ParametricAnim
{
    Vector3[] verts;

	public override void Setup()
	{
        verts = GetComponent<MeshFilter>().sharedMesh.vertices;
    }


	public override void KeyModelAt(TransformKeyData2 keyData, int frame, float factor)
	{
        MorphKey key = new MorphKey();
        key.frame = frame;

        for (int j = 0; j < verts.Length; j++)
        {
            key.Vertices.Add(GetVert(j, factor, verts));
        }
        keyData.MorphKeys.Add(key);
    }

    public override void KeyBoneAt(TransformKeyData keyData, int frame, float factor)
    {
    }

    public abstract MorphKey.VertexMorph GetVert(int index, float factor, Vector3[] verts);


    public new void FromModelAnimation(List<ModelAnimation> anims)
    {
        Debug.LogError("This shouldnt be posible! :o");
    }
    public new void FromBoneAnimation(List<BoneAnimation> anims)
    {
        Debug.LogError("This shouldnt be posible! :o");
    }
}
