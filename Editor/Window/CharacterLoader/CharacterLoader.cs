using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using System.Xml;
using System.Collections.Generic;
using System.IO;
using NetsphereScnTool.Scene;
using AevenScnTool.IO;
using System;

public class CharacterLoader : EditorWindow
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

    PaperDoll paperdoll;

    string rootFolder = string.Empty;

    [MenuItem("Window/S4 Scn/CharacterLoader")]
    public static void Open()
    {
        CharacterLoader wnd = GetWindow<CharacterLoader>();
        wnd.titleContent = new GUIContent("CharacterLoader");
    }

    public static CharacterLoader OpenPaperdoll(PaperDoll pd)
	{
        CharacterLoader wnd = GetWindow<CharacterLoader>();
        wnd.titleContent = new GUIContent("CharacterLoader");

        wnd.paperdoll = pd;
        wnd.GoToClotheTypeSelection();
        wnd.Root.style.display = DisplayStyle.Flex;
        wnd.RedrawAllItems();
        
        return wnd;
    }

    void AttachToPaperDoll()
	{
        PaperDoll pd = Selection.activeGameObject?.GetComponent<PaperDoll>();

        if (pd)
		{
            paperdoll = pd;
            RedrawAllItems();
		}
	}

    void CreatePaperDoll()
	{

	}

    public void CreateGUI()
    {
        // Each editor window contains a root VisualElement object
        VisualElement root = rootVisualElement;
        var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(AevenScnTool.IO.ScnFileImporter.RootPath + "Editor/Window/CharacterLoader/CharacterLoader.uxml");
        VisualElement labelFromUXML = visualTree.Instantiate();
        root.Add(labelFromUXML);

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
        backButton1.clicked += GoToGenderSelection;

        Button backButton2 = root.Q<Button>("Back2");
        backButton2.clicked += GoToClotheTypeSelection;

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

    }

    void GoToGenderSelection()
	{
        Root.style.left = new StyleLength(new Length(0, LengthUnit.Percent));
    }
    void GoToClotheTypeSelection()
	{
        Root.style.left = new StyleLength(new Length(-100, LengthUnit.Percent));
    }
    void SelectS4Folder()
	{
        string path = EditorUtility.OpenFolderPanel("Select the S4 Client Folder!", "", "");
        if (path == string.Empty)
        {
            return;
        }

		if (!ReadFile(path))
		{
            EditorUtility.DisplayDialog("Goodness!", "I couldnt find a item.x7 in a xml folder in that S4 client! Maybe the file is missing or it could be that you extracted the resources diferently!", "D:");
            return;
        }

        Root.style.display = DisplayStyle.Flex;
        folder_text.text = path;
        rootFolder = path;
    }

    void SelectClothes(PaperDoll.Type type)
    {
		if (rootFolder == string.Empty)
		{
            EditorUtility.DisplayDialog("Oooopsie!", "Haha, you have to select an s4 client folder first, silly!", "Alright! :)");
            return;
        }
        ClothesSelector.Clear();
        List<XmlNode> items = new List<XmlNode>();
        items.AddRange(unisex[type]);
        items.AddRange(paperdoll.isGirl ? female[type] : male[type]);


        for (int i = 0; i < items.Count; i++)
        {
            XmlNode graphic = items[i].SelectSingleNode("./child::graphic");
            XmlNode icon_image = graphic.Attributes.GetNamedItem("icon_image");

            Texture2D tex = null;
            if (icon_image != null && icon_image != null)
            {
                var file = icon_image.Value.Replace(".tga", ".dds");
                string path = rootFolder + $@"\resources\image\costume\{file}";
                if (File.Exists(path))
                {
                    tex = ScnFileImporter.ParseTextureDXT(File.ReadAllBytes(path));
                }
            }
            ClothesSelector.Add(CreateIcon(tex, () => { SelectClotheItem(type, graphic); }));
        }

        Root.style.left = new StyleLength(new Length(-200, LengthUnit.Percent));
    }

    VisualElement CreateIcon(Texture2D icon, Action action, int size = 80)
    {
        var item = new Button();
        if (icon == null)
        {
            item.style.backgroundColor = Color.gray;
        }
        else
        {
            item.style.backgroundImage = icon;
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
        XmlNode to_part_scene_file = graphicNode.Attributes.GetNamedItem("to_part_scene_file");

        List<(string, string, string)> nodes = new();
        int index = 1;
        while (true)
        {
            XmlNode to_node_scene_file = graphicNode.Attributes.GetNamedItem("to_node_scene_file" + index);
            if (to_node_scene_file != null)
            {
                XmlNode to_node_parent_node = graphicNode.Attributes.GetNamedItem("to_node_parent_node" + index);
                XmlNode to_node_animation_part = graphicNode.Attributes.GetNamedItem("to_node_animation_part" + index);

                nodes.Add((to_node_scene_file.Value, to_node_parent_node.Value, to_node_animation_part.Value));
                index++;
            }
            else
            {
                break;
            }
        }
        XmlNode hiding_option = graphicNode.Attributes.GetNamedItem("hiding_option");
        XmlNode icon_image = graphicNode.Attributes.GetNamedItem("icon_image");

        paperdoll.SelectClotheItem(type, rootFolder, to_part_scene_file?.Value, nodes.ToArray(), hiding_option?.Value,icon_image?.Value);

        RedrawItems(type);
    }

    void DeleteClotheItem(PaperDoll.Container item)
    {
        paperdoll.DeleteClotheItem(item);
        RedrawItems(item.type);
    }

    void RedrawAllItems()
	{
        foreach (var item in Enum.GetValues(typeof(PaperDoll.Type)))
        {
            if ((PaperDoll.Type)item == PaperDoll.Type.NONE)
            {
                continue;
            }
            RedrawItems((PaperDoll.Type)item);
        }
    }

    void RedrawItems(PaperDoll.Type type)
    {
        List<PaperDoll.Container> list = paperdoll.GetAttachedParts(type);
        var scrollview = GetItemList(type);
        scrollview.Clear();
        for (int i = 0; i < list.Count; i++)
        {
            var parts = list[i];
            scrollview.Add(CreateIcon(list[i].icon, () => { DeleteClotheItem(parts); }, 40));
        }
    }

    bool ReadFile(string folder)
    {
        XmlDocument doc = new XmlDocument();
		if (!File.Exists(folder + "\\xml\\item.x7"))
		{
            return false;
		}
        doc.Load(folder + "\\xml\\item.x7");

        unisex[PaperDoll.Type.hair] = new List<XmlNode>();
        unisex[PaperDoll.Type.face]= new List<XmlNode>();
        unisex[PaperDoll.Type.body]= new List<XmlNode>();
        unisex[PaperDoll.Type.leg]=new List<XmlNode>();
        unisex[PaperDoll.Type.hand]= new List<XmlNode>();
        unisex[PaperDoll.Type.foot]= new List<XmlNode>();
        unisex[PaperDoll.Type.acc]=new List<XmlNode>();
        unisex[PaperDoll.Type.pet]=new List<XmlNode>();
        unisex[PaperDoll.Type.NONE]= new List<XmlNode>();

        female[PaperDoll.Type.hair] = new List<XmlNode>();
        female[PaperDoll.Type.face]= new List<XmlNode>();
        female[PaperDoll.Type.body]= new List<XmlNode>();
        female[PaperDoll.Type.leg]=new List<XmlNode>();
        female[PaperDoll.Type.hand]= new List<XmlNode>();
        female[PaperDoll.Type.foot]= new List<XmlNode>();
        female[PaperDoll.Type.acc]=new List<XmlNode>();
        female[PaperDoll.Type.pet]=new List<XmlNode>();
        female[PaperDoll.Type.NONE] = new List<XmlNode>();

        male[PaperDoll.Type.hair] = new List<XmlNode>();
        male[PaperDoll.Type.face]= new List<XmlNode>();
        male[PaperDoll.Type.body]= new List<XmlNode>();
        male[PaperDoll.Type.leg]=new List<XmlNode>();
        male[PaperDoll.Type.hand]= new List<XmlNode>();
        male[PaperDoll.Type.foot]= new List<XmlNode>();
        male[PaperDoll.Type.acc]=new List<XmlNode>();
        male[PaperDoll.Type.pet]=new List<XmlNode>();
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

            PaperDoll.Type type = PaperDoll.GetType(doc.DocumentElement.ChildNodes[i].Attributes.GetNamedItem("item_key").Value.Substring(0,3));

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

        return true;
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
		if ( paperdoll != null && paperdoll.isGirl == isGirl)
		{
            paperdoll.ClearPaperdoll();
		}
		else
        {
            ClearBiped();

            var sceneObj = ScnFileImporter.LoadModel(rootFolder + $@"\resources\model\character\{(isGirl ? "female" : "male")}_bip.scn");
            paperdoll = sceneObj.gameObject.AddComponent<PaperDoll>();
            paperdoll.isGirl = isGirl;
            if (false)
            {
                //load the other bip with newer animations
                ScnData extrAnimations = ScnFileImporter.LoadModel(rootFolder + $@"\resources\model\character\bip_{(paperdoll.isGirl ? "female" : "male")}\{(paperdoll.isGirl ? "female" : "male")}_bip_0000.scn");
            }
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

        paperdoll.SetBaseAnimation();
    }

    void ClearBiped()
	{
		if (paperdoll != null)
		{
            paperdoll.ClearPaperdoll();
            DestroyImmediate(paperdoll.gameObject);

            paperdoll = null;
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
}