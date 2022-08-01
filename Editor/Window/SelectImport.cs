using NetsphereScnTool.Scene;
using NetsphereScnTool.Scene.Chunks;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEditor;
using UnityEngine;
using AevenScnTool.IO;

namespace AevenScnTool.Menus
{
    public class SelectImport : EditorWindow
    {
        Vector2 scrollPos = Vector2.zero;
        SceneContainer container;
        public List<SelectableItem> roots = new List<SelectableItem>();

        GameObject sceneObj = null;

        void OnGUI()
        {
            GUILayout.Label("Select what parts you want to have! <3", EditorStyles.boldLabel);
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos, false, true, GUIStyle.none, GUI.skin.verticalScrollbar, GUI.skin.box, GUILayout.MinHeight(position.height - 70f));

            foreach (var item in roots)
            {
                DrawNode(item, 0);
            }
            EditorGUILayout.EndScrollView();
            GUILayout.FlexibleSpace();
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Select All! :O"))
            {
                SetAllTo(roots, true);
            }
            if (GUILayout.Button("Select None! :X"))
            {
                SetAllTo(roots, false);
            }
            if (GUILayout.Button("Invert! o'o"))
            {
                Invert(roots);
            }
            GUILayout.EndHorizontal();

            if (GUILayout.Button("Import! <3"))
            {
                Import();
            }
        }

        void DrawNode(SelectableItem item, int indentation)
        {
            EditorGUI.indentLevel = indentation;

            Rect pos = EditorGUILayout.BeginHorizontal();
            if (item.childs.Count != 0)
            {
                bool displayed = EditorGUILayout.Foldout(item.displayed, "");
                if (item.displayed != displayed)
                {
                    item.displayed = displayed;
                    if (Event.current.modifiers == EventModifiers.Alt)
                    {
                        CascadeDisplay(item.childs, displayed);
                        Event.current.Use();
                    }
                }
            }
            else
            {
                EditorGUILayout.LabelField("");
            }
            pos.x += 20f;

            bool selected = EditorGUI.ToggleLeft(pos, item.name, item.selected);
            if (item.selected != selected)
            {
                item.selected = selected;
                if (Event.current.modifiers == EventModifiers.Alt)
                {
                    SetAllTo(item.childs, selected);
                    Event.current.Use();
                }
            }

            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            if (item.displayed)
            {
                for (int i = 0; i < item.childs.Count; i++)
                {
                    DrawNode(item.childs[i], indentation + 1);
                }
            }
        }

        void Import()
        {
            List<SceneChunk> selectedChunks = GetSelectedChunks(roots);

            SceneContainer newContainer = new SceneContainer(selectedChunks);
            newContainer.Header = container.Header;
            newContainer.fileInfo = container.fileInfo;

            if (!sceneObj)
            {
                sceneObj = new GameObject(container.Header.Name);
                sceneObj.AddComponent<ScnData>().folderPath = container.fileInfo.Directory.FullName;
            }

            ScnFileImporter.BuildFromContainer(newContainer, sceneObj, ScnToolMenu.identityMatrix);

            sceneObj = null;
            Close();
        }

        public static void Open(SceneContainer container)
        {
            SelectImport window = (SelectImport)GetWindow(typeof(SelectImport));
            window.titleContent = new GUIContent("Import Selection!");
            window.container = container;

            window.roots = window.GetSelectedItems(container);
            window.sceneObj = null;

            EditorApplication.quitting += window.Close;
        }

        public static void OpenTo(SceneContainer container, ScnData sceneData)
        {
            SelectImport window = (SelectImport)GetWindow(typeof(SelectImport));
            window.container = container;

            window.roots = window.GetSelectedItems(container);
            window.sceneObj = sceneData.gameObject;

            EditorApplication.quitting += window.Close;
        }

        List<SceneChunk> GetSelectedChunks(List<SelectableItem> items)
        {
            List<SceneChunk> list = new List<SceneChunk>();
            foreach (var item in items)
            {
                if (item.selected)
                {
                    list.Add((SceneChunk)item.element);
                }
                list.AddRange(GetSelectedChunks(item.childs));

            }
            return list;
        }

        List<SelectableItem> GetSelectedItems(Collection<SceneChunk> chunks)
        {
            List<SelectableItem> items = new List<SelectableItem>();
            List<SelectableItem> roots = new List<SelectableItem>();
            foreach (var chunk in chunks)
            {
                items.Add(new SelectableItem(chunk.Name, chunk, new List<SelectableItem>()));
            }
            foreach (var item in items)
            {
				if (((SceneChunk)item.element).SubName == container.Header.Name)
				{
                    roots.Add(item);
                    continue;
                }
                bool foundParent = false;
                for (int i = 0; i < items.Count; i++)
                {
                    if (((SceneChunk)item.element).SubName == ((SceneChunk)items[i].element).Name)
                    {
                        items[i].childs.Add(item);
                        foundParent = true;
                        break;
                    }
                }
                if (foundParent == false)
                {
                    roots.Add(item);
                }
            }
            return roots;
        }

        void SetAllTo(List<SelectableItem> items, bool state)
        {
            foreach (var item in items)
            {
                item.selected = state;
                SetAllTo(item.childs, state);
            }
        }

        void Invert(List<SelectableItem> items)
        {
            foreach (var item in items)
            {
                item.selected = !item.selected;
                Invert(item.childs);
            }
        }

        void CascadeDisplay(List<SelectableItem> items, bool state)
        {

            foreach (var item in items)
            {
                item.displayed = state;
                CascadeDisplay(item.childs, state);
            }
        }
    }

    public class SelectItem
    {
        public bool displayed = true;
        public SceneChunk chunk;
        public bool selected = true;
        public List<SelectItem> childs;

        public SelectItem(SceneChunk chunk, List<SelectItem> childs)
        {
            this.chunk = chunk;
            this.childs = childs;
        }
    }
}