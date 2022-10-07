using AevenScnTool;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[AddComponentMenu("S4 scn/Animations!/UVScroll")]
[System.Serializable]
public class UVScroll : UVAnimBase
{
    public Vector2 offset;

    public override MorphKey.UVMorph GetUV(int index, float factor, Vector2[] uvs)
	{
        return new MorphKey.UVMorph((uint)index, uvs[index] + offset * factor);

    }
}
