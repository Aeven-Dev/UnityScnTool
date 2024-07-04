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
		float DISC_RADIUS = 300f;

		bool editingTarget = false;
		Vector3 targetPoint = Vector3.zero;
		float hintHeight = 0f;

		private void OnEnable()
		{
			jumppad = (Jumppad)target;
		}
		public void OnSceneGUI()
		{
			if (!jumppad) return;
			if (!jumppad.power) return;
			if (jumppad.transform.hasChanged || jumppad.power.transform.hasChanged)
			{
				CalculateArc();
			}
			if (editingTarget)
			{
				DrawTarget();
			}
			else{
				DrawYaw();
				DrawPitch();
				DrawPower();
			}
			void DrawTarget(){
				Vector3 newPoint = Handles.DoPositionHandle(targetPoint, jumppad.transform.rotation);
				Handles.DrawWireDisc(targetPoint, Vector3.up, DISC_RADIUS);
				Vector3 middle = Vector3.Lerp(jumppad.transform.position, targetPoint,0.5f);
				middle.y = hintHeight;
				Vector3 newHint = Handles.Slider(middle,Vector3.up,300f,Handles.ArrowHandleCap, 0f);
				if(newPoint != targetPoint || newHint.y != hintHeight){
					CalculateParametersByTarget();
				}
			}
			void DrawYaw(){
				Handles.color = Color.yellow;

				Quaternion rot = Handles.Disc(jumppad.power.transform.rotation, jumppad.transform.position,
					jumppad.transform.up, DISC_RADIUS / ScnToolData.Instance.scale, true, 1f);
				if (jumppad.power.transform.rotation != rot)
				{
					Undo.RecordObject(jumppad.power.transform, "Jumpad Yaw");
					jumppad.power.transform.rotation = rot;
					jumppad.power.transform.position = jumppad.transform.position + jumppad.power.transform.forward * jumppad.power.size.z / 2f;
				}
			}
			void DrawPitch(){
				Handles.color = Color.cyan;
				Quaternion rot2 = Handles.Disc(jumppad.power.transform.rotation, jumppad.transform.position, jumppad.power.transform.right, DISC_RADIUS / ScnToolData.Instance.scale, true, 1f);
				if (jumppad.power.transform.rotation != rot2)
				{
					Undo.RecordObject(jumppad.power.transform, "Jumpad Pitch");
					jumppad.power.transform.rotation = rot2;
					jumppad.power.transform.position = jumppad.transform.position + jumppad.power.transform.forward * jumppad.power.size.z / 2f;
				}
			}
			void DrawPower(){
				Handles.color = Color.white;
				float power = Handles.ScaleSlider(jumppad.power.size.z, jumppad.transform.position, jumppad.power.transform.forward, jumppad.power.transform.rotation, DISC_RADIUS * 2f / ScnToolData.Instance.scale, 1f);
				if (jumppad.power.size.z != power)
				{
					Undo.RecordObject(jumppad.power, "Jumpad Power");
					jumppad.power.size = new Vector3(jumppad.power.size.x, jumppad.power.size.y, power);
					jumppad.power.transform.position = jumppad.transform.position + jumppad.power.transform.forward * jumppad.power.size.z / 2f;
				}
			}
		}

		public override void OnInspectorGUI(){

			if (!editingTarget &&GUILayout.Toggle(editingTarget,"Edit by Target!"))
			{
				editingTarget = true;
				CalculateTarget();
			}

			base.OnInspectorGUI();
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
		void CalculateTarget(){
			if (!jumppad.power) return;
			if(jumppad.arcPoints == null && jumppad.arcCount > 0) CalculateArc();
			if(jumppad.arcPoints.Length == 0 && jumppad.arcCount > 0) CalculateArc(); else return;

			targetPoint = jumppad.arcPoints[jumppad.arcPoints.Length - 1];
		}

		void CalculateParametersByTarget(){
			Quaternion rotTarget = Quaternion.LookRotation(targetPoint - jumppad.transform.position, Vector3.up);
			Vector3 middle = Vector3.Lerp(jumppad.transform.position, targetPoint,0.5f);
			middle.y = hintHeight;
			Quaternion rotHint = Quaternion.LookRotation(middle - jumppad.transform.position, Vector3.up);
			var rot = Quaternion.Slerp(rotTarget,rotHint,0.5f);
			jumppad.power.transform.rotation = rot;
			jumppad.power.transform.position = jumppad.transform.position + jumppad.power.transform.forward * jumppad.power.size.z / 2f;

			float power = (targetPoint - jumppad.transform.position).magnitude;
			jumppad.power.size = new Vector3(jumppad.power.size.x, jumppad.power.size.y, power);
		}
	}
}