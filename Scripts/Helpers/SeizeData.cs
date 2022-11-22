using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class SeizeData : MonoBehaviour
{
    public string ally_seq;
    public string enemy_seq;
    public string neutral_seq;
    public int seize_slot;

	public void Parse(StringBuilder sb, int index)
	{
		sb.AppendLine($"[SEIZE_{seize_slot.ToString("00")}]");
		sb.AppendLine("ally_seq=" + ally_seq);
		sb.AppendLine("enemy_seq=" + enemy_seq);
		sb.AppendLine("neutral_seq=" + neutral_seq);
		sb.AppendLine("seize_slot=" + seize_slot);
	}
}
