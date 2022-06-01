using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;


namespace AevenScnTool.Menus
{
    public class MapInfoWindow : EditorWindow
    {
        string text;
        SelectableItem[] scenesInHierarchy = null;
        void OnGUI()
        {
            if (scenesInHierarchy == null)
            {
                Close();
            }
            GUILayout.Label("Copy this text in the map info ini! <3", EditorStyles.boldLabel);
            GUILayout.BeginHorizontal();
            EditorGUILayout.TextArea(text, GUILayout.MinHeight(position.height - 23), GUILayout.Width(position.width - 170));
            GUILayout.BeginVertical();
            foreach (var item in scenesInHierarchy)
            {
                item.selected = EditorGUILayout.ToggleLeft(item.name, item.selected);
            }
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Generate Map Info! >.<'", GUILayout.Height(50), GUILayout.Width(161)))
            {
                GenerateMapInfoText();
            }
            if (GUILayout.Button("Generate Sector Info! >.<'", GUILayout.Height(50), GUILayout.Width(161)))
            {
                GenerateSectorInfoText();
            }
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
        }

        private void GenerateMapInfoText()
        {
            List<GameObject> go_list = new List<GameObject>();
            List<Scene> scenes = SelectableItem.GetRootSelectedItems<Scene>(scenesInHierarchy);
            foreach (var scene in scenes)
            {
                go_list.AddRange(ObjectCreation.GetAllGameObjectsFromScene(scene));
            }

            List<BlastData> bd = new List<BlastData>();
            List<DOTData> dotd = new List<DOTData>();
            List<SeizeData> sd = new List<SeizeData>();
            List<WarpGateData> wgd = new List<WarpGateData>();
            List<EventItemPosData> eipd = new List<EventItemPosData>();
            List<SpectatorCameraData> scd = new List<SpectatorCameraData>();

            foreach (var item in go_list)
            {
                BlastData blast = item.GetComponent<BlastData>();
                if (blast) bd.Add(blast);
                DOTData dot = item.GetComponent<DOTData>();
                if (dot) dotd.Add(dot);
                SeizeData seize = item.GetComponent<SeizeData>();
                if (seize) sd.Add(seize);
                WarpGateData warp = item.GetComponent<WarpGateData>();
                if (warp) wgd.Add(warp);
                EventItemPosData eventitem = item.GetComponent<EventItemPosData>();
                if (eventitem) eipd.Add(eventitem);
                SpectatorCameraData cam = item.GetComponent<SpectatorCameraData>();
                if (cam) scd.Add(cam);
            }


            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < bd.Count; i++) { bd[i].Parse(sb, i + 1); sb.AppendLine(); }
            if (dotd.Count > 0) { sb.AppendLine(); sb.AppendLine("[DOT_STATIC]"); }
            for (int i = 0; i < dotd.Count; i++) { dotd[i].Parse(sb, i + 1); sb.AppendLine(); }
            for (int i = 0; i < sd.Count; i++) { sd[i].Parse(sb, i + 1); sb.AppendLine(); }
            for (int i = 0; i < wgd.Count; i++) { wgd[i].Parse(sb, i + 1); sb.AppendLine(); }
            for (int i = 0; i < scd.Count; i++) { scd[i].Parse(sb, i + 1); sb.AppendLine(); }
            if (eipd.Count > 0)
            {
                sb.AppendLine(); sb.AppendLine("[EVENT_ITEM_POS]");
                for (int i = 0; i < eipd.Count; i++) { eipd[i].Parse(sb, i + 1); sb.AppendLine(); }
                sb.AppendLine("Pos_Count=" + eipd.Count);
            }
            text = sb.ToString();
        }

        private void GenerateSectorInfoText()
        {
            List<GameObject> go_list = new List<GameObject>();
            List<Scene> scenes = SelectableItem.GetRootSelectedItems<Scene>(scenesInHierarchy);
            foreach (var scene in scenes)
            {
                go_list.AddRange(ObjectCreation.GetAllGameObjectsFromScene(scene));
            }

            List<SectorData> sd = new List<SectorData>();

            foreach (var item in go_list)
            {
                SectorData sector = item.GetComponent<SectorData>();
                if (sector) sd.Add(sector);
            }


            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < sd.Count; i++) { sd[i].ParseContent(sb, i + 1); }
            string content = sb.ToString();

            sb.Clear();
            for (int i = 0; i < sd.Count; i++) { sd[i].ParseTitle(sb, i + 1); sb.AppendLine(content); }

            text = sb.ToString();
        }

        public static void Init()
        {
            MapInfoWindow window = (MapInfoWindow)GetWindow(typeof(MapInfoWindow));
            window.text = "";
            window.scenesInHierarchy = new SelectableItem[SceneManager.sceneCount];
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                Scene scene = SceneManager.GetSceneAt(i);
                List<SelectableItem> scns = new List<SelectableItem>();
                GameObject[] rootGO = scene.GetRootGameObjects();

                for (int j = 0; j < rootGO.Length; j++)
                {
                    ScnData sd = rootGO[i].GetComponent<ScnData>();
                    if (sd)
                    {
                        scns.Add(new SelectableItem(sd.name, sd, new SelectableItem[0]));
                    }
                }

                window.scenesInHierarchy[i] = new SelectableItem(scene.name, scene, scns.ToArray());
                if (SceneManager.GetSceneAt(i) == SceneManager.GetActiveScene())
                {
                    window.scenesInHierarchy[i].selected = true;
                }
            }

            window.GenerateMapInfoText();


            EditorApplication.quitting += window.Close;
        }
    }
}