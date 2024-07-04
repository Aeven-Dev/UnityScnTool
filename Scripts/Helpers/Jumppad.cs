using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[ExecuteInEditMode]
[AddComponentMenu("S4 scn/JumpPad (Dont add from here)")]
public class Jumppad : MonoBehaviour
{
	[Range(0, 100)]	public int arcCount = 22;
	[Range(0f, 20f)] public float downwardsPower = 7f;
	[Range(0f, 1f)] public float powerMult = 0.1f;

	[HideInInspector]public Vector3[] arcPoints = new Vector3[0];
	public BoxCollider thisColl;
	[HideInInspector]public BoxCollider power;

	private void Start()
	{
		thisColl = GetComponent<BoxCollider>();
		power = (transform.childCount > 0) ? transform.GetChild(0).GetComponent<BoxCollider>() : null;
	}

	private void OnDrawGizmos()
	{
		Gizmos.matrix = transform.localToWorldMatrix;
		Gizmos.DrawWireCube(Vector3.zero, thisColl.size);
	}

	private void OnDrawGizmosSelected()
	{
		if (!power) return;
		
		for (int i = 1; i < arcPoints.Length; i++)
		{
			Vector3 prev = transform.position + arcPoints[i - 1];
			Vector3 next = transform.position + arcPoints[i];
			Gizmos.DrawLine(transform.position + arcPoints[i - 1], transform.position + arcPoints[i]);
			WarpGateData.DrawArrow(next, next - prev,headSize:power.size.z /50f);
		}
	}

	private void OnValidate()
	{
		CalculateArc();
	}

	public void CalculateArc()
	{
		if (!power) return;
		Vector3 pos = Vector3.zero;
		Vector3 dir = power.transform.forward * power.size.z * powerMult;
		arcPoints = new Vector3[arcCount];
		for (int i = 0; i < arcCount; i++)
		{
			arcPoints[i] = pos;
			pos += dir;
			dir += Vector3.down * 9.8f * downwardsPower;//downwards power
		}
	}
}
