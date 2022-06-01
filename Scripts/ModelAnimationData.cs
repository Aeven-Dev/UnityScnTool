using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Obsolete("This is no longer used! Please change it", true)]
[AddComponentMenu("S4 scn/Old model data that you shouldnt be using, immma delete it :3")]
public class ModelAnimationData : MonoBehaviour
{
	public List<ModelAnimation> animations = new List<ModelAnimation>();

	//[ContextMenu("Set first to default :D")]
	public void SetDefault()
	{
		ModelAnimation ma;
		if (animations.Count > 0)
		{
			 ma = animations[0];
		}
		else
		{
			ma = new ModelAnimation();
			animations.Add(ma);
		}
		
		ma.Name = "DEFAULT_(>w<)";

		ma.TransformKeyData2 = new TransformKeyData2();
		ma.TransformKeyData2.TransformKey = new TransformKey();
		ma.TransformKeyData2.TransformKey.Translation = transform.position;
		ma.TransformKeyData2.TransformKey.Rotation = transform.rotation;
		ma.TransformKeyData2.TransformKey.Scale = transform.lossyScale;
	}
	[ContextMenu("Delete all!")]
	public void DeleteAllModelAnimData()
	{
		ModelAnimationData[] all = FindObjectsOfType<ModelAnimationData>();
		for (int i = 0; i < all.Length; i++)
		{
			DestroyImmediate(all[i]);
		}
	}


	[ContextMenu("Change to new S4Anims!")]
	public void ChangeToNew()
	{
		gameObject.AddComponent<S4Animations>().FromModelAnimation(animations);

		DestroyImmediate(this);
	}
}
