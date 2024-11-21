using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace AevenScnTool
{
    [AddComponentMenu("S4 scn/Texture Reference")]
    [ExecuteInEditMode]
    public class TextureReference : MonoBehaviour
    {
        [Range(0f,1f)]public float transparency = 1f;
        public RenderFlag renderFlags;
        //[SerializeField]private RenderFlag RenderFlags;

        public bool flipUvHorizontal = false;

        public bool flipUvHorizontal_lm = false;

        public bool ignoreLightmaps = false;

        public bool isNormal = false;

        public List<TextureItem> textures = new List<TextureItem>();

        public bool hasLightmap { get { return HasLightmap(); } }

        [Button("Load From Material! <3")] public ButtonAction loadFromMaterial;
        [Button("Load To Material! :>")] public ButtonAction loadToMaterial;
        [Button("Save Mesh! :3")] public ButtonAction saveMesh;
        [Button("Copy Textures! o.O")] public ButtonAction copyTextures;
        [Button("Save Textures! o.O/DDS! (experimental)")] public ButtonAction saveTexturesDDS;
        [Button("Save Textures! o.O/TGA!")] public ButtonAction saveTexturesTGA;
        [Button("Remove Lightmap! U.u'")] public ButtonAction removeLightmaps;

        private void OnEnable()
        {
            loadFromMaterial = new ButtonAction(LoadFromMaterial);
            loadToMaterial = new ButtonAction(LoadToMaterial);
            saveMesh = new ButtonAction(SaveMesh);
            copyTextures = new ButtonAction(CopyTextures);
            saveTexturesDDS = new ButtonAction(SaveTexturesDds);
            saveTexturesTGA = new ButtonAction(SaveTexturesTga);
            removeLightmaps = new ButtonAction(RemoveLightmap);
        }

		[ContextMenu("Load from materials! <3")]
        public void LoadFromMaterial()
        {
            Material[] mats;
            MeshRenderer mr = GetComponent<MeshRenderer>();
            SkinnedMeshRenderer smr = GetComponent<SkinnedMeshRenderer>();
            Mesh mesh;
            if (mr != null)
            {
                mats = mr.sharedMaterials;
                mesh = mr.GetComponent<MeshFilter>().sharedMesh;
            }
            else if (smr != null)
            {
                mats = smr.sharedMaterials;
                mesh = smr.sharedMesh;
            }
            else
            {
                mats = new Material[0];
                mesh = null;
            }

            textures.Clear();
            for (int i = 0; i < mats.Length; i++)
            {
                Texture mainTexture = mats[i].mainTexture;
                string mainTex = string.Empty;

                if (mainTexture)
                {
                    mainTex = UnityEditor.AssetDatabase.GetAssetPath(mainTexture);
                    if (mainTex == string.Empty)
                    {
                        mainTex = mainTexture.name;
                    }
                }

                Texture lightmap = mats[i].GetTexture("_DetailAlbedoMap");
                Texture normal = mats[i].GetTexture("_BumpMap");
                string sideTex;

                if (lightmap)
                {
                    sideTex = AssetDatabase.GetAssetPath(lightmap);
                    if (sideTex == string.Empty)
                    {
                        sideTex = lightmap.name;
                    }
                }
                else if (normal)
                {
                    sideTex = AssetDatabase.GetAssetPath(normal);
                    if (sideTex == string.Empty)
                    {
                        sideTex = normal.name;
                    }
                }
                else
                {
                    sideTex = string.Empty;
                }
                string n = (mainTexture != null) ? mainTexture.name : "EmptyTexture";
                textures.Add(new ( n, mainTex, sideTex));
            }

            if (mesh != null)
            {
                if (mesh.subMeshCount < textures.Count)
                {
                    Debug.LogWarning($"Goodness! Object: {name} has less submeshes than materials, when exporting you'll probably lose some data that way, be careful~~~!", gameObject);
                }
            }
        }

        [ContextMenu("Load to material! :>")]
        public void LoadToMaterial()
        {
            MeshRenderer mr = GetComponent<MeshRenderer>();
            SkinnedMeshRenderer smr = GetComponent<SkinnedMeshRenderer>();
            if (mr == null && smr == null)
            {
                if (GetComponent<MeshCollider>())
                {
                    return;
                }
                Debug.LogWarning("Oh my! You have TextureReference script but you have no Renderer, not even a MeshCollider! That's quite silly, did you delete the renderer? xP, or maybe this script was added by accident! :O better do some cleanup~~~!", gameObject);
                return;
            }

            Material base_mat = null;
            base_mat = ScnToolData.GetMatFromShader(renderFlags);

            List<Material> materials = new List<Material>();
            for (int i = 0; i < textures.Count; i++)
            {
                TextureItem item = textures[i];
                Material mat = new Material(base_mat);

                Texture mainTexture = null;
                Texture sideTexture = null;
                if (item.mainTexturePath != String.Empty)
                {
                    if (AssetDatabase.IsValidFolder(new FileInfo(item.mainTexturePath).Directory.FullName))
                    {
                        mainTexture = AssetDatabase.LoadAssetAtPath<Texture>(item.mainTexturePath);
                    }
                    else
                    {
						if (File.Exists(item.mainTexturePath))
                        {
                            mainTexture = IO.ScnFileImporter.ParseTextureDXT(File.ReadAllBytes(item.mainTexturePath));
                            mainTexture.name = new FileInfo(item.mainTexturePath).Name;
                        }
						else
						{
                            mainTexture = Texture2D.whiteTexture;
                            mainTexture.name = "missing texture";
                        }
                    }
                }

                if (item.sideTexturePath != String.Empty)
                {
                    if (AssetDatabase.IsValidFolder(new FileInfo(item.sideTexturePath).Directory.FullName))
                    {
                        sideTexture = AssetDatabase.LoadAssetAtPath<Texture>(item.sideTexturePath);
                    }
                    else
                    {
                        sideTexture = IO.ScnFileImporter.ParseTextureDXT(File.ReadAllBytes(item.sideTexturePath));
                        sideTexture.name = new FileInfo(item.sideTexturePath).Name;
                    }
                }

                mat.mainTexture = mainTexture;
                if (isNormal)
                {
                    mat.SetTexture("_BumpMap", sideTexture);
                    mat.EnableKeyword("_NORMALMAP");
                }
                else
                {
                    mat.SetTexture("_DetailAlbedoMap", sideTexture);
                    mat.EnableKeyword("_DETAIL_MULX2");
                }
                materials.Add(mat);
            }

            if (mr)
            {
                mr.materials = materials.ToArray();
            }
            else if (smr)
            {
                smr.materials = materials.ToArray();
            }
        }

        [ContextMenu("Save Mesh! :3")]
        public void SaveMesh()
        {
            MeshRenderer mr = GetComponent<MeshRenderer>();
            SkinnedMeshRenderer smr = GetComponent<SkinnedMeshRenderer>();
            MeshCollider mc = GetComponent<MeshCollider>();
            Mesh mesh = null;
            if (mr)
            {
                mesh = mr.GetComponent<MeshFilter>().sharedMesh;
            }
            else if (smr)
            {
                mesh = smr.sharedMesh;
            }
            else if (mc)
            {
                mesh = mc.sharedMesh;
            }
            else
            {
                Debug.LogWarning("Im sorry! I couldnt find any MeshRenderer, SkinnedMeshRenderer or MeshCollider! Is your mesh somewhere else? TT.TT");
                return;
            }
            string path = EditorUtility.SaveFilePanelInProject("Save mesh!", name + ".mesh", "mesh",
                "Please enter a file name to save the mesh to!");
            if (path == string.Empty)
            {
                return;
            }
            AssetDatabase.CreateAsset(mesh, path);
            AssetDatabase.SaveAssets();
        }

        [ContextMenu("Copy Textures! o.O")]
        public void CopyTextures()
        {
            string path = EditorUtility.SaveFolderPanel("Save Textures!", "", "");
            foreach ( var item in textures)
            {
                if (File.Exists(item.mainTexturePath))
                    File.Copy(item.mainTexturePath, path + "/" + new FileInfo(item.mainTexturePath).Name);
                else if (item.mainTexturePath != string.Empty)
                    Debug.LogWarning("Im sorry t.t! I couldnt find the textures that were in the text field. Can you check that the path is correct please?" + Environment.NewLine + item.mainTexturePath);

                if (File.Exists(item.sideTexturePath))
                    File.Copy(item.sideTexturePath, path + "/" + new FileInfo(item.sideTexturePath).Name);
                else if (item.sideTexturePath != string.Empty)
                    Debug.LogWarning("Im sorry t.t! I couldnt find the textures that were in the text field. Can you check that the path is correct please?" + Environment.NewLine + item.sideTexturePath);
            }
        }

        [ContextMenu("Save Textures! o.O/DDS! (experimental)")]
        public void SaveTexturesDds()
        {
            string path = EditorUtility.SaveFolderPanel("Save Textures!", "", "");

            if (path == string.Empty)
            {
                return;
            }
            MeshRenderer mr = gameObject.GetComponent<MeshRenderer>();
            if (!mr)
            {
                return;
            }
            foreach (Material item in mr.sharedMaterials)
            {
                if (item.mainTexture is Texture2D tex)
                {
                    byte[] bytes = IO.ScnFileExporter.WriteTextureDXT(tex);


                    string name = path + "\\" + tex.name;
                    if (!name.EndsWith(".dds"))
                    {
                        name += ".dds";
                    }

                    File.WriteAllBytes(name, bytes);
                }

                if (item.GetTexture("_BumpMap") is Texture2D normal)
                {
                    byte[] bytes = IO.ScnFileExporter.WriteTextureDXT(normal);

                    string name = path + "\\" + normal.name;
                    if (!name.EndsWith(".dds"))
                    {
                        name += ".dds";
                    }

                    File.WriteAllBytes(name, bytes);
                }
                if (item.GetTexture("_DetailAlbedoMap") is Texture2D lightmap)
                {
                    byte[] bytes = IO.ScnFileExporter.WriteTextureDXT(lightmap);

                    string name = path + "\\" + lightmap.name;
                    if (!name.EndsWith(".dds"))
                    {
                        name += ".dds";
                    }

                    File.WriteAllBytes(name, bytes);
                }
            }
        }


        [ContextMenu("Save Textures! o.O/TGA!")]
        public void SaveTexturesTga()
        {
            string path = EditorUtility.SaveFolderPanel("Save Textures!", "", "");

            if (path == string.Empty)
            {
                return;
            }
            MeshRenderer mr = gameObject.GetComponent<MeshRenderer>();
            if (!mr)
            {
                return;
            }
            foreach (Material item in mr.sharedMaterials)
            {
                if (item.mainTexture is Texture2D tex)
                {
                    var t = new Texture2D(tex.width, tex.height);
                    t.SetPixels32(tex.GetPixels32(0));
                    byte[] bytes =  t.EncodeToTGA();


                    string name = path + "\\" + tex.name;
                    if (!name.EndsWith(".tga"))
                    {
                        name += ".tga";
                    }

                    File.WriteAllBytes(name, bytes);
                }

                if (item.GetTexture("_BumpMap") is Texture2D normal)
                {
                    var t = new Texture2D(normal.width, normal.height);
                    t.SetPixels32(normal.GetPixels32(0));
                    byte[] bytes = t.EncodeToTGA();

                    string name = path + "\\" + normal.name;
                    if (!name.EndsWith(".tga"))
                    {
                        name += ".tga";
                    }

                    File.WriteAllBytes(name, bytes);
                }
                if (item.GetTexture("_DetailAlbedoMap") is Texture2D lightmap)
                {
                    var t = new Texture2D(lightmap.width, lightmap.height);
                    t.SetPixels32(lightmap.GetPixels32(0));
                    byte[] bytes = t.EncodeToTGA();

                    string name = path + "\\" + lightmap.name;
                    if (!name.EndsWith(".tga"))
                    {
                        name += ".tga";
                    }

                    File.WriteAllBytes(name, bytes);
                }
            }
        }

        [ContextMenu("Remove Lightmap for Probuilder! U.u'")]
        public void RemoveLightmap()
        {
            MeshRenderer mr = GetComponent<MeshRenderer>();
            SkinnedMeshRenderer smr = GetComponent<SkinnedMeshRenderer>();
            Material[] materials;
            if (mr)
            {
                materials = mr.sharedMaterials;
            }
            else if (smr)
            {
                materials = smr.sharedMaterials;
            }
            else
            {
                Debug.LogWarning("Oh my! You have you have no Renderer! did you delete it? xP, or maybe this script was added by accident! Anyways, you cant delete lightmaps where there are none~~~!", gameObject);
                return;
            }
            foreach (Material mat in materials)
            {
                mat.SetTexture("_DetailAlbedoMap", null);
                mat.DisableKeyword("_DETAIL_MULX2");
            }
            foreach (var item in textures)
            {
                item.sideTexturePath = string.Empty;
            }
        }
        
        bool HasLightmap()
        {
            if(isNormal){
                return false;
            }
			if (ignoreLightmaps || ScnToolData.Instance.ignoreLightmapsGlobally)
			{
                return false;
			}
            var mr = GetComponent<MeshRenderer>();
			if (mr)
			{
                if( mr.lightmapIndex == -1){
                    foreach (var tex in textures)
                    {
                        return tex.sideTexturePath != string.Empty;
                    }
                }
            }
            return false;
        }

    }

    [Serializable]
    public class TextureItem
    {
        [HideInInspector] [SerializeField] string name;
        //[Path]
        public string mainTexturePath;
        //[Path]
        public string sideTexturePath;
        //public bool sideTextureIsNormal;

        //[Button("Select Main Tex O.O")] public ButtonAction selectMainTex;
        //[Button("Select Side Tex 0.0")] public ButtonAction selectSideTex;

        public TextureItem(string name, string _mainTexturePath, string _sideTexturePath)
        { 
            this.name = name;
            this.mainTexturePath = _mainTexturePath;
            this.sideTexturePath = _sideTexturePath;
            //this.sideTextureIsNormal = _sideTextureIsNormal;
            //selectMainTex = new ButtonAction(SelectMainTex);
            //selectSideTex = new ButtonAction(SelectSideTex);
        }

        public void SelectMainTex()
        {
			if (mainTexturePath != string.Empty)
			{
                mainTexturePath = EditorUtility.OpenFilePanel("Select Main Tex O.O", new FileInfo(mainTexturePath).DirectoryName, "");
            }
			else
			{
                var path = EditorUtility.OpenFilePanel("Select Main Tex O.O", "", "");
				if (path != string.Empty)
				{
                    mainTexturePath = path;
				}
            }
        }
        public void SelectSideTex()
        {
            if (sideTexturePath != string.Empty)
            {
                sideTexturePath = EditorUtility.OpenFilePanel("Select Side Tex 0.0", new FileInfo(sideTexturePath).DirectoryName, "");
            }
            else
            {
                var path = EditorUtility.OpenFilePanel("Select Side Tex 0.0", "", "");
                if (path != string.Empty)
                {
                    sideTexturePath = path;
                }
            }
        }
    }
}

[Flags]
public enum RenderFlag
{
    None = 0,
    NoLight = 1,
    Transparent = 2,
    Cutout = 4,

    NoCulling = 8,
    Billboard = 16,
    Flare = 32,
    ZWriteOff = 64,

    Shader = 128,
    NoPrerender = 256,
    NoFog = 512,
    Unknown = 1024,

    NoMipmap = 2048,
    VertexAnim = 4096,
    Shadow = 8192,
    Glow = 16384,

    Water = 32768,
    Distortion = 65536,
    Dark = 131072,
    Unk1 = 262144,

    Unk2 = 524288,
    Unk3 = 1048576
}
