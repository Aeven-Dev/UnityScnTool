using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using NetsphereScnTool.Scene.Chunks;
using System.IO;
using System;
using Unity.Collections;
using UnityEngine.SceneManagement;
using NetsphereScnTool.Scene;
using UnityEditor.SceneManagement;
using AevenScnTool.IO;

namespace AevenScnTool.Menus
{
	public class ScnToolMenu : EditorWindow
	{
		public static bool identityMatrix = false;
		bool bottomFoldeout = false;

		private void OnSelectionChange()
		{
			Repaint();
		}

		void OnGUI()
		{
			Undo.RecordObject(ScnToolData.Instance, "Changed Scn Tool Menu Data");

			EditorGUILayout.LabelField("S4 Scn Window <3<3", EditorStyles.boldLabel);
			if (position.width > 300)
				WideGUI();
			else
				SlimGUI();

			ScnToolData.Instance.uv_flipVertical = EditorGUILayout.Toggle("Flip UVs Vertically .-.", ScnToolData.Instance.uv_flipVertical);
			ScnToolData.Instance.uv_flipHorizontal = EditorGUILayout.Toggle("Flip UVs Horizontally q:", ScnToolData.Instance.uv_flipHorizontal);


			ScnToolData.Instance.uv_flipVertical_lm = EditorGUILayout.Toggle("Flip UVs Vertically for lightmaps", ScnToolData.Instance.uv_flipVertical_lm);
			ScnToolData.Instance.uv_flipHorizontal_lm = EditorGUILayout.Toggle("Flip UVs Horizontally for lightmaps", ScnToolData.Instance.uv_flipHorizontal_lm);
			GUILayout.Space(10f);
			ScnToolData.Instance.ignoreLightmapsGlobally = EditorGUILayout.Toggle("Ignore all lightmaps!", ScnToolData.Instance.ignoreLightmapsGlobally);

			GUILayout.Space(40);
			if (GUILayout.Button(new GUIContent("Ultimate Modding Power!!",
				"A powerful mode where every little change is saved to the file, dangerous but useful!"),
				GUILayout.Height(35)))
			{
				Set_UMP();
			}
			GUILayout.Space(5);
			if (GUILayout.Button(new GUIContent("Generate MapInfo text! [].[]",
				"Opens a window to generate a text that you can paste inside the mapinfo ini to configure your DOTs, Warps, Blasts and such!")))
			{
				MapInfoWindow.Init();
			}

			

			bottomFoldeout = EditorGUILayout.BeginFoldoutHeaderGroup(bottomFoldeout, "Settings!");
			if (bottomFoldeout)
			{
				EditorGUI.indentLevel += 2;
				if (position.width > 300)
					WideBottom();
				else
					SlimBottom();

				ScnToolData.Instance.main_animation_name = EditorGUILayout.TextField("Main anim name!", ScnToolData.Instance.main_animation_name);
				EditorGUI.indentLevel -= 2;
			}

			EditorGUILayout.EndFoldoutHeaderGroup();

			if (ScnToolData.Instance.s4_startup_file != string.Empty)
			{
				if (GUILayout.Button(new GUIContent("Open S4... #_#", "Shortcut to opening the client!")))
				{
					System.Diagnostics.ProcessStartInfo psi = new System.Diagnostics.ProcessStartInfo();
					psi.FileName = ScnToolData.Instance.s4_startup_file;
					psi.WorkingDirectory = Path.GetDirectoryName(ScnToolData.Instance.s4_startup_file);
					System.Diagnostics.Process.Start(psi);
				}
			}

			EditorUtility.SetDirty(ScnToolData.Instance);
		}

