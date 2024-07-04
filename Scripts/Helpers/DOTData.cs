using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

[ExecuteInEditMode]
public class DOTData : MonoBehaviour
{
	BoxCollider coll;
    public Team team;
    public int damage;
    public int timer;
    public string effect= "dot_eff";
    public string hud= "dot_eff_hud";

	public void Parse(StringBuilder sb, int index)
	{
		sb.AppendLine($"object_{index}_node=" + name);
		sb.AppendLine($"object_{index}_team=" + team.ToString());
		sb.AppendLine($"object_{index}_damage=" + damage);
		sb.AppendLine($"object_{index}_timer=" + timer);
		sb.AppendLine($"object_{index}_effect=" + effect);
		sb.AppendLine($"object_{index}_hud=" + hud);
	}

	private void Start()
	{
		coll = GetComponent<BoxCollider>();
	}
	private void OnDrawGizmos()
	{
		if (!coll)
		{
			return;
		}
		Gizmos.matrix = transform.localToWorldMatrix;
		Gizmos.DrawWireCube(Vector3.zero, coll.size);
	}
}
public enum Team { all, alpha, beta,}