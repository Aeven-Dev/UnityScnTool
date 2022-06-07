using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using System.Collections.Generic;

public class MorphEditor : VisualElement
{
    public new class UxmlFactory : UxmlFactory<MorphEditor, UxmlTraits> { }

    public new class UxmlTraits : VisualElement.UxmlTraits
    {
        public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
        {
            base.Init(ve, bag, cc);

            MorphEditor me = ve as MorphEditor;
            me.generateVisualContent += me.OnGenerateVisualContent;
        }
    }

    Vector2[] uvs;
    List<(int, int)> lines;
    Rect uvRegion;

    void OnGenerateVisualContent(MeshGenerationContext mgc)
    {
		if (uvs == null)
		{
            return;
		}
        Rect r = contentRect;
        if (r.width < 0.01f || r.height < 0.01f)
            return; // Skip rendering when too small.

        MeshWriteData mwd = mgc.Allocate(lines.Count * 4, lines.Count * 6);
        uvRegion = mwd.uvRegion;

        (var vertices, var indices) = DrawUVs();
        // Since the texture may be stored in an atlas, the UV coordinates need to be
        // adjusted. Simply rescale them in the provided uvRegion.

        mwd.SetAllVertices(vertices.ToArray());
        mwd.SetAllIndices(indices.ToArray());
    }

    (Vertex[], ushort[]) DrawLine(Vector2 a, Vector2 b, int indexoffset)
    {
        Vertex[] verts = new Vertex[4];
        ushort[] indices = {
            (ushort)(indexoffset + 2),
            (ushort)(indexoffset + 1),
            (ushort)(indexoffset),
            (ushort)(indexoffset + 2),
            (ushort)(indexoffset + 3),
            (ushort)(indexoffset)};

        Vector2 dir = a - b;
        var per = Vector2.Perpendicular(dir).normalized;

        //k_Vertices[0].position = new Vector3(left, bottom, Vertex.nearZ);
        verts[0].position = a + per;
        verts[1].position = a - per;
        verts[2].position = b + per;
        verts[3].position = b - per;

        verts[0].tint = Color.white;
        verts[1].tint = Color.white;
        verts[2].tint = Color.white;
        verts[3].tint = Color.white;

        verts[0].uv = new Vector2(0, 0) * uvRegion.size + uvRegion.min;
        verts[1].uv = new Vector2(0, 1) * uvRegion.size + uvRegion.min;
        verts[2].uv = new Vector2(1, 1) * uvRegion.size + uvRegion.min;
        verts[3].uv = new Vector2(1, 0) * uvRegion.size + uvRegion.min;

        return (verts, indices);
    }

    public void SetMesh( Mesh mesh)
	{
        this.uvs = mesh.uv;
        this.lines = CalculateLines(mesh);
	}

    public void SetUVs(Vector2[] uvs)
	{
        this.uvs = uvs;
    }

    public static List<(int, int)> CalculateLines(Mesh mesh)
	{
        List<(int, int)> lines = new List<(int, int)>();
		for (int i = 0; i < mesh.triangles.Length; i+=3)
        {
            int a = mesh.triangles[i];
            int b = mesh.triangles[i + 1];
            int c = mesh.triangles[i + 2];
            if (ContainsLine(lines, a, b) == false)
                lines.Add((a, b));
            if (ContainsLine(lines, b, c) == false)
                lines.Add((b, c));
            if (ContainsLine(lines, c, a) == false)
                lines.Add((c, a));
        }
        return lines;
	}

    static bool ContainsLine(List<(int, int)> lines, int a, int b)
	{
		foreach (var pair in lines)
		{
            if (pair.Item1 == a && pair.Item2 == b)
            {
                return true;
            }
            if (pair.Item1 == b && pair.Item2 == a)
            {
                return true;
            }
        }
        return false;
	}

    (List<Vertex>, List<ushort>) DrawUVs()
	{
        List<Vertex> vertices = new List<Vertex>(lines.Count * 4);
        List<ushort> indices = new List<ushort>(lines.Count * 6);
        for (int i = 0; i < lines.Count; i++)
        {
            var pair = DrawLine(uvs[lines[i].Item1], uvs[lines[i].Item2], i * 4);
            vertices.AddRange(pair.Item1);
            indices.AddRange(pair.Item2);
        }
        return (vertices, indices);
    }
}