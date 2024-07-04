using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[AddComponentMenu("S4 scn/Animations!/VertexRadialNoise")]
public class VertexRadialNoise : VertexAnimBase
{
    public float minRange = 1;
    public float maxRange = 2;

    public override MorphKey.VertexMorph GetVert(int index, float factor, Vector3[] vertex)
    {
        Vector3 vert = vertex[index];

        vert *= Random.Range(minRange, maxRange);

        return new MorphKey.VertexMorph((uint)index, vert);

    }
}
