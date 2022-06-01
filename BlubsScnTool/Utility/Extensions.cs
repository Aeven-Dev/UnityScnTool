using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace NetsphereScnTool
{
    public static class ResourceExtensions
    {
        internal static Matrix4x4 ReadMatrix(this BinaryReader r)
        {
            return new Matrix4x4(
                new Vector4(r.ReadSingle(), r.ReadSingle(), r.ReadSingle(), r.ReadSingle()),
                new Vector4(r.ReadSingle(), r.ReadSingle(), r.ReadSingle(), r.ReadSingle()),
                new Vector4(r.ReadSingle(), r.ReadSingle(), r.ReadSingle(), r.ReadSingle()),
                new Vector4(r.ReadSingle(), r.ReadSingle(), r.ReadSingle(), r.ReadSingle())
                );
        }

        internal static void Write(this BinaryWriter w, Matrix4x4 value)
        {
            Matrix4x4 transposed = value.transpose;

            w.Write(transposed.m00);
            w.Write(transposed.m01);
            w.Write(transposed.m02);
            w.Write(transposed.m03);

            w.Write(transposed.m10);
            w.Write(transposed.m11);
            w.Write(transposed.m12);
            w.Write(transposed.m13);
                    
            w.Write(transposed.m20);
            w.Write(transposed.m21);
            w.Write(transposed.m22);
            w.Write(transposed.m23);
                    
            w.Write(transposed.m30);
            w.Write(transposed.m31);
            w.Write(transposed.m32);
            w.Write(transposed.m33);
        }
    }
}
