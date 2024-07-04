using AevenScnTool;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("S4 scn/Animations!/VertexWave")]
[System.Serializable]
public class VertexWave : VertexAnimBase
{
    public Vector3 waveForward = Vector3.forward;
    public Vector3 waveUp = Vector3.up;
    public float magnitud = 1f;
    public float speed = 1f;

    public override MorphKey.VertexMorph GetVert(int index, float factor, Vector3[] vertex)
	{
        Vector3 vert = vertex[index];
        float x = Vector3.Project(vert,waveForward).magnitude;
        vert += waveUp * Mathf.Sin( x + (factor * speed)) * magnitud;
        return new MorphKey.VertexMorph((uint)index, vert);

    }
}
