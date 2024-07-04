using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace AevenScnTool
{
	public class PointDrawer : MonoBehaviour
	{
		public enum PointType { ball_spawn_pos, alpha_net, beta_net, alpha_spawn_pos, beta_spawn_pos, alpha_limited_area, beta_limited_area }
		public PointType type;

		private void OnDrawGizmos()
		{
			switch (type)
			{
				case PointType.ball_spawn_pos:
					DrawFumbi();
					break;
				case PointType.alpha_net:
					DrawANet();
					break;
				case PointType.beta_net:
					DrawBNet();
					break;
				case PointType.alpha_spawn_pos:
					DrawA();
					break;
				case PointType.beta_spawn_pos:
					DrawB();
					break;
				case PointType.alpha_limited_area:
					DrawA();
					DrawX();
					break;
				case PointType.beta_limited_area:
					DrawB();
					DrawX();
					break;
				default:
					break;
			}
		}

		void DrawFumbi()
		{
			Gizmos.DrawIcon(transform.position, ScnToolData.RootPath + @"Editor/Data/icon_pumbi_01.dds", true);
		}

		void DrawA()
		{
			Gizmos.DrawIcon(transform.position, ScnToolData.RootPath + @"/Editor/Data/A_icon.png", true);
		}

		void DrawB()
		{
			Gizmos.DrawIcon(transform.position, ScnToolData.RootPath + @"Editor/Data/B_icon.png", true);
		}

		void DrawANet()
		{
			Gizmos.DrawIcon(transform.position, ScnToolData.RootPath + @"Editor/Data/net_a_icon.png", true);
		}

		void DrawBNet()
		{
			Gizmos.DrawIcon(transform.position, ScnToolData.RootPath + @"Editor/Data/net_b_icon.png", true);
		}

		void DrawX()
		{
			Gizmos.DrawIcon(transform.position, ScnToolData.RootPath + @"Editor/Data/X_icon.png", true);
		}
	}
}