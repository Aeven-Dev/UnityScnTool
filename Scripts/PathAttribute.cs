using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[System.AttributeUsage(System.AttributeTargets.Field | System.AttributeTargets.Property, AllowMultiple = false)]
public class PathAttribute : PropertyAttribute
{
    public string extension = "";
    public PathAttribute() { }
    public PathAttribute(string extension) { this.extension = extension; }
}
