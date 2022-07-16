using AevenScnTool.IO;
using NetsphereScnTool.Scene;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

[CustomEditor(typeof(PaperDoll))]
public class PaperDollEditor : Editor
{
    VisualElement Root;
    Label folder_text;
    ScrollView ClothesSelector;


    ScrollView headlist;
    ScrollView facelist;
    ScrollView shirtlist;
    ScrollView pantslist;
    ScrollView gloveslist;
    ScrollView bootslist;
    ScrollView accesorylist;
    ScrollView petlist;

    Dictionary<PaperDoll.Type, List<XmlNode>> unisex = new();
    Dictionary<PaperDoll.Type, List<XmlNode>> female = new();
    Dictionary<PaperDoll.Type, List<XmlNode>> male = new();

    ScnData sceneObj;
    PaperDoll paperdoll;

    string rootFolder;

    public override VisualElement CreateInspectorGUI()
    {
        // Each editor window contains a root VisualElement object
        var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/ScnToolByAeven/Editor/Window/CharacterLoader/CharacterLoader.uxml");
        VisualElement root = visualTree.Instantiate();

        root.style.height = 570;

        Root = root.Q("Root");
        folder_text = root.Q<Label>("Folder_Text");
        root.Q<Button>("Folder").clicked += SelectS4Folder;
        ClothesSelector = root.Q<ScrollView>("ClothesSelector");
        ClothesSelector.contentContainer.style.flexWrap = Wrap.Wrap;

        Button boyButton = root.Q<Button>("Boy");
        Button girlButton = root.Q<Button>("Girl");


        boyButton.clicked += () => {
            SpawnBiped(false);
            Root.style.left = new StyleLength(new Length(-100, LengthUnit.Percent));
        };
        girlButton.clicked += () => {
            SpawnBiped(true);
            Root.style.left = new StyleLength(new Length(-100, LengthUnit.Percent));//and also spawn stuff
        };

        Button backButton1 = root.Q<Button>("Back1");
        backButton1.clicked += () => { Root.style.left = new StyleLength(new Length(0, LengthUnit.Percent)); /*And also delete stuff*/ };

        Button backButton2 = root.Q<Button>("Back2");
        backButton2.clicked += () => { Root.style.left = new StyleLength(new Length(-100, LengthUnit.Percent)); /*And also delete stuff*/ };

        Button headSelection = root.Q<Button>("Head");
        Button faceSelection = root.Q<Button>("Face");
        Button shirtSelection = root.Q<Button>("Shirt");
        Button pantsSelection = root.Q<Button>("Pants");
        Button glovesSelection = root.Q<Button>("Gloves");
        Button bootsSelection = root.Q<Button>("Boots");
        Button accesorySelection = root.Q<Button>("Accesory");
        Button petSelection = root.Q<Button>("Pet");

        headlist = root.Q<ScrollView>("HeadList");
        facelist = root.Q<ScrollView>("FaceList");
        shirtlist = root.Q<ScrollView>("ShirtList");
        pantslist = root.Q<ScrollView>("PantsList");
        gloveslist = root.Q<ScrollView>("GlovesList");
        bootslist = root.Q<ScrollView>("BootsList");
        accesorylist = root.Q<ScrollView>("AccesoryList");
        petlist = root.Q<ScrollView>("PetList");

        headSelection.clicked += () => { SelectClothes(PaperDoll.Type.hair); };
        faceSelection.clicked += () => { SelectClothes(PaperDoll.Type.face); };
        shirtSelection.clicked += () => { SelectClothes(PaperDoll.Type.body); };
        pantsSelection.clicked += () => { SelectClothes(PaperDoll.Type.leg); };
        glovesSelection.clicked += () => { SelectClothes(PaperDoll.Type.hand); };
        bootsSelection.clicked += () => { SelectClothes(PaperDoll.Type.foot); };
        accesorySelection.clicked += () => { SelectClothes(PaperDoll.Type.acc); };
        petSelection.clicked += () => { SelectClothes(PaperDoll.Type.pet); };

        return root;
    }

    void SelectS4Folder()
    {
        string path = EditorUtility.OpenFolderPanel("Select the S4 Client Folder!", "", "");
        if (path == string.Empty)
        {
            return;
        }
        rootFolder = path;

        ReadFile();


        Root.style.display = DisplayStyle.Flex;


        folder_text.text = path;
    }

    void SelectClothes(PaperDoll.Type type)
    {
        ClothesSelector.Clear();
        List<XmlNode> items = new List<XmlNode>();
        items.AddRange(unisex[type]);
        items.AddRange(paperdoll.isGirl ? female[type] : male[type]);


        for (int i = 0; i < items.Count; i++)
        {
            XmlNode graphic = items[i].SelectSingleNode("./child::graphic");
            XmlNode icon_image = graphic.Attributes.GetNamedItem("icon_image");

            string file = string.Empty;
            if (icon_image != null)
            {
                file = icon_image.Value.Replace(".tga", ".dds");
            }
            ClothesSelector.Add(CreateIcon(file, () => { SelectClotheItem(type, graphic); }));
        }

        Root.style.left = new StyleLength(new Length(-200, LengthUnit.Percent));
    }

    VisualElement CreateIcon(string icon_name, Action action, int size = 80)
    {
        var item = new Button();
        if (icon_name == string.Empty)
        {
            item.style.backgroundColor = Color.gray;
        }
        else
        {
            if (File.Exists(rootFolder + @"\resources\image\costume\" + icon_name))
            {
                Texture2D tex = ScnFileImporter.ParseTextureDXT(File.ReadAllBytes(
                rootFolder + @"\resources\image\costume\" + icon_name));
                item.style.backgroundImage = tex;
            }
            else
            {
                item.style.backgroundColor = Color.gray;
            }
        }
        item.style.borderBottomColor = new StyleColor(Color.black);
        item.style.borderTopColor = new StyleColor(Color.black);
        item.style.borderLeftColor = new StyleColor(Color.black);
        item.style.borderRightColor = new StyleColor(Color.black);

        item.style.borderBottomWidth = 1;
        item.style.borderTopWidth = 1;
        item.style.borderLeftWidth = 1;
        item.style.borderRightWidth = 1;

        item.style.width = size;
        item.style.height = size;
        item.style.scale = new StyleScale(new Scale(new Vector3(-1, -1, 1)));

        item.clicked += action;
        return item;
    }

    void SelectClotheItem(PaperDoll.Type type, XmlNode graphicNode)
    {
        List<ScnData> parts = new();
        XmlNode to_part_scene_file = graphicNode.Attributes.GetNamedItem("to_part_scene_file");
        if (to_part_scene_file != null)
        {
            string path = rootFolder + $@"\resources\model\character\{type.ToString()}\{to_part_scene_file.Value}";
            ScnData obj = LoadModel(path);
            MergeBoneSystem(sceneObj, obj);
            SetBaseAnimation(obj);
            parts.Add(obj);
        }

        int index = 1;
        while (true)
        {
            XmlNode to_node_scene_file = graphicNode.Attributes.GetNamedItem("to_node_scene_file" + index);
            if (to_node_scene_file != null)
            {
                XmlNode to_node_parent_node = graphicNode.Attributes.GetNamedItem("to_node_parent_node" + index);
                XmlNode to_node_animation_part = graphicNode.Attributes.GetNamedItem("to_node_animation_part" + index);

                string path = rootFolder + $@"\resources\model\character\{type}\{to_node_scene_file.Value}";
                ScnData obj = LoadModel(path);
                parts.Add(obj);
                AttachBonesystem(sceneObj, obj, to_node_parent_node.Value);

                SetBaseAnimation(obj);
                index++;
            }
            else
            {
                break;
            }
        }

        XmlNode hiding_option = graphicNode.Attributes.GetNamedItem("hiding_option");


        XmlNode icon_image = graphicNode.Attributes.GetNamedItem("icon_image");

        string file = string.Empty;
        if (icon_image != null)
        {
            file = icon_image.Value.Replace(".tga", ".dds");
        }
        PaperDoll.Container cont = new PaperDoll.Container(file, parts);

        paperdoll.GetAttachedParts(type).Add(cont);

        RedrawItems(type);
    }

    void DeleteClotheItem(List<ScnData> item)
    {
        for (int i = 0; i < item.Count; i++)
        {
            DestroyImmediate(item[i]);
        }
    }

    void RedrawItems(PaperDoll.Type type)
    {
        List<PaperDoll.Container> list = paperdoll.GetAttachedParts(type);
        var scrollview = GetItemList(type);
        scrollview.Clear();
        for (int i = 0; i < list.Count; i++)
        {
            scrollview.Add(CreateIcon(list[i].iconName, () => { DeleteClotheItem(list[i].parts); }, 40));
        }
    }

    void ReadFile()
    {
        XmlDocument doc = new XmlDocument();
        doc.Load(rootFolder + "\\xml\\item.x7");

        unisex[PaperDoll.Type.hair] = new List<XmlNode>();
        unisex[PaperDoll.Type.face] = new List<XmlNode>();
        unisex[PaperDoll.Type.body] = new List<XmlNode>();
        unisex[PaperDoll.Type.leg] = new List<XmlNode>();
        unisex[PaperDoll.Type.hand] = new List<XmlNode>();
        unisex[PaperDoll.Type.foot] = new List<XmlNode>();
        unisex[PaperDoll.Type.acc] = new List<XmlNode>();
        unisex[PaperDoll.Type.pet] = new List<XmlNode>();
        unisex[PaperDoll.Type.NONE] = new List<XmlNode>();

        female[PaperDoll.Type.hair] = new List<XmlNode>();
        female[PaperDoll.Type.face] = new List<XmlNode>();
        female[PaperDoll.Type.body] = new List<XmlNode>();
        female[PaperDoll.Type.leg] = new List<XmlNode>();
        female[PaperDoll.Type.hand] = new List<XmlNode>();
        female[PaperDoll.Type.foot] = new List<XmlNode>();
        female[PaperDoll.Type.acc] = new List<XmlNode>();
        female[PaperDoll.Type.pet] = new List<XmlNode>();
        female[PaperDoll.Type.NONE] = new List<XmlNode>();

        male[PaperDoll.Type.hair] = new List<XmlNode>();
        male[PaperDoll.Type.face] = new List<XmlNode>();
        male[PaperDoll.Type.body] = new List<XmlNode>();
        male[PaperDoll.Type.leg] = new List<XmlNode>();
        male[PaperDoll.Type.hand] = new List<XmlNode>();
        male[PaperDoll.Type.foot] = new List<XmlNode>();
        male[PaperDoll.Type.acc] = new List<XmlNode>();
        male[PaperDoll.Type.pet] = new List<XmlNode>();
        male[PaperDoll.Type.NONE] = new List<XmlNode>();

        for (int i = 0; i < doc.DocumentElement.ChildNodes.Count; i++)
        {
            XmlNode graphic = doc.DocumentElement.ChildNodes[i].SelectSingleNode("./child::graphic");
            XmlNode baseNode = doc.DocumentElement.ChildNodes[i].SelectSingleNode("./child::base");
            if (graphic == null)
            {
                continue;
            }
            XmlNode node = graphic.Attributes.GetNamedItem("to_node_scene_file1");
            XmlNode part = graphic.Attributes.GetNamedItem("to_part_scene_file");

            if (node == null && part == null)
            {
                continue;
            }
            if (baseNode == null)
            {
                continue;
            }
            XmlNode genderNode = baseNode.Attributes.GetNamedItem("sex");
            if (genderNode == null)
            {
                continue;
            }

            PaperDoll.Type type = GetType(doc.DocumentElement.ChildNodes[i].Attributes.GetNamedItem("item_key").Value.Substring(0, 3));

            string gender = genderNode.Value;
            switch (gender)
            {
                case "unisex":
                    unisex[type].Add(doc.DocumentElement.ChildNodes[i]);
                    break;
                case "woman":
                    female[type].Add(doc.DocumentElement.ChildNodes[i]);
                    break;
                case "man":
                    male[type].Add(doc.DocumentElement.ChildNodes[i]);
                    break;
                default:
                    break;
            }
        }
    }

    PaperDoll.Type GetType(string id_string)
    {
        if (int.TryParse(id_string, out int id))
        {
            if (Enum.IsDefined(typeof(PaperDoll.Type), id))
            {
                return (PaperDoll.Type)id;
            }
        }
        return PaperDoll.Type.NONE;
    }

    ScrollView GetItemList(PaperDoll.Type type)
    {
        if (type == PaperDoll.Type.hair)
        {
            return headlist;
        }
        if (type == PaperDoll.Type.face)
        {
            return facelist;
        }
        if (type == PaperDoll.Type.body)
        {
            return shirtlist;
        }
        if (type == PaperDoll.Type.leg)
        {
            return pantslist;
        }
        if (type == PaperDoll.Type.hand)
        {
            return gloveslist;
        }
        if (type == PaperDoll.Type.foot)
        {
            return bootslist;
        }
        if (type == PaperDoll.Type.acc)
        {
            return accesorylist;
        }
        if (type == PaperDoll.Type.pet)
        {
            return petlist;
        }
        return null;
    }


    void SpawnBiped(bool isGirl)
    {
        sceneObj = LoadModel(rootFolder + $@"\resources\model\character\{(isGirl ? "female" : "male")}_bip.scn");
        paperdoll = sceneObj.gameObject.AddComponent<PaperDoll>();
        paperdoll.isGirl = isGirl;
        if (false)
        {
            //load the other bip with newer animations
            ScnData extraNimations = LoadModel(rootFolder + $@"\resources\model\character\bip_{(paperdoll.isGirl ? "female" : "male")}\{(paperdoll.isGirl ? "female" : "male")}_bip_0000.scn");
        }

        //Play Base Animation
        if (paperdoll.isGirl)
        {
            SelectClotheItem(PaperDoll.Type.hair, female[PaperDoll.Type.hair][0].SelectSingleNode("./child::graphic"));
            SelectClotheItem(PaperDoll.Type.body, female[PaperDoll.Type.body][0].SelectSingleNode("./child::graphic"));
            SelectClotheItem(PaperDoll.Type.leg, female[PaperDoll.Type.leg][2].SelectSingleNode("./child::graphic"));
            SelectClotheItem(PaperDoll.Type.face, female[PaperDoll.Type.face][0].SelectSingleNode("./child::graphic"));
            SelectClotheItem(PaperDoll.Type.hand, female[PaperDoll.Type.hand][0].SelectSingleNode("./child::graphic"));
            SelectClotheItem(PaperDoll.Type.foot, female[PaperDoll.Type.foot][0].SelectSingleNode("./child::graphic"));
        }
        else
        {

            SelectClotheItem(PaperDoll.Type.hair, male[PaperDoll.Type.hair][0].SelectSingleNode("./child::graphic"));
            SelectClotheItem(PaperDoll.Type.body, male[PaperDoll.Type.body][0].SelectSingleNode("./child::graphic"));
            SelectClotheItem(PaperDoll.Type.leg, male[PaperDoll.Type.leg][0].SelectSingleNode("./child::graphic"));
            SelectClotheItem(PaperDoll.Type.face, male[PaperDoll.Type.face][0].SelectSingleNode("./child::graphic"));
            SelectClotheItem(PaperDoll.Type.hand, male[PaperDoll.Type.hand][0].SelectSingleNode("./child::graphic"));
            SelectClotheItem(PaperDoll.Type.foot, male[PaperDoll.Type.foot][0].SelectSingleNode("./child::graphic"));
        }

        SetBaseAnimation(sceneObj);
    }

    ScnData LoadModel(string path)
    {
        SceneContainer container = SceneContainer.ReadFrom(path);
        var go = new GameObject(container.Header.Name);
        container.fileInfo = new FileInfo(path);
        ScnData sd = go.AddComponent<ScnData>();
        sd.folderPath = path;

        ScnFileImporter.BuildFromContainer(container, go);

        return sd;
    }

    void AttachBonesystem(ScnData attachTo, ScnData addon, string attachPoint)
    {
        addon.transform.SetParent(FindChild(attachTo.transform, attachPoint));
        addon.transform.localPosition = Vector3.zero;
        addon.transform.localRotation = Quaternion.identity;
        addon.transform.localScale = Vector3.one;
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

                smr.bones = newBones.ToArray();
            }
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

    void SetBaseAnimation(ScnData scn)
    {
        S4Animations[] parts = scn.GetComponentsInChildren<S4Animations>();
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
}