		void WideGUI()
		{
			GUILayout.Space(20);
			EditorGUILayout.BeginHorizontal();
			float width = position.width / 2f;
			if (GUILayout.Button(new GUIContent("Open... :¬)", "Opens a scn file into a new SceneData root object of the same name!"), GUILayout.Width(width)))
			{
				Open();
			}
			if (GUILayout.Button(new GUIContent("Append... :D", "Opens a scn file into the SceneData root object of the selected object!"), GUILayout.Width(width)))
			{
				Append();
			}
			EditorGUILayout.EndHorizontal();

			var old = EditorGUIUtility.labelWidth;
			EditorGUIUtility.labelWidth = 200;
			if (EditorGUILayout.Toggle("Click me if you open official files!", !identityMatrix))
			{
				identityMatrix = false;
			}
			if (EditorGUILayout.Toggle("Click me if you open custom files!", identityMatrix))
			{
				identityMatrix = true;
			}
			EditorGUIUtility.labelWidth = old;

			EditorGUILayout.BeginHorizontal();
			if (GUILayout.Button(new GUIContent("Save... :3", "Saves the SceneData root objects to a the specified file!"), GUILayout.Width(position.width)))
			{
				SaveCurrent();
			}
			EditorGUILayout.EndHorizontal();


			EditorGUILayout.BeginHorizontal();
			GUI.enabled = ScnIsSelected();
			if (GUILayout.Button(new GUIContent("Set Scene! \\(> W <)", "Sets up all of the monobehaviours needed for the proper functionality of the tool in the selected ScnData object!"), GUILayout.Width(width)))
			{
				SetScene(Selection.activeGameObject.GetComponent<ScnData>());
			}
			GUI.enabled = true;
			if (GUILayout.Button(new GUIContent("Set Scenes! \\(> W <)<)<)", "Sets up all of the monobehaviours needed for the proper functionality of the tool in all the loaded ScnData objects!"), GUILayout.Width(width)))
			{
				SetAllOpenScenes();
			}
			EditorGUILayout.EndHorizontal();
		}

		void SlimGUI()
		{
			GUILayout.Space(20);
			if (GUILayout.Button(new GUIContent("Open... :¬)",
				"Opens a scn file into a new SceneData root object of the same name!")))
			{
				Open();
			}
			if (GUILayout.Button(new GUIContent("Append... :D",
				"Opens a scn file into into a window where you can select what objects to import!")))
			{
				Append();
			}



			if (GUILayout.Button(new GUIContent("Save... :3",
				"Saves the SceneData root objects to a the specified file!")))
			{
				SaveCurrent();
			}
			GUI.enabled = ScnIsSelected();
			if (GUILayout.Button(new GUIContent("Set Scene! \\(> W <)",
				"Sets up all of the monobehaviours needed for the proper functionality of the tool in the current scene!")))
			{
				SetScene(Selection.activeGameObject.GetComponent<ScnData>());
			}
			GUI.enabled = true;
			if (GUILayout.Button(new GUIContent("Set Scenes! \\(> W <)<)<)",
				"Sets up all of the monobehaviours needed for the proper functionality of the tool in all the loaded scenes!")))
			{
				SetAllOpenScenes();
			}
		}

		void WideBottom()
		{
			ScnToolData.Instance.scale = EditorGUILayout.FloatField("Scale!", ScnToolData.Instance.scale);
			if (ScnToolData.Instance.scale <= 0f)
			{
				ScnToolData.Instance.scale = 0.0001f;
			}

			EditorGUILayout.BeginHorizontal();
			//EditorGUILayout.LabelField("Path to S4 Folder", GUILayout.Width(170));
			ScnToolData.Instance.s4_folder_path = EditorGUILayout.DelayedTextField("S4 Folder", ScnToolData.Instance.s4_folder_path);
			if (GUILayout.Button("open...", GUILayout.Width(75)))
			{
				ScnToolData.Instance.s4_folder_path = EditorUtility.OpenFolderPanel("Select the your S4 folder!", ScnToolData.Instance.s4_folder_path, "");
			}

			EditorGUILayout.EndHorizontal();

			EditorGUILayout.BeginHorizontal();
			//ditorGUILayout.LabelField("Path to S4 Executable", GUILayout.Width(170));
			ScnToolData.Instance.s4_startup_file = EditorGUILayout.DelayedTextField("S4 Executable", ScnToolData.Instance.s4_startup_file);
			if (GUILayout.Button("open...", GUILayout.Width(75)))
			{
				ScnToolData.Instance.s4_startup_file = EditorUtility.OpenFilePanel("Select the your S4 Launcher!", ScnToolData.Instance.s4_startup_file, "exe,bat");
			}
			EditorGUILayout.EndHorizontal();
		}

		void SlimBottom()
		{
			ScnToolData.Instance.scale = EditorGUILayout.FloatField("Scale!", ScnToolData.Instance.scale);
			if (ScnToolData.Instance.scale <= 0f)
			{
				ScnToolData.Instance.scale = 0.0001f;
			}

			EditorGUILayout.LabelField("Path to S4 Folder");

			ScnToolData.Instance.s4_folder_path = EditorGUILayout.DelayedTextField(ScnToolData.Instance.s4_folder_path);
			EditorGUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			if (GUILayout.Button("open", GUILayout.Width(50)))
			{
				ScnToolData.Instance.s4_folder_path = EditorUtility.OpenFolderPanel("Select the your S4 folder!", "", "");
			}
			EditorGUILayout.EndHorizontal();
			EditorGUILayout.LabelField("Path to S4 Executable");

			ScnToolData.Instance.s4_startup_file = EditorGUILayout.DelayedTextField(ScnToolData.Instance.s4_startup_file);
			EditorGUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			if (GUILayout.Button("open", GUILayout.Width(50)))
			{
				ScnToolData.Instance.s4_startup_file = EditorUtility.OpenFolderPanel("Select the your S4 Launcher!", "", "");
			}
			EditorGUILayout.EndHorizontal();
		}

		private static void Append()
		{
			string fileName = EditorUtility.OpenFilePanel("Select your scn file!", ScnToolData.Instance.s4_folder_path, "scn");
			if (fileName == string.Empty) return;

			FileInfo fi = new FileInfo(fileName);
			if (fi.Exists == false) return;
			//Open scn file
			SceneContainer container = SceneContainer.ReadFrom(fileName);
			container.fileInfo = fi;

			SelectImport.Open(container);
		}
		private static void Open()
		{
			string fileName = EditorUtility.OpenFilePanel("Select your scn file!", ScnToolData.Instance.s4_folder_path, "scn");
			if (fileName == string.Empty) return;

			FileInfo fi = new FileInfo(fileName);
			if (fi.Exists == false) return;
			//Open scn file

			ScnFileImporter.LoadModel(fileName, identityMatrix);
		}

		private static void SaveCurrent()
		{
			SelectExport.Open();
		}

		static void SetScene(ScnData scn)
		{
			GameObject[] allGameObjects = ObjectCreation.GetAllGameObjectsFromScn(scn);

			foreach (GameObject go in allGameObjects)
			{
				ObjectCreation.SetGameObject(go);
			}

		}
		static void SetAllOpenScenes()
		{
			ScnData[] scenes = FindObjectsOfType<ScnData>();
			if (scenes.Length == 0)
			{
				Debug.LogWarning("You silly goose! there's no ScnData object selected! If you dont have one then make one!");
			}

			List<GameObject> allGameObjects = new List<GameObject>();
			foreach (ScnData scene in scenes)
			{
				allGameObjects.AddRange(ObjectCreation.GetAllGameObjectsFromScn(scene));
			}

			foreach (GameObject go in allGameObjects)
			{
				ObjectCreation.SetGameObject(go);
			}

		}

		static void Set_UMP()
		{
			if (EditorUtility.DisplayDialog("Activating UMP...", "Are you sure you want to activate the 'ULTIMATE MODDING POWER!!!!'?", "Yes!", "no..."))
			{
				EditorUtility.DisplayDialog("Ooopsie!", "Tehe, it's not implemented yet :P", "Understandable", "Have a nice day");
			}
		}

		[MenuItem("Window/S4 Scn/Scn IO <3",priority = 0)]
		public static void OpenWindow()
		{
			GetWindow(typeof(ScnToolMenu));
		}

		public bool ScnIsSelected()
		{
			GameObject selected = Selection.activeGameObject;
			if (selected)
			{
				if (selected.GetComponent<ScnData>())
				{
					return true;
				}
			}
			return false;
		}

		[MenuItem("Window/S4 Scn/Load Aaaaaall maps!!!")]
		public static void LoadAllS4Scns()
		{
			string resourcesFolder = EditorUtility.OpenFolderPanel("Select the resources folder!", "", "");
			if (resourcesFolder == string.Empty)
			{
				return;
			}

			string[] files = Directory.GetFiles(resourcesFolder + "\\mapinfo", "*.ini");

			if (AssetDatabase.IsValidFolder("Assets/AllScns") == false)
			{
				AssetDatabase.CreateFolder("Assets", "AllScns");
			}

			foreach (string file in files)
			{
				var sr = File.OpenText(file);
				bool readstatic = false;
				bool readdynamic = false;
				List<string> scnNames = new List<string>();
				while (sr.EndOfStream == false)
				{
					string line = sr.ReadLine();
					if (line == string.Empty) continue;
					if (readstatic)
					{
						if (readdynamic)
						{
							if (line.StartsWith("["))
							{
								break;
							}
							else
							{
								if (line.StartsWith(";")) continue;

								string[] parts = line.Split("=");
								if (parts.Length == 2)
								{
									scnNames.Add(parts[1]);
								}
							}
						}
						else if (line == "[DYNAMIC]")
						{
							readdynamic = true;
						}
						else
						{
							if (line.StartsWith(";")) continue;

							string[] parts = line.Split("=");
							if (parts.Length == 2)
							{
								scnNames.Add(parts[1]);
							}
						}
					}
					else if (line == "[STATIC]")
					{
						readstatic = true;
					}
				}

				Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
				EditorSceneManager.SetActiveScene(scene);

				string folder = new FileInfo(file).Name.Replace(".ini", "");
				if (AssetDatabase.IsValidFolder("Assets/AllScns/" + folder) == false)
				{
					AssetDatabase.CreateFolder("Assets/AllScns", folder);
				}
				if (AssetDatabase.IsValidFolder("Assets/AllScns/" + folder + "/Materials") == false)
				{
					AssetDatabase.CreateFolder("Assets/AllScns/" + folder, "Materials");
				}
				if (AssetDatabase.IsValidFolder("Assets/AllScns/" + folder + "/Materials/Lightmaps") == false)
				{
					AssetDatabase.CreateFolder("Assets/AllScns/" + folder + "/Materials", "Lightmaps");
				}

				//Clear the materials so we get a clean list of the materials that we generated for this scn
				ModelChunkImporter.MainMaterials.Clear();
				ModelChunkImporter.SideMaterials.Clear();

				foreach (string scnName in scnNames)
				{
					if (File.Exists(resourcesFolder + "\\model\\background\\" + scnName) == false) continue;

					SceneContainer container = SceneContainer.ReadFrom(resourcesFolder + "\\model\\background\\" + scnName);
					container.fileInfo = new FileInfo(resourcesFolder + "\\model\\background\\" + scnName);

					GameObject sceneObj = new GameObject(container.Header.Name);
					sceneObj.AddComponent<ScnData>().folderPath = container.fileInfo.Directory.FullName;


					ScnFileImporter.BuildFromContainer(container, sceneObj);
				}

				foreach (var item in ModelChunkImporter.MainMaterials)
				{
					AssetDatabase.CreateAsset(item.Value, "Assets/AllScns/" + folder + "Materials/" + new FileInfo(item.Key).Name);
				}
				foreach (var item in ModelChunkImporter.SideMaterials)
				{
					AssetDatabase.CreateAsset(item.Value, "Assets/AllScns/" + folder + "Materials/Lightmaps/" + new FileInfo(item.Key).Name);
				}
				AssetDatabase.SaveAssets();

				EditorSceneManager.SaveScene(scene, "Assets/AllScns/" + folder + "/" + folder + ".unity");

				//also save texturesand maybe materials, or maybe make global materials for certain things
			}
		}
	}
}