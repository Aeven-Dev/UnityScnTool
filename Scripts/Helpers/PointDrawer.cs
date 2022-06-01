using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class PointDrawer : MonoBehaviour
{
    public enum PointType { ball_spawn_pos, alpha_net,beta_net,alpha_spawn_pos,beta_spawn_pos}
    public PointType type;

	private void OnDrawGizmos()
	{
		switch (type)
		{
			case PointType.ball_spawn_pos:
				DrawFumbi();
				break;
			case PointType.alpha_net:
				DrawA();
				break;
			case PointType.beta_net:
				DrawB();
				break;
			case PointType.alpha_spawn_pos:
				DrawA();
				break;
			case PointType.beta_spawn_pos:
				DrawB();
				break;
			default:
				break;
		}
	}

	void DrawFumbi()
	{
		Handles.DrawWireDisc(transform.position, transform.forward, 100f);
		Handles.DrawWireDisc(transform.position, transform.forward, 50f);
		Handles.DrawWireDisc(transform.position + transform.up * 150f, transform.forward, 30f);

		Vector3 pos = transform.position + transform.up * 100f;
		Vector3 left = pos + transform.right * 20f;
		Vector3 right = pos - transform.right * 20f;
		Gizmos.DrawLine(left, left + Vector3.up * 30f);
		Gizmos.DrawLine(right, right + Vector3.up * 30f);
	}

	void DrawA()
	{
		float size = 100f;

		Vector3 point1 = transform.position + new Vector3(-1.5f, -2f, 0f) * size;
		Vector3 point2 = transform.position + new Vector3(-0.5f, 2f, 0f) * size;
		Vector3 point3 = transform.position + new Vector3(0.5f, 2f, 0f) * size;
		Vector3 point4 = transform.position + new Vector3(1.5f, -2f, 0f) * size;
		Vector3 point5 = transform.position + new Vector3(0.7f, -2f, 0f) * size;
		Vector3 point6 = transform.position + new Vector3(0.5f, -0.5f, 0f) * size;
		Vector3 point7 = transform.position + new Vector3(-0.5f, -0.5f, 0f) * size;
		Vector3 point8 = transform.position + new Vector3(-0.7f, -2f, 0f) * size;
		Vector3 point9 = transform.position + new Vector3(0f, 1.5f, 0f) * size;
		Vector3 point10 = transform.position + new Vector3(-0.5f, 0f, 0f) * size;
		Vector3 point11 = transform.position + new Vector3(0.5f, 0f, 0f) * size;
		Gizmos.DrawLine(point1, point2);
		Gizmos.DrawLine(point2, point3);
		Gizmos.DrawLine(point3, point4);
		Gizmos.DrawLine(point4, point5);
		Gizmos.DrawLine(point5, point6);
		Gizmos.DrawLine(point6, point7);
		Gizmos.DrawLine(point7, point8);
		Gizmos.DrawLine(point8, point1);
		Gizmos.DrawLine(point9, point10);
		Gizmos.DrawLine(point10, point11);
		Gizmos.DrawLine(point11, point9);
	}

	void DrawB()
	{
		float size = 100f;

		Vector3 point1 = transform.position + new Vector3(1.5f, -2f, 0f) * size;
		Vector3 point2 = transform.position + new Vector3(-1f, -2f, 0f) * size;
		Vector3 point3 = transform.position + new Vector3(-1.5f, -1.5f, 0f) * size;
		Vector3 point4 = transform.position + new Vector3(-1.5f, 1.5f, 0f) * size;
		Vector3 point5 = transform.position + new Vector3(-1f, 2f, 0f) * size;
		Vector3 point6 = transform.position + new Vector3(1.5f, 2f, 0f) * size;

		Vector3 point7 = transform.position + new Vector3(0.9f, -1.2f, 0f) * size;
		Vector3 point8 = transform.position + new Vector3(-0.8f, -1.2f, 0f) * size;
		Vector3 point9 = transform.position + new Vector3(-0.8f, -0.3f, 0f) * size;
		Vector3 point10 = transform.position + new Vector3(0.9f, -0.3f, 0f) * size;

		Vector3 point11 = transform.position + new Vector3(0.9f, 1.2f, 0f) * size;
		Vector3 point12 = transform.position + new Vector3(-0.8f, 1.2f, 0f) * size;
		Vector3 point13 = transform.position + new Vector3(-0.8f, 0.3f, 0f) * size;
		Vector3 point14 = transform.position + new Vector3(0.9f, 0.3f, 0f) * size;
		Gizmos.DrawLine(point1, point2);
		Gizmos.DrawLine(point2, point3);
		Gizmos.DrawLine(point3, point4);
		Gizmos.DrawLine(point4, point5);
		Gizmos.DrawLine(point5, point6);
		Gizmos.DrawLine(point6, point1);

		Gizmos.DrawLine(point7, point8);
		Gizmos.DrawLine(point8, point9);
		Gizmos.DrawLine(point9, point10);
		Gizmos.DrawLine(point10, point7);

		Gizmos.DrawLine(point11, point12);
		Gizmos.DrawLine(point12, point13);
		Gizmos.DrawLine(point13, point14);
		Gizmos.DrawLine(point14, point11);
	}
}
