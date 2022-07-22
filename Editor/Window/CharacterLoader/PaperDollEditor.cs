using AevenScnTool.IO;
using NetsphereScnTool.Scene;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

[CustomEditor(typeof(PaperDoll))]
public class PaperDollEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
		if (GUILayout.Button("Open Character Loader"))
		{
            CharacterLoader.OpenPaperdoll(target as PaperDoll);
		}
    }

}
