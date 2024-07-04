using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class SectorData : MonoBehaviour
{
	public void ParseContent(StringBuilder sb, int index)
	{
		sb.Append($"sector{index}=" + name + Environment.NewLine);
	}

	public void ParseTitle(StringBuilder sb, int index)
	{
		sb.Append($"[sector{index}]" + Environment.NewLine);
	}
}
