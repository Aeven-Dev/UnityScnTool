using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
[AddComponentMenu("S4 scn/Bone")]
public class Bone : MonoBehaviour
{
	Bone parentBone;

	public S4Animations s4Animations;

	private void Start()
	{
		parentBone = transform.parent?.GetComponent<Bone>();
		for (int i = 0; i < transform.childCount; i++)
		{
			transform.GetChild(i).GetComponent<Bone>()?.ParentBoneAdded();
		}
	}
	private void OnTransformParentChanged()
	{
		parentBone = transform.parent?.GetComponent<Bone>();
	}
	private void OnValidate()
	{
		parentBone = transform.parent?.GetComponent<Bone>();
	}
	private void ParentBoneAdded()
	{
		parentBone = transform.parent?.GetComponent<Bone>();
	}
	private void OnDrawGizmos()
	{
		Gizmos.color = Color.white;
		Gizmos.DrawSphere(transform.position, 1f);
		if (parentBone)
		{
			Gizmos.DrawLine(transform.position, parentBone.transform.position);
		}
	}
}
