using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class SpectatorCameraData : MonoBehaviour
{
    public int delayTime;

	public void Parse(StringBuilder sb, int index )
	{
		sb.AppendLine($"[BROADCASTINGCAMERA_{index.ToString("00")}]");
		sb.AppendLine("PosX=" + transform.position.x);
		sb.AppendLine("PosY=" + transform.position.y);
		sb.AppendLine("PosZ=" + transform.position.z);
		sb.AppendLine("DirX=" + transform.forward.x);
		sb.AppendLine("DirY=" + transform.forward.y);
		sb.AppendLine("DirZ=" + transform.forward.z);
		sb.AppendLine("DelayTime=" + delayTime);
	}


	private void OnDrawGizmos()
	{
		Gizmos.DrawWireCube(transform.position, Vector3.one * 100);
	}
}
