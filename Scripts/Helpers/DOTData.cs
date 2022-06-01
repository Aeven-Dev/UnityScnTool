using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class DOTData : MonoBehaviour
{
	BoxCollider coll;
    public Team team;
    public int damage;
    public int timer;
    public string effect;
    public string hud;

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
		Gizmos.DrawWireCube(transform.position, coll.size);
	}
}
public enum Team { all, alpha, beta,}