using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace AevenScnTool
{
	[CustomEditor(typeof(Jumppad)), CanEditMultipleObjects]
	public class JumppadEditor : Editor
	{
		Jumppad jumppad;
		float size;
		private void OnEnable()
		{
			jumppad = (Jumppad)target;
			size = 300f;
		}
		public void OnSceneGUI()
		{
			if (!jumppad) return;
			if (!jumppad.power) return;
			if (jumppad.transform.hasChanged || jumppad.power.transform.hasChanged)
			{
				CalculateArc();
			}
			Handles.color = Color.yellow;

			Quaternion rot = Handles.Disc(jumppad.power.transform.rotation, jumppad.transform.position,
				jumppad.transform.up, size / ScnToolData.Instance.scale, true, 1f);
			if (jumppad.power.transform.rotation != rot)
			{
				Undo.RecordObject(jumppad.power.transform, "Jumpad Yaw");
				jumppad.power.transform.rotation = rot;
				jumppad.power.transform.position = jumppad.transform.position + jumppad.power.transform.forward * jumppad.power.size.z / 2f;
			}

			Handles.color = Color.cyan;
			Quaternion rot2 = Handles.Disc(jumppad.power.transform.rotation, jumppad.transform.position, jumppad.power.transform.right, size / ScnToolData.Instance.scale, true, 1f);
			if (jumppad.power.transform.rotation != rot2)
			{
				Undo.RecordObject(jumppad.power.transform, "Jumpad Pitch");
				jumppad.power.transform.rotation = rot2;
				jumppad.power.transform.position = jumppad.transform.position + jumppad.power.transform.forward * jumppad.power.size.z / 2f;
			}

			Handles.color = Color.white;
			float power = Handles.ScaleSlider(jumppad.power.size.z, jumppad.transform.position, jumppad.power.transform.forward, jumppad.power.transform.rotation, size * 2f / ScnToolData.Instance.scale, 1f);
			if (jumppad.power.size.z != power)
			{
				Undo.RecordObject(jumppad.power, "Jumpad Power");
				jumppad.power.size = new Vector3(jumppad.power.size.x, jumppad.power.size.y, power);
				jumppad.power.transform.position = jumppad.transform.position + jumppad.power.transform.forward * jumppad.power.size.z / 2f;
			}
		}

		public void CalculateArc()
		{
			if (!jumppad.power) return;
			Vector3 pos = Vector3.zero;
			Vector3 dir = jumppad.power.transform.forward * jumppad.power.size.z * jumppad.powerMult;
			jumppad.arcPoints = new Vector3[jumppad.arcCount];
			for (int i = 0; i < jumppad.arcCount; i++)
			{
				jumppad.arcPoints[i] = pos;
				pos += dir;
				dir += Vector3.down * 9.8f * jumppad.downwardsPower / ScnToolData.Instance.scale;//downwards power
			}
		}
	}
}