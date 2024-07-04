using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

[ExecuteInEditMode]
public class WarpGateData : MonoBehaviour
{
	BoxCollider coll;

    public GameObject ShapeNode; //Mesh, object?
    public int DelayTime;
    public int ReUsableTime;
    public string Effect_OnEnter;
    public string Effect_OnLeave;
    public string Effect_OnPlayerEnter;
    public string Effect_OnPlayerLeave;
	public List<WarpGateData> targetNodes = new List<WarpGateData>();

	public void Parse(StringBuilder sb, int index)
	{
		sb.AppendLine($"[WARPGATE_{index.ToString("00")}]");
		sb.AppendLine("WarpGate_Node=" + name);
		sb.AppendLine("WarpGate_ShapeNode=" + (ShapeNode? ShapeNode.name: ""));
		sb.AppendLine("WarpGate_DelayTime=" + DelayTime);
		sb.AppendLine("WarpGate_ReUsableTime=" + ReUsableTime);
		sb.AppendLine("WarpGate_Effect_OnEnter=" + Effect_OnEnter);
		sb.AppendLine("WarpGate_Effect_OnLeave=" + Effect_OnLeave);
		sb.AppendLine("WarpGate_Effect_OnPlayerEnter=" + Effect_OnPlayerEnter);
		sb.AppendLine("WarpGate_Effect_OnPlayerLeave=" + Effect_OnPlayerLeave);
		for(int i = 0; i < targetNodes.Count; i++)
		{
			sb.AppendLine($"WarpGate_TargetNode_{i+1}=" + targetNodes[i].name);
		}
	}

	private void OnEnable()
	{
		coll = GetComponent<BoxCollider>();
	}
	private void OnDrawGizmos()
	{
		Gizmos.matrix = transform.localToWorldMatrix;
		Gizmos.DrawWireCube(Vector3.zero, coll.size);
	}
	private void OnDrawGizmosSelected()
	{
		for (int i = 0; i < targetNodes.Count; i++)
		{
			if (targetNodes[i] == null) continue;

			float dist = Vector3.Distance(transform.position, targetNodes[i].transform.position);
			Gizmos.DrawLine(transform.position,targetNodes[i].transform.position);
			int count = (int)(dist / 1000f);
			for (int j = 1; j < count; j++)
			{
				DrawArrow(Vector3.Lerp(transform.position, targetNodes[i].transform.position, (float)j / (float)count), (targetNodes[i].transform.position- transform.position).normalized);
			}
		}
	}

	public static void DrawArrow(Vector3 position, Vector3 direction, float angle = -25f, float headSize = 200f)
	{
		Gizmos.DrawRay(position, Quaternion.LookRotation(direction) * Quaternion.Euler( angle, 0, 0) * Vector3.back * headSize);
		Gizmos.DrawRay(position, Quaternion.LookRotation(direction) * Quaternion.Euler(-angle, 0, 0) * Vector3.back * headSize);
		Gizmos.DrawRay(position, Quaternion.LookRotation(direction) * Quaternion.Euler( 0, angle, 0) * Vector3.back * headSize);
		Gizmos.DrawRay(position, Quaternion.LookRotation(direction) * Quaternion.Euler( 0,-angle, 0) * Vector3.back * headSize);
	}
}
