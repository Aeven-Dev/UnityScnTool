using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("S4 scn/Animations!/VertexLinearNoise")]
public class VertexLinearNoise : VertexAnimBase
{
    public Vector3 direction = Vector3.up;
    public float minRange = 0;
    public float maxRange = 1;

    public override MorphKey.VertexMorph GetVert(int index, float factor, Vector3[] vertex)
    {
        Vector3 vert = vertex[index];

        vert += direction * Random.Range(minRange, maxRange);

        return new MorphKey.VertexMorph((uint)index, vert);

    }
}
