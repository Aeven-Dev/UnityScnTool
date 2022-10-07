using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("S4 scn/Animations!/UVScale")]
public class UVRotate : UVAnimBase
{
	public float angle = 360f;
	public Vector2 center = new Vector2(0.5f, 0.5f);

	public override MorphKey.UVMorph GetUV(int index, float factor, Vector2[] uvs)
	{
		var uv = uvs[index];

		uv = (Vector2)(Quaternion.Euler(0f, 0f, angle) * (uv - center)) + center;

		return new MorphKey.UVMorph((uint)index, uv);
	}
}
