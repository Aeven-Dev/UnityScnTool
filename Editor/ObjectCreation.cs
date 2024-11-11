using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace AevenScnTool.Menus
{
    public class ObjectCreation
    {
        public static void SetGameObject(GameObject gameObject)
        {
            MeshRenderer mr = gameObject.GetComponent<MeshRenderer>();
            SkinnedMeshRenderer smr = gameObject.GetComponent<SkinnedMeshRenderer>();
            MeshCollider mc = gameObject.GetComponent<MeshCollider>();
            if (mr || smr)
            {
                TextureReference tf = gameObject.GetComponent<TextureReference>();
                if (tf == null)
                {
                    AddTextureReference(gameObject);
                }
                else
                {
                    tf.LoadFromMaterial();
                }
            }
            if (smr)
            {
                foreach (Transform bone in smr.bones)
                {
                    if (bone.GetComponent<Bone>() == null)
                    {
                        bone.gameObject.AddComponent<Bone>();
                    }
                }
            }
            if (mc)
            {
                CollisionData cd = gameObject.GetComponent<CollisionData>();
                if (cd == null)
                {
                    gameObject.AddComponent<CollisionData>();
                }
            }
        }

        static void AddTextureReference(GameObject gameObject)
        {
            gameObject.AddComponent<TextureReference>().LoadFromMaterial();
        }


        [MenuItem("GameObject/S4 Scn/Create Scn! 8D")]
        static void CreateMesh()
        {
            GameObject go = new GameObject("Your Brand New Scn! 8D");
            go.AddComponent<ScnData>();
        }


        [MenuItem("GameObject/S4 Scn/Create Mesh! :D")]
        static void CreateScn()
        {
            GameObject go = new GameObject("New Mesh! :D");
            go.AddComponent<MeshFilter>();
            go.AddComponent<MeshRenderer>();
            go.AddComponent<TextureReference>();
            Parent(go);
        }

        [MenuItem("GameObject/S4 Scn/Create Collider! :O")]
        static void CreateCollider()
        {
            GameObject go = new GameObject("New Collider! :O");
            go.AddComponent<MeshCollider>().cookingOptions = MeshColliderCookingOptions.None;
            go.AddComponent<CollisionData>();
            Parent(go);
        }

        [MenuItem("GameObject/S4 Scn/Bone/Create Bone! 8==8")]
        static void CreateBone()
        {
            GameObject go = new GameObject("New Bone! 8==8");
            go.AddComponent<Bone>().s4Animations = go.AddComponent<S4Animations>();
            Parent(go);
        }

        [MenuItem("GameObject/S4 Scn/Box/Create Alpha Spawn! \\o")]
        static void CreateAlphaSpawn()
        {
            GameObject go = new GameObject("alpha_spawn_pos_XX");
            go.AddComponent<BoxCollider>().size = Vector3.one * 100f / ScnToolData.Instance.scale;
            go.AddComponent<PointDrawer>().type = PointDrawer.PointType.alpha_spawn_pos;
            go.transform.rotation = Quaternion.Euler(-90f, 0f, 0f);
            Parent(go);
        }

        [MenuItem("GameObject/S4 Scn/Box/Create Beta Spawn! \\o")]
        static void CreateBetaSpawn()
        {
            GameObject go = new GameObject("beta_spawn_pos_XX");
            go.AddComponent<BoxCollider>().size = Vector3.one * 100f / ScnToolData.Instance.scale;
            go.AddComponent<PointDrawer>().type = PointDrawer.PointType.beta_spawn_pos;
            go.transform.rotation = Quaternion.Euler(-90f, 0f, 0f);
            Parent(go);
        }

        [MenuItem("GameObject/S4 Scn/Box/TD/Create Fumbi Spawn! (o)")]
        static void CreateBallSpawn()
        {
            GameObject go = new GameObject("ball_spawn_pos");
            go.AddComponent<BoxCollider>().size = Vector3.one * 100f / ScnToolData.Instance.scale;
            go.AddComponent<PointDrawer>().type = PointDrawer.PointType.ball_spawn_pos;
            Parent(go);
        }

        [MenuItem("GameObject/S4 Scn/Box/TD/Create Alpha Goal! ((_))")]
        static void CreateAlphaGoal()
        {
            GameObject go = new GameObject("alpha_net");
            go.AddComponent<BoxCollider>().size = new Vector3(300, 125.4f, 300) / ScnToolData.Instance.scale;
            go.AddComponent<PointDrawer>().type = PointDrawer.PointType.alpha_net;
            Parent(go);
        }

        [MenuItem("GameObject/S4 Scn/Box/TD/Create Beta Goal! ((_))")]
        static void CreateBetaGoal()
        {
            GameObject go = new GameObject("beta_net");
            go.AddComponent<BoxCollider>().size = new Vector3(300, 125.4f, 300) / ScnToolData.Instance.scale;
            go.AddComponent<PointDrawer>().type = PointDrawer.PointType.beta_net;
            Parent(go);
        }

        [MenuItem("GameObject/S4 Scn/Box/Create Alpha death zone! DX")]
        static void CreateAlphaDeathZone()
        {
            GameObject go = new GameObject("alpha_limited_areaXX");
            go.AddComponent<BoxCollider>().size = Vector3.one * 1000f / ScnToolData.Instance.scale;
            Parent(go);
        }

        [MenuItem("GameObject/S4 Scn/Box/Create Beta death zone! XP")]
        static void CreateBetaDeathZone()
        {
            GameObject go = new GameObject("alpha_limited_areaXX");
            go.AddComponent<BoxCollider>().size = Vector3.one * 1000f / ScnToolData.Instance.scale;
            Parent(go);
        }

        [MenuItem("GameObject/S4 Scn/Box/Create Jump Pad! [^^^]")]
        static void CreateJumpPad()
        {
            GameObject go = new GameObject("jump_dirXX");
            BoxCollider c = go.AddComponent<BoxCollider>();
            c.size = Vector3.one * 1000f / ScnToolData.Instance.scale;

            GameObject power = new GameObject("powerXX");
            BoxCollider p = power.AddComponent<BoxCollider>();
            p.size = new Vector3(10, 10, 2000) / ScnToolData.Instance.scale;

            Jumppad j = go.AddComponent<Jumppad>();
            j.power = p;
            j.thisColl = c;
            power.transform.SetParent(go.transform);
            power.transform.rotation = Quaternion.Euler(-45, 0, 0);
            power.transform.position = j.transform.position + power.transform.forward * p.size.z / 2f;

            Parent(go);
        }

        [MenuItem("GameObject/S4 Scn/Box/Create Warp Gate! O -> O")]
        static void CreateWarpGate()
        {
            GameObject go = new GameObject("New Warp Gate! O -> O");
            go.AddComponent<BoxCollider>().size = Vector3.one * 1000f / ScnToolData.Instance.scale;
            go.AddComponent<WarpGateData>();

            Parent(go);
        }

        [MenuItem("GameObject/S4 Scn/Box/Create DOT Area! <^^>")]
        static void CreateDOTArea()
        {
            GameObject go = new GameObject("New DOT Area! <^^>");
            go.AddComponent<BoxCollider>().size = Vector3.one * 1000f / ScnToolData.Instance.scale;
            go.AddComponent<DOTData>();

            Parent(go);
        }

        [MenuItem("GameObject/S4 Scn/Create Breakable! [#]")]
        static void CreateBlast()
        {
            GameObject go = new GameObject("New Breakable! [#]");
            go.AddComponent<Bone>();
            go.AddComponent<BlastData>();

            GameObject mesh = GameObject.CreatePrimitive(PrimitiveType.Cube);
            Object.DestroyImmediate(mesh.GetComponent<BoxCollider>());
            mesh.name = "New Mesh! :D";
            mesh.AddComponent<TextureReference>();

            GameObject coll = new GameObject("New Collider! :O");
            coll.AddComponent<MeshCollider>().sharedMesh = mesh.GetComponent<MeshFilter>().sharedMesh;
            coll.AddComponent<CollisionData>().ground = GroundFlag.blast;
            coll.AddComponent<TextureReference>();

            coll.transform.SetParent(mesh.transform);
            mesh.transform.SetParent(go.transform);

            Parent(go);
        }

        [MenuItem("GameObject/S4 Scn/Siege/Create Siege Point! _�_")]
        static void CreateSiegePoint()
        {
            GameObject go = new GameObject("Point holder!");
            GameObject smolbox = new GameObject("seize_xx");
            GameObject beegbox = new GameObject("seize_outbox_xx");

            go.AddComponent<SeizeData>().seize_slot = 1;
            smolbox.AddComponent<BoxCollider>().size = Vector3.one * 1000f / ScnToolData.Instance.scale;
            beegbox.AddComponent<BoxCollider>().size = Vector3.one * 1000f / ScnToolData.Instance.scale;

            beegbox.transform.SetParent(go.transform);
            smolbox.transform.SetParent(go.transform);

            Parent(go);
        }

        private static double renameTime;
        static void Parent(GameObject go)
        {
            if (Selection.activeTransform != null)
            {
                go.transform.SetParent(Selection.activeTransform, false);
            }

            EditorApplication.update += RenameMode;
            renameTime = EditorApplication.timeSinceStartup + 0.4d;
            EditorApplication.ExecuteMenuItem("Window/General/Hierarchy");
            Selection.activeGameObject = go;
        }

        static void RenameMode()
        {
            if (EditorApplication.timeSinceStartup >= renameTime)
            {
                EditorApplication.update -= RenameMode;
                var e = Event.KeyboardEvent("f2");
                EditorWindow.focusedWindow.SendEvent(e);
            }
        }

        [MenuItem("GameObject/S4 Scn/Swap Teams! :3")]
        static void SwapTeam()
        {
			for (int i = 0; i < Selection.gameObjects.Length; i++)
			{
                if (Selection.activeTransform == null)
                {
                    Debug.Log("You need to have an object selected, silly! :P");
                    return;
                }
                if (Selection.gameObjects[i].name.Contains("alpha"))
                {
                    Selection.gameObjects[i].name=Selection.gameObjects[i].name.Replace("alpha", "beta");
                }
                else if (Selection.gameObjects[i].name.Contains("beta"))
                {
                    Selection.gameObjects[i].name=Selection.gameObjects[i].name.Replace("beta", "alpha");
                }
                else if (Selection.gameObjects[i].name.Contains("red"))
                {
                    Selection.gameObjects[i].name=Selection.gameObjects[i].name.Replace("red", "blue");
                }
                else if (Selection.gameObjects[i].name.Contains("blue"))
                {
                    Selection.gameObjects[i].name=Selection.gameObjects[i].name.Replace("blue", "red");
                }
                else
                {
                    Debug.Log("Your object isnt name alpha, beta, red or blue! There is nothing i can do! :P");
                }
            }
        }
        [MenuItem("GameObject/S4 Scn/Mesh to Collider! :D => :O")]
        static void MeshToCollider()
		{
            for (int i = 0; i < Selection.gameObjects.Length; i++)
            {
                if (Selection.activeTransform == null)
                {
                    Debug.Log("You need to have an object selected, silly! :P");
                    return;
                }
                var go = Selection.gameObjects[i];

                var mf = go.GetComponent<MeshFilter>();
				if (!mf)
				{
                    continue;
				}
                var tr = go.GetComponent<TextureReference>();
                var mr = go.GetComponent<MeshRenderer>();
                var mc = go.AddComponent<MeshCollider>();
                mc.cookingOptions = MeshColliderCookingOptions.None;
                mc.sharedMesh = mf.sharedMesh;
                go.AddComponent<CollisionData>();
                if (tr)
                {
					Object.DestroyImmediate(tr);
                }
				if (mr)
				{
                    Object.DestroyImmediate(mr);
				}
				if (mf)
				{
                    Object.DestroyImmediate(mf);
				}
            }
        }

        [MenuItem("GameObject/S4 Scn/Collider to Mesh! :0 => :D")]
        static void ColliderToMesh()
		{
            for (int i = 0; i < Selection.gameObjects.Length; i++)
            {
                if (Selection.activeTransform == null)
                {
                    Debug.Log("You need to have an object selected, silly! :P");
                    return;
                }
                var go = Selection.gameObjects[i];

                var mc = go.GetComponent<MeshCollider>();
				if (!mc)
				{
                    continue;
				}
                var tr = go.GetComponent<TextureReference>();
                if (tr == null)
                {
                    tr = go.AddComponent<TextureReference>();
                }
                var mf = go.AddComponent<MeshFilter>();
                var mr = go.AddComponent<MeshRenderer>();
                mr.material = ScnToolData.GetMatFromShader(RenderFlag.None);
                var cd = go.GetComponent<CollisionData>();
                mc.cookingOptions = MeshColliderCookingOptions.None;
                mf.sharedMesh = mc.sharedMesh;
                if(go.name.StartsWith("oct_") == false){
                    if(cd.ground == GroundFlag.blast){
                        go.name = "oct_" + go.name;
                    }
                    else if(cd.ground == GroundFlag.NONE){
                        if(cd.weapon != WeaponFlag.NONE){
                            go.name = "oct_" + nameof(cd.weapon).Replace("wire","@");
                        }
                    }
                    else{
                        go.name = "oct_" + nameof(cd.ground).Replace("wire","@").Replace("hash","#");
                    }
                    go.name = "oct_" + go.name;
                }
				if (cd)
				{
                    Object.DestroyImmediate(cd);
				}
                if (mc)
                {
					Object.DestroyImmediate(mc);
                }
            }
        }


        public static GameObject[] GetAllGameObjectsFromScene(Scene scene)
        {
            GameObject[] rootGO = scene.GetRootGameObjects();
            HashSet<GameObject> allGameObjects = new HashSet<GameObject>();
            for (int i = 0; i < rootGO.Length; i++)
            {
                Transform[] go = rootGO[i].GetComponentsInChildren<Transform>();
                for (int j = 0; j < go.Length; j++)
                {
                    allGameObjects.Add(go[j].gameObject);
                }
            }
            GameObject[] array = new GameObject[allGameObjects.Count];
            allGameObjects.CopyTo(array);
            return array;
        }

        public static GameObject[] GetAllGameObjectsFromScn(ScnData scene)
        {
            HashSet<GameObject> allGameObjects = new HashSet<GameObject>();
            for (int i = 0; i < scene.transform.childCount; i++)
            {
                Transform[] go = scene.transform.GetChild(i).GetComponentsInChildren<Transform>();
                for (int j = 0; j < go.Length; j++)
                {
                    allGameObjects.Add(go[j].gameObject);
                }
            }
            GameObject[] array = new GameObject[allGameObjects.Count];
            allGameObjects.CopyTo(array);
            return array;
        }

        public static GameObject[] GetAllChildsRecursively(Transform parent)
        {
            HashSet<GameObject> allGameObjects = new HashSet<GameObject>();
            Transform[] go = parent.GetComponentsInChildren<Transform>();
            for (int j = 0; j < go.Length; j++)
            {
                allGameObjects.Add(go[j].gameObject);
            }
            GameObject[] array = new GameObject[allGameObjects.Count];
            allGameObjects.CopyTo(array);
            return array;
        }
    }
}