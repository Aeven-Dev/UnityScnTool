using AevenScnTool.IO;
using NetsphereScnTool.Scene;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class PaperDoll : MonoBehaviour
{
    public enum Type
    {
        hair = 100,
        face,
        body,
        leg,
        hand,
        foot,
        acc,
        pet,

        melee = 200,
        riffle,
        sniper,
        instalation,
        throwable,
        mind,

        NONE
    }


    public static Type GetType(string id_string)
    {
        if (int.TryParse(id_string, out int id))
        {
            if (Enum.IsDefined(typeof(Type), id))
            {
                return (Type)id;
            }
        }
        return Type.NONE;
    }


    [HideInInspector]public bool isGirl;

    [HideInInspector]public List<Container> attachedHair = new List<Container>();
    [HideInInspector]public List<Container> attachedFaces = new List<Container>();
    [HideInInspector]public List<Container> attachedBodies = new List<Container>();
    [HideInInspector]public List<Container> attachedLegs = new List<Container>();
    [HideInInspector]public List<Container> attachedHands = new List<Container>();
    [HideInInspector]public List<Container> attachedFeet = new List<Container>();
    [HideInInspector]public List<Container> attachedAccesories = new List<Container>();
    [HideInInspector] public List<Container> attachedPets = new List<Container>();

    [HideInInspector] public List<Container> attachedMelee = new List<Container>();
    [HideInInspector] public List<Container> attachedRiffle = new List<Container>();
    [HideInInspector] public List<Container> attachedSniper = new List<Container>();
    [HideInInspector] public List<Container> attachedInstalation = new List<Container>();
    [HideInInspector] public List<Container> attachedThrowable = new List<Container>();
    [HideInInspector] public List<Container> attachedMind = new List<Container>();

    public List<Container> GetAttachedParts(Type part)
	{
		return part switch
		{
			Type.hair           => attachedHair,
			Type.face           => attachedFaces,
			Type.body           => attachedBodies,
			Type.leg            => attachedLegs,
			Type.hand           => attachedHands,
			Type.foot           => attachedFeet,
			Type.acc            => attachedAccesories,
			Type.pet            => attachedPets,
			Type.melee          => attachedMelee,
			Type.riffle         => attachedRiffle,
			Type.sniper         => attachedSniper,
			Type.instalation    => attachedInstalation,
			Type.throwable      => attachedThrowable,
			Type.mind           => attachedMind,
			_                   => null
		};
	}

    public void SelectClotheItem(Type type, string rootFolder, string to_part_scene_file, (string,string,string)[] nodes, string hiding_option, string icon_image)
    {
        List<ScnData> parts = new();
        if (to_part_scene_file != null)
        {
            string path = rootFolder + $@"\resources\model\character\{type.ToString()}\{to_part_scene_file}";
            ScnData obj = ScnFileImporter.LoadModel(path);
            MergeBoneSystem(GetComponent<ScnData>(), obj);
            SetBaseAnimation();
            parts.Add(obj);
        }
        foreach (var node in nodes)
        {
            string to_node_scene_file = node.Item1;

            string to_node_parent_node = node.Item2;
            string to_node_animation_part = node.Item3;

            string path = rootFolder + $@"\resources\model\character\{type}\{to_node_scene_file}";
            ScnData obj = ScnFileImporter.LoadModel(path);
            parts.Add(obj);
            AttachBonesystem(GetComponent<ScnData>(), obj, to_node_parent_node);

            SetBaseAnimation();
        }

        Texture2D tex = null;
        if (icon_image != null && icon_image != string.Empty)
        {
            var file = icon_image.Replace(".tga", ".dds");
            string path = rootFolder + $@"\resources\image\costume\{file}";
			if (File.Exists(path))
            {
                tex = ScnFileImporter.ParseTextureDXT(File.ReadAllBytes(path));
            }
        }
        Container cont = new Container(tex, parts, type);

        GetAttachedParts(type).Add(cont);

    }

    public void SelectWeapon(Type type, string rootFolder, (string scnFile,string attackAttach,string idleAttach)[] values, string icon_image)
	{
        List<ScnData> parts = new(values.Length);
		for (int i = 0; i < values.Length; i++)
		{
            string path = rootFolder + $@"\resources\model\weapon\{values[i].scnFile}";
            ScnData obj = ScnFileImporter.LoadModel(path); 
            parts.Add(obj);
            AttachBonesystem(GetComponent<ScnData>(), obj, values[i].attackAttach);

            Texture2D tex = null;
            if (icon_image != null && icon_image != string.Empty)
            {
                var file = icon_image.Replace(".tga", ".dds");
                string p = rootFolder + $@"\resources\image\weapon\{file}";
                if (File.Exists(p))
                {
                    tex = ScnFileImporter.ParseTextureDXT(File.ReadAllBytes(p));
                }
            }

            Container cont = new(tex, parts, type);

            GetAttachedParts(type).Add(cont);
        }
    }

    public void DeleteItem(Container item)
    {
        for (int i = 0; i < item.parts.Count; i++)
        {
            DestroyImmediate(item.parts[i].gameObject);
        }
        GetAttachedParts(item.type).Remove(item);
    }

    void MergeBoneSystem(ScnData attachTo, ScnData addon)
    {
        //Getting the mesh to be skinned to the biped
        //I'll asume that under the addon Scn there's an object called BONESYSTEM
        foreach (Transform item in addon.transform.GetChild(0))
        {
            var smr = item.GetComponent<SkinnedMeshRenderer>();
            if (smr)
            {
                smr.rootBone = attachTo.transform.Find("Bip01");
                List<Transform> newBones = new List<Transform>();
                for (int i = 0; i < smr.bones.Length; i++)
                {
                    Transform b = FindChild(attachTo.transform, smr.bones[i].name);
                    
                    newBones.Add(b);
                }
                for (int i = 0; i < smr.bones.Length; i++)
                {
                    DestroyImmediate(smr.bones[i]);
                }

                smr.bones = newBones.ToArray();
            }
        }
    }

    void AttachBonesystem(ScnData attachTo, ScnData addon, string attachPoint)
    {
        addon.transform.SetParent(FindChild(attachTo.transform, attachPoint));
        addon.transform.localPosition = Vector3.zero;
        addon.transform.localRotation = Quaternion.identity;
        addon.transform.localScale = Vector3.one;
    }

    public void ClearPaperdoll()
	{
        foreach (var item in Enum.GetValues(typeof(Type)))
        {
            if ((Type)item == Type.NONE)
            {
                continue;
            }
            var parts = GetAttachedParts((Type)item);
			for (int i = 0; i < parts.Count; i++)
			{
                DeleteItem(parts[i]);
			}
        }
    }

    public void SetBaseAnimation()
    {
        S4Animations[] parts = GetComponentsInChildren<S4Animations>();
        foreach (var part in parts)
        {
            TransformKeyData tkd = null;

            for (int i = 0; i < part.animations.Count; i++)
            {
                if (string.Equals(part.animations[i].Name, "BASE", StringComparison.OrdinalIgnoreCase))
                {
                    tkd = part.animations[i].TransformKeyData;
                    break;
                }
            }

            if (tkd == null)
            {
                if (part.animations.Count > 0)
                {
                    tkd = part.animations[0].TransformKeyData;
                }
                else
                {
                    continue;
                }
            }
            part.transform.localPosition = tkd.SamplePosition(0);
            part.transform.localRotation = tkd.SampleRotation(0);
            part.transform.localScale = tkd.SampleScale(0);

        }
    }

    public Transform FindChild(Transform root, string name)
    {
        for (int i = 0; i < root.transform.childCount; i++)
        {
            Transform child = root.transform.GetChild(i);
            if (child.name == name)
            {
                return child;
            }
            child = FindChild(child, name);
            if (child)
            {
                return child;
            }
        }
        return null;
    }

    [System.Serializable]
    public struct Container
	{
        public Type type;
        public List<ScnData> parts;
        public Texture2D icon;

        public Container(Texture2D icon, Type type)
        {
            this.icon = icon;
            parts = new List<ScnData>();
            this.type = type;
        }
        public Container(Texture2D icon, List<ScnData> parts, Type type)
        {
            this.icon = icon;
            this.parts = parts;
            this.type = type;
        }
        public Container(Texture2D icon, ScnData part, Type type)
        {
            this.icon = icon;
            this.parts = new List<ScnData>();
            parts.Add(part);
            this.type = type;
        }
    }
}
