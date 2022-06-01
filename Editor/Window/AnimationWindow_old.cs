using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class AnimationWindow_old : EditorWindow
{
	bool playing = false;
	int frame = 0;
	Vector2 elementScroll = Vector2.zero;
	Vector2 keyframeScroll = Vector2.zero;
	float zoom = 1;

	private void OnGUI()
	{
		EditorGUILayout.BeginHorizontal(GUILayout.Width(100));
		EditorGUILayout.BeginVertical();
		ControlGUI();
		EditorGUILayout.EndVertical();
		EditorGUILayout.BeginVertical();
		KeyframesGUI();
		EditorGUILayout.EndVertical();
		EditorGUILayout.EndHorizontal();
	}

	private void ControlGUI()
	{
		//animation selector and playback
		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.Popup(0, new string[] { "test1", "test2" }, GUILayout.Width(100));
		if (GUILayout.Button("Prev",GUILayout.Width(40)))
		{
			frame--;
			//wrap
			//update frame
		}
		if (GUILayout.Button(playing ? "Stop" : "Play", GUILayout.Width(40)))
		{
			playing = !playing;

		}
		if (GUILayout.Button("Next", GUILayout.Width(40)))
		{
			frame++;
			//wrap
			//update frame
		}
		frame = EditorGUILayout.DelayedIntField(frame, GUILayout.Width(40));
		
		EditorGUILayout.EndHorizontal();
		//element selector
		elementScroll = EditorGUILayout.BeginScrollView(elementScroll);
		EditorGUILayout.LabelField("test");
		EditorGUILayout.LabelField("test");
		EditorGUILayout.LabelField("test");
		EditorGUILayout.LabelField("test");
		EditorGUILayout.LabelField("test");
		EditorGUILayout.LabelField("test");
		EditorGUILayout.LabelField("test");
		EditorGUILayout.LabelField("test");
		EditorGUILayout.LabelField("test");
		EditorGUILayout.LabelField("test");
		EditorGUILayout.LabelField("test");
		EditorGUILayout.LabelField("test");
		EditorGUILayout.LabelField("test");
		EditorGUILayout.LabelField("test");
		EditorGUILayout.LabelField("test");
		EditorGUILayout.LabelField("test");
		EditorGUILayout.LabelField("test");
		EditorGUILayout.LabelField("test");
		EditorGUILayout.LabelField("test");
		EditorGUILayout.LabelField("test");
		EditorGUILayout.LabelField("test");
		EditorGUILayout.LabelField("test");
		EditorGUILayout.LabelField("test");
		EditorGUILayout.LabelField("test");
		EditorGUILayout.LabelField("test");
		EditorGUILayout.LabelField("test");
		EditorGUILayout.LabelField("test");
		EditorGUILayout.LabelField("test");
		EditorGUILayout.LabelField("test");
		EditorGUILayout.LabelField("test");
		EditorGUILayout.LabelField("test");
		EditorGUILayout.LabelField("test");
		EditorGUILayout.LabelField("test");
		EditorGUILayout.LabelField("test");
		EditorGUILayout.LabelField("test");
		EditorGUILayout.LabelField("test");
		EditorGUILayout.LabelField("test");
		EditorGUILayout.LabelField("test");
		EditorGUILayout.LabelField("test");
		EditorGUILayout.LabelField("test");
		EditorGUILayout.LabelField("test");
		EditorGUILayout.LabelField("test");
		EditorGUILayout.LabelField("test");

		EditorGUI.indentLevel = 0;
		EditorGUILayout.EndScrollView();
	}

	void DrawElement()
	{
		EditorGUI.indentLevel = 0;
	}
	void DrawPosition()
	{

		EditorGUI.indentLevel = 1;
	}
	void DrawRotation()
	{
		EditorGUI.indentLevel = 1;
	}
	void DrawScale()
	{
		EditorGUI.indentLevel = 1;
	}
	void DrawVis()
	{
		EditorGUI.indentLevel = 1;
	}


	private void KeyframesGUI()
	{

		frame = (int)GUILayout.HorizontalSlider(frame, 0, 10, GUI.skin.box, GUI.skin.verticalSliderThumb, GUILayout.Width(200 * 20 * zoom));
		keyframeScroll = EditorGUILayout.BeginScrollView(keyframeScroll,true,true, GUILayout.Width(position.width - 300));


		List<Vector2> points = new List<Vector2> { Vector2.zero, Vector2.one * 100 };

		EditorGUILayout.Space(1000);
		GUILayout.BeginArea(new Rect(5,30,1000,1000), GUI.skin.box);
		Handles.BeginGUI();
		Handles.color = Color.red;
		foreach (var item in points)
		{
			Handles.DrawSolidDisc(item, Vector3.forward, 3f);
		}
		Handles.EndGUI();

		GUILayout.EndArea();
		EditorGUILayout.EndScrollView();
	}

	public static void Init()
	{
		GetWindow(typeof(AnimationWindow));
	}
}
