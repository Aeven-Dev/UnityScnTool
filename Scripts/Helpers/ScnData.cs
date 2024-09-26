using AevenScnTool.IO;
using AevenScnTool.Menus;
using NetsphereScnTool.Scene;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

[ExecuteInEditMode]
public class ScnData : MonoBehaviour
{
    public string filePath = "";
	public string subName = "";
	public string animationCopy = "";
	public int version = 1045220557;

	public Vector3 octTreeBox = Vector3.one * 30;
	public Rect minimapCoverage = new Rect(Vector2.zero, Vector2.one * 30);

	public string lightmapName = "";
	public bool saveLightmapTexture = false;

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
			SaveScnFileAs();
			return;
		}
		SaveFile();

	}
	void SaveScnFileAs()
	{
		var dir = "";
		if (filePath != string.Empty && filePath != null)
		{
			dir = new FileInfo(filePath).DirectoryName;
		}
		var result = EditorUtility.SaveFilePanel("Save your scn file!", dir, name, "scn");
		if (result != string.Empty && result != null)
		{
			filePath = result;
			SaveFile();
		}
	}

	void SaveFile(){
		FileInfo fileInfo = new FileInfo(filePath);
		SceneContainer container = ScnFileExporter.CreateContainerFromScenes(fileInfo.Name, new ScnData[] { this });
		container.Write(filePath);

		if (saveLightmapTexture)
		{
			SelectExport.ExportLightmaps(fileInfo.Directory.FullName);
		}
	}

	private void OnDrawGizmos()
	{
		Gizmos.matrix = transform.localToWorldMatrix;
		Gizmos.color = new Color(0.5f, 0.5f, 0.5f, 0.5f);
		Gizmos.DrawWireCube(Vector3.zero, octTreeBox);
	}
	private void OnDrawGizmosSelected()
	{
		Gizmos.matrix = transform.localToWorldMatrix;
		Gizmos.color = Color.white;
		Gizmos.DrawWireCube(Vector3.zero, octTreeBox);
		Gizmos.color = Color.cyan;
		Gizmos.DrawWireCube(minimapCoverage.position,new Vector3(minimapCoverage.size.x, 0f, minimapCoverage.size.y));
	}
}
