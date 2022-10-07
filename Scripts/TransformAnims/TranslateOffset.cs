using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[AddComponentMenu("S4 scn/Animations!/TranslateOffset")]
public class TranslateOffset : TranslateAnimBase
{
	public Vector3 offset;
	public override Vector3 GetTranslation(float factor)
	{
		return transform.position + offset * factor;
	}
}
