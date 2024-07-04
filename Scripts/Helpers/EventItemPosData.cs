using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class EventItemPosData : MonoBehaviour
{
	public void Parse(StringBuilder sb, int index)
	{
		sb.AppendLine($"POSX{index.ToString("000")}=" + transform.position.x);
		sb.AppendLine($"POSY{index.ToString("000")}=" + transform.position.y);
		sb.AppendLine($"POSZ{index.ToString("000")}=" + transform.position.z);
	}
	private void OnDrawGizmos()
	{
		Gizmos.DrawWireCube(transform.position, Vector3.one * 100);
	}
}
