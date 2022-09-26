using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UVScale : UVAnimBase
{
	public Vector2 factor;

	public override MorphKey.UVMorph GetUV(int index, float factor, Vector2[] uvs)
	{
		return new MorphKey.UVMorph((uint)index, uvs[index] * factor * factor);
	}
}
