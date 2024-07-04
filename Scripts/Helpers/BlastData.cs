using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace AevenScnTool
{
	public class BlastData : MonoBehaviour
	{
		public string sequence;
		public bool timed;
		public int blasttime_min;
		public int blasttime_max;
		public string sound;
		public int hp;
		public int rebirthtime;
		public int material;

		public void CreateNewBlastChild()
		{
			GameObject mesh = new GameObject("New Mesh! :D");
			mesh.AddComponent<MeshFilter>();
			mesh.AddComponent<MeshRenderer>();
			mesh.AddComponent<TextureReference>();

			GameObject coll = new GameObject("New Collider! :O");
			coll.AddComponent<MeshCollider>();
			coll.AddComponent<CollisionData>().ground = GroundFlag.blast;
			coll.AddComponent<TextureReference>();

			coll.transform.SetParent(mesh.transform);
			mesh.transform.SetParent(transform);
		}

		public void Parse(StringBuilder sb, int index)
		{

			sb.AppendLine($"[BLASTINFO_{index.ToString("00")}]");
			sb.AppendLine("name=" + name);
			sb.AppendLine("sequence=" + sequence);
			if (timed)
			{
				sb.AppendLine("blasttime_min=" + blasttime_min);
				sb.AppendLine("blasttime_max=" + blasttime_max);
			}
			sb.AppendLine("sound=" + sound);
			sb.AppendLine("hp=" + hp);
			sb.AppendLine("rebirthtime=" + rebirthtime);
			sb.AppendLine("material=" + material);
		}

		private void OnDrawGizmos()
		{
			Gizmos.DrawWireCube(transform.position, Vector3.one);
		}
	}
}