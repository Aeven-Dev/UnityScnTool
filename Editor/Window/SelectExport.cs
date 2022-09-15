using NetsphereScnTool.Scene;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using AevenScnTool.IO;

namespace AevenScnTool.Menus
{
    public class SelectExport : EditorWindow
    {
        Vector2 scrollPos = Vector2.zero;
        SelectableItem[] scenesInHierarchy = null;

        bool saveLightmaps = false;

        void OnGUI()
        {
			if (scenesInHierarchy == null)
			{
                Close();
			}
			GUILayout.Label("Select the scenes to compile! <3", EditorStyles.boldLabel);
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos, false, true, GUIStyle.none, GUI.skin.verticalScrollbar, GUI.skin.box, GUILayout.MinHeight(position.height - 105f));

            DrawItems();

            EditorGUILayout.EndScrollView();

            GUILayout.FlexibleSpace();
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Select All! :O"))
            {
                foreach (var item in scenesInHierarchy)
                {
                    item.selected = true;
                }
            }
            if (GUILayout.Button("Select None! :X"))
            {
                foreach (var item in scenesInHierarchy)
                {
                    item.selected = false;
                }
            }
            if (GUILayout.Button("Invert! o'o"))
            {
                foreach (var item in scenesInHierarchy)
                {
                    item.selected = !item.selected;
                }
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(10);
            saveLightmaps = GUILayout.Toggle(saveLightmaps, "Save Lightmaps too!");
            GUILayout.Space(10);


            if (GUILayout.Button("Export! <3"))
            {
                Export();
            }
        }

        public static void Open()
        {
            SelectExport window = (SelectExport)GetWindow(typeof(SelectExport));
            window.titleContent = new GUIContent("Export Selection!");

            List<SelectableItem> scenesInHierarchy = new List<SelectableItem>();
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                Scene scene = SceneManager.GetSceneAt(i);
                List<SelectableItem> scns = new List<SelectableItem>();
                if (scene.isLoaded == false)
                {
                    continue;
                }
                GameObject[] rootGO = scene.GetRootGameObjects();

                for (int j = 0; j < rootGO.Length; j++)
                {
                    ScnData sd = rootGO[j].GetComponent<ScnData>();
                    if (sd)
                    {
                        scns.Add(new SelectableItem(sd.name, sd, new SelectableItem[0]));
                    }
                }
                scenesInHierarchy.Add(new SelectableItem(scene.name, scene, scns.ToArray()));
                if (SceneManager.GetSceneAt(i) == SceneManager.GetActiveScene())
                {
                    scenesInHierarchy[scenesInHierarchy.Count - 1].selected = true;
                }
            }
            window.scenesInHierarchy = scenesInHierarchy.ToArray();
            EditorApplication.quitting += window.Close;
        }

        void DrawItems()
        {
            foreach (var item in scenesInHierarchy)
            {
                item.selected = EditorGUILayout.ToggleLeft(item.name, item.selected);
                EditorGUI.indentLevel = 1;
                GUI.enabled = item.selected;
                foreach (var scn in item.childs)
                {
                    scn.selected = EditorGUILayout.ToggleLeft(scn.name, scn.selected);
                }
                GUI.enabled = true;

                EditorGUI.indentLevel = 0;
            }
        }

        void Export()
        {
            string fileName = EditorUtility.SaveFilePanel("Select a location", ScnToolData.Instance.s4_folder_path, SceneManager.GetActiveScene().name, "scn");
            if (fileName == string.Empty) return;

            FileInfo fileInfo = new FileInfo(fileName);

            List<ScnData> scnData = new List<ScnData>();
            foreach (var item in scenesInHierarchy)
            {
                if (item.selected == false) continue;
                foreach (var scn in item.childs)
                {
                    if (scn.selected == false) continue;
                    scnData.Add(scn.element as ScnData);
                }
            }

            SceneContainer container = ScnFileExporter.CreateContainerFromScenes(fileInfo, scnData.ToArray());
            container.Write(fileInfo.FullName);

			if (saveLightmaps)
			{
                foreach (var tex in ScnFileExporter.lightmaps)
                {
					if (tex.isReadable)
					{
                        continue;
					}
                    string assetPath = AssetDatabase.GetAssetPath(tex);
                    var tImporter = AssetImporter.GetAtPath(assetPath) as TextureImporter;
                    if (tImporter != null)
                    {
                        tImporter.textureType = TextureImporterType.Lightmap;

                        tImporter.isReadable = true;
                        tImporter.textureCompression = TextureImporterCompression.Uncompressed;

                        AssetDatabase.ImportAsset(assetPath);
                    }
                }

                AssetDatabase.Refresh();
                foreach (var tex in ScnFileExporter.lightmaps)
                {
                    try
                    {
                        var bytes = tex.EncodeToTGA();
                        File.WriteAllBytes(fileInfo.Directory.FullName + "\\" + tex.name + ".tga", bytes);
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogError(e.Message);
                    }
                }
            }

            Close();
        }
    }

    public class SelectableItem
    {
        public string name;
        public object element;
        public List<SelectableItem> childs;
        public bool selected = true;
        public bool displayed = true;

        public SelectableItem(string name, object element, SelectableItem[] childs)
        {
            this.name = name;
            this.element = element;
            this.childs = new List<SelectableItem>(childs);
        }
        public SelectableItem(string name, object element, List<SelectableItem> childs)
        {
            this.name = name;
            this.element = element;
            this.childs = new List<SelectableItem>(childs);
        }

        public static List<T> GetRootSelectedItems<T>(SelectableItem[] collection)
        {
            List<T> selected = new List<T>();
            foreach (var item in collection)
            {
                if (item.selected)
                {
                    selected.Add((T)item.element);
                }
            }
            return selected;
        }
        public static List<T> GetRecursiveSelectedItems<T>(SelectableItem[] collection, bool branchCutoff)
        {
            List<T> list = new List<T>();
            foreach (var item in collection)
            {
                if (branchCutoff)
                {
                    if (item.selected)
                    {
                        list.Add((T)item.element);
                        list.AddRange(GetRecursiveSelectedItems<T>(item.childs.ToArray(), branchCutoff));
                    }
                }
                else
                {
                    if (item.selected)
                    {
                        list.Add((T)item.element);
                    }
                    list.AddRange(GetRecursiveSelectedItems<T>(item.childs.ToArray(), branchCutoff));
                }
            }
            return list;
        }
    }
}