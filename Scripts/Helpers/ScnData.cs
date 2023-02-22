using AevenScnTool.IO;
using NetsphereScnTool.Scene;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

[ExecuteInEditMode]
public class ScnData : MonoBehaviour
{
    public string filePath;
	public int version = 1045220557;

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
			var file = new FileInfo(filePath);
			filePath = EditorUtility.SaveFilePanel("Save your scn file!", file.DirectoryName,name, "scn");
			if (filePath == string.Empty)
			{
				return;
			}
		}
		SceneContainer container = ScnFileExporter.CreateContainerFromScenes(new System.IO.FileInfo(filePath).Name, new ScnData[] { this });
		container.Write(filePath);
	}
	void SaveScnFileAs()
	{
		var dir = "";
		if (filePath != string.Empty)
		{
			dir = new FileInfo(filePath).DirectoryName;
		}
		var result = EditorUtility.SaveFilePanel("Save your scn file!", dir, name, "scn");
		if (result != string.Empty)
		{
			filePath = result;
			SceneContainer container = ScnFileExporter.CreateContainerFromScenes(new System.IO.FileInfo(filePath).Name, new ScnData[] { this });
			container.Write(filePath);
		}
	}
}
