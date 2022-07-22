using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

[AddComponentMenu("S4 scn/Texture Reference")]
public class TextureReference : MonoBehaviour
{
    public RenderFlag renderFlags;

    public bool flipUvVertical = false;
    public bool flipUvHorizontal = false;

    public List<TextureItem> textures = new List<TextureItem>();

    public bool hasLightmap { get { return HasLightmap(); } }

    [ContextMenu("Load from material! <3")]
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
        Material base_mat = AssetDatabase.LoadAssetAtPath<Material>(ScnToolData.RootPath + "Editor/Materials/S4_Base_Mat.mat");
        for (int i = 0; i < mats.Length; i++)
        {
			if (mats[i] == base_mat)
			{
                continue;
			}
            Texture mainTexture = mats[i].mainTexture;
            string mainTex = UnityEditor.AssetDatabase.GetAssetPath(mainTexture);
            Texture lightmap = mats[i].GetTexture("_DetailAlbedoMap");
            Texture normal = mats[i].GetTexture("_BumpMap");
            string sideTex; bool nor = false;
            if (lightmap)
            {
                sideTex = AssetDatabase.GetAssetPath(lightmap);
            }
            else if (normal)
            {
                sideTex = AssetDatabase.GetAssetPath(normal);
                nor = true;
            }
            else
            {
                sideTex = String.Empty;
            }
            string n = (mainTexture != null)? mainTexture.name : "EmptyTexture";
            textures.Add(new TextureItem(n, mainTex, sideTex,nor));
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
		base_mat = AssetDatabase.LoadAssetAtPath<Material>(ScnToolData.RootPath + "Editor/Materials/S4_Base_Mat.mat");

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
                    mainTexture = LoadTextureDXT(File.ReadAllBytes(item.mainTexturePath));
                    mainTexture.name = new FileInfo(item.mainTexturePath).Name;
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
                    sideTexture = LoadTextureDXT(File.ReadAllBytes(item.sideTexturePath));
                    sideTexture.name = new FileInfo(item.sideTexturePath).Name;
                }
            }

            mat.mainTexture = mainTexture;
			if (item.normal)
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

    [ContextMenu("Save Textures! o.O")]
    public void SaveTextures()
    {
        string path = EditorUtility.SaveFolderPanel("Save Textures!", "", "");
		foreach (TextureItem item in textures)
        {
            if (File.Exists(item.mainTexturePath))
                File.Copy(item.mainTexturePath, path + "/" + new FileInfo(item.mainTexturePath).Name);
            else if (item.sideTexturePath != string.Empty)
                Debug.LogWarning("Im sorry t.t! I couldnt find the textures that were in the text field. Can you check that the path is correct please?" + Environment.NewLine + item.mainTexturePath);

            if (File.Exists(item.sideTexturePath))
                File.Copy(item.sideTexturePath, path + "/" + new FileInfo(item.sideTexturePath).Name);
            else if(item.sideTexturePath != string.Empty)
                Debug.LogWarning("Im sorry t.t! I couldnt find the textures that were in the text field. Can you check that the path is correct please?" + Environment.NewLine + item.sideTexturePath);
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
            materials = mr.materials;
        }
        else if (smr)
        {
            materials = smr.materials;
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

    public static Texture2D LoadTextureDXT(byte[] ddsBytes)
    {
        byte a = ddsBytes[84];
        byte b = ddsBytes[85];
        byte c = ddsBytes[86];
        byte d = ddsBytes[87];

        string format = System.Text.Encoding.ASCII.GetString(new byte[] { a, b, c, d });
        //Debug.Log(format);
        TextureFormat textureFormat = TextureFormat.DXT1;

        if (format == "DXT3" || format == "DXT5")
        {
            textureFormat = TextureFormat.DXT5;
        }
        byte ddsSizeCheck = ddsBytes[4];
        if (ddsSizeCheck != 124)
            throw new Exception("Invalid DDS DXTn texture. Unable to read. Oh no, looks like the file wasnt a dds, or maybe it's corrupted, check it out!");  //this header byte should be 124 for DDS image files

        int height = ddsBytes[13] * 256 + ddsBytes[12];
        int width = ddsBytes[17] * 256 + ddsBytes[16];

        int DDS_HEADER_SIZE = 128;
        byte[] dxtBytes = new byte[ddsBytes.Length - DDS_HEADER_SIZE];
        Buffer.BlockCopy(ddsBytes, DDS_HEADER_SIZE, dxtBytes, 0, ddsBytes.Length - DDS_HEADER_SIZE);

        Texture2D texture = new Texture2D(width, height, textureFormat, false);
        texture.LoadRawTextureData(dxtBytes);
        texture.Apply();

        return (texture);
    }

    bool HasLightmap()
	{
		foreach (var item in textures)
		{
			if (item.sideTexturePath != string.Empty)
			{
                return true;
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
    public bool normal;
    public TextureItem(string name, string mainTexturePath, string sideTexturePath, bool normal) { this.name = name; this.mainTexturePath = mainTexturePath; this.sideTexturePath = sideTexturePath; this.normal = normal; }
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
