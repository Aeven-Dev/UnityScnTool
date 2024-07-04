using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[AddComponentMenu("S4 scn/Animations!/UVScale")]
public class UVScale : UVAnimBase
{
	public Vector2 amount;

	public override MorphKey.UVMorph GetUV(int index, float factor, Vector2[] uvs)
	{
		return new MorphKey.UVMorph((uint)index, uvs[index] * factor * amount);
	}
}
