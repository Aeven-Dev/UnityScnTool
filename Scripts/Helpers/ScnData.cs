using AevenScnTool.IO;
using NetsphereScnTool.Scene;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[ExecuteInEditMode]
public class ScnData : MonoBehaviour
{
    public string filePath;

	[Button("Save Scn File! :D")] public ButtonAction saveScnfile;
	[Button("Save Scn File as...! :3")] public ButtonAction saveScnfileAs;
	private void OnEnable()
	{
		saveScnfile = new ButtonAction(SaveScnFile);
		saveScnfileAs = new ButtonAction(SaveScnFileAs);
	}
	void SaveScnFile()
	{
		if (filePath == string.Empty)
		{
			filePath = EditorUtility.OpenFilePanel("Save your scn file!", "", "scn");
		}
		SceneContainer container = ScnFileExporter.CreateContainerFromScenes(new System.IO.FileInfo(filePath).Name, new ScnData[] { this });
		container.Write(filePath);
	}
	void SaveScnFileAs()
	{
		filePath = EditorUtility.OpenFilePanel("Save your scn file!", "", "scn");
		
		SceneContainer container = ScnFileExporter.CreateContainerFromScenes(new System.IO.FileInfo(filePath).Name, new ScnData[] { this });
		container.Write(filePath);
	}
}
