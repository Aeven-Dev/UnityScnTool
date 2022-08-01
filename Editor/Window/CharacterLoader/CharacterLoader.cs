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
    ScrollView ItemSelector;


    VisualElement clothesTab;
    VisualElement weaponsTab;
    Button switchToClothesTab;
    Button switchToWeaponsTab;

    ScrollView hairlist;
    ScrollView facelist;
    ScrollView bodylist;
    ScrollView pantslist;
    ScrollView handslist;
    ScrollView feetlist;
    ScrollView accesorylist;
    ScrollView petlist;

    ScrollView meleelist;
    ScrollView rifflelist;
    ScrollView sniperlist;
    ScrollView throwablelist;
    ScrollView instalationlist;
    ScrollView mindlist;

    Dictionary<PaperDoll.Type, List<Item>> unisex = new();
    Dictionary<PaperDoll.Type, List<Item>> female = new();
    Dictionary<PaperDoll.Type, List<Item>> male = new();

    Dictionary<PaperDoll.Type, List<Item>> weapons = new();

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
        ItemSelector = root.Q<ScrollView>("ClothesSelector");
        ItemSelector.contentContainer.style.flexWrap = Wrap.Wrap;

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

        hairlist = root.Q<ScrollView>("HeadList");
        facelist = root.Q<ScrollView>("FaceList");
        bodylist = root.Q<ScrollView>("ShirtList");
        pantslist = root.Q<ScrollView>("PantsList");
        handslist = root.Q<ScrollView>("GlovesList");
        feetlist = root.Q<ScrollView>("BootsList");
        accesorylist = root.Q<ScrollView>("AccesoryList");
        petlist = root.Q<ScrollView>("PetList");

        meleelist = root.Q<ScrollView>("MeleeList");
        rifflelist = root.Q<ScrollView>("RiffleList");
        sniperlist = root.Q<ScrollView>("SniperList");
        instalationlist = root.Q<ScrollView>("InstalationList");
        throwablelist = root.Q<ScrollView>("ThrowableList");
        mindlist = root.Q<ScrollView>("MindList");

        headSelection.clicked += () => { SelectClothes(PaperDoll.Type.hair); };
        faceSelection.clicked += () => { SelectClothes(PaperDoll.Type.face); };
        shirtSelection.clicked += () => { SelectClothes(PaperDoll.Type.body); };
        pantsSelection.clicked += () => { SelectClothes(PaperDoll.Type.leg); };
        glovesSelection.clicked += () => { SelectClothes(PaperDoll.Type.hand); };
        bootsSelection.clicked += () => { SelectClothes(PaperDoll.Type.foot); };
        accesorySelection.clicked += () => { SelectClothes(PaperDoll.Type.acc); };
        petSelection.clicked += () => { SelectClothes(PaperDoll.Type.pet); };


        Button meleeSelection = root.Q<Button>("Melee");
        Button riffleSelection = root.Q<Button>("Riffle");
        Button sniperSelection = root.Q<Button>("Sniper");
        Button instalationSelection = root.Q<Button>("Instalation");
        Button throwableSelection = root.Q<Button>("Throwable");
        Button mindSelection = root.Q<Button>("Mind");

        meleeSelection.clicked += () => { SelectWeapons(PaperDoll.Type.melee); };
        riffleSelection.clicked += () => { SelectWeapons(PaperDoll.Type.riffle); };
        sniperSelection.clicked += () => { SelectWeapons(PaperDoll.Type.sniper); };
        instalationSelection.clicked += () => { SelectWeapons(PaperDoll.Type.instalation); };
        throwableSelection.clicked += () => { SelectWeapons(PaperDoll.Type.throwable); };
        mindSelection.clicked += () => { SelectWeapons(PaperDoll.Type.mind); };

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
        ItemSelector.Clear();
        List<Item> items = new();
        items.AddRange(unisex[type]);
        items.AddRange(paperdoll.isGirl ? female[type] : male[type]);


        for (int i = 0; i < items.Count; i++)
        {
            Item item = items[i];
            Texture2D tex = null;
            if (item.icon_image_name != null && item.icon_image_name != string.Empty)
            {
                var file = item.icon_image_name.Replace(".tga", ".dds");
                string path = rootFolder + $@"\resources\image\costume\{file}";
                if (File.Exists(path))
                {
                    tex = ScnFileImporter.ParseTextureDXT(File.ReadAllBytes(path));
                }
            }
            ItemSelector.Add(CreateIcon(tex, () => { SelectClotheItem(type, item); }));
        }

        Root.style.left = new StyleLength(new Length(-200, LengthUnit.Percent));
    }

    void SelectWeapons(PaperDoll.Type type)
	{
        if (rootFolder == string.Empty)
        {
            EditorUtility.DisplayDialog("Oooopsie!", "Haha, you have to select an s4 client folder first, silly!", "Alright! :)");
            return;
        }
        ItemSelector.Clear();

        var w = weapons[type];

        for (int i = 0; i < w.Count; i++)
        {
            var item = w[i];

            Texture2D tex = null;
            if (item.icon_image_name != null && item.icon_image_name != string.Empty)
            {
                var file = item.icon_image_name.Replace(".tga", ".dds");
                string path = rootFolder + $@"\resources\image\costume\{file}";
                if (File.Exists(path))
                {
                    tex = ScnFileImporter.ParseTextureDXT(File.ReadAllBytes(path));
                }
            }
            ItemSelector.Add(CreateIcon(tex, () => { SelectWeaponItem(type, item); }));
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

    void SelectClotheItem(PaperDoll.Type type, Item item)
    {

        paperdoll.SelectClotheItem(type, rootFolder, item.mainPart, item.nodes, item.hiding_option, item.icon_image_name);

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

    void SelectWeaponItem(PaperDoll.Type type, Item item)
	{

        paperdoll.SelectWeapon(type, rootFolder, item.nodes, item.icon_image_name);
	}

    bool ReadFile(string folder)
    {
        XmlDocument item_doc = new XmlDocument();
        if (!File.Exists(folder + "\\xml\\item.x7"))
        {
            return false;
        }
        item_doc.Load(folder + "\\xml\\item.x7");

        XmlDocument _eu_weapon_doc = new XmlDocument();
        if (!File.Exists(folder + "\\xml\\_eu_weapon.x7"))
        {
            return false;
        }
        _eu_weapon_doc.Load(folder + "\\xml\\_eu_weapon.x7");

        XmlDocument weapon_attach_doc = new XmlDocument();
        if (!File.Exists(folder + "\\xml\\weapon_attach.x7"))
        {
            return false;
        }
        weapon_attach_doc.Load(folder + "\\xml\\weapon_attach.x7");

        InitLists();

        for (int i = 0; i < item_doc.DocumentElement.ChildNodes.Count; i++)
        {
            var child = item_doc.DocumentElement.ChildNodes[i];
            var graphic = child.SelectSingleNode("./child::graphic");
            var baseNode = child.SelectSingleNode("./child::base");
            if (graphic == null)
            {
                continue;
            }
            var node = graphic.Attributes.GetNamedItem("to_node_scene_file1");
            var part = graphic.Attributes.GetNamedItem("to_part_scene_file");
            if (baseNode == null)
            {
                continue;
            }
            if (node == null && part == null)
            {
                //might be a weapon?
                string item_key = child.Attributes.GetNamedItem("item_key").Value;
                PaperDoll.Type type = PaperDoll.GetType(item_key.Substring(0, 3));

				if (weapons.ContainsKey(type))
                {
                    var weapon = _eu_weapon_doc.DocumentElement.SelectSingleNode($"./child::weapon[@item_key='{item_key}']");
                    var scene = weapon.SelectSingleNode("./child::scene");

                    List<string> values = new();
                    for (int w = 1; true; w++)
                    {
                        var val = scene.Attributes.GetNamedItem("value" + w);
						if (val != null)
						{
                            values.Add(val.Value);
						}
						else
						{
                            break;
						}
                    }

                    string t = weapon.SelectSingleNode("./child::ability").Attributes.GetNamedItem("type").Value;

                    List<(string,string,string)> nodes = new();
                    for (int a = 0; a < values.Count; a++)
					{
                        var thing = weapon_attach_doc.DocumentElement.SelectSingleNode($"./child::weapon[@type='{t}' and sceneIndex='{a}']'");
                        var aa = thing.Attributes.GetNamedItem("attackAttach").Value;
                        var ia = thing.Attributes.GetNamedItem("idleAttach").Value;

                        nodes.Add((values[a], aa, ia));
                    }
                    //Goodness gracious, there's a shortage of names!
                    var item = new Item()
                    {
                        item_key = item_key,
                        nodes = nodes.ToArray(),
                        icon_image_name = graphic.Attributes.GetNamedItem("icon_image")?.Value,
                    };
                    weapons[type].Add(item);
                }
            }
			else
			{
                var genderNode = baseNode.Attributes.GetNamedItem("sex");
                if (genderNode == null)
                {
                    continue;
                }

                PaperDoll.Type type = PaperDoll.GetType(child.Attributes.GetNamedItem("item_key").Value.Substring(0, 3));
                List<(string, string, string)> nodes = new();
				for (int p = 1; true; p++)
                {
                    var n = (graphic.Attributes.GetNamedItem("to_node_scene_file" + p),
                            graphic.Attributes.GetNamedItem("to_node_parent_node" + p),
                            graphic.Attributes.GetNamedItem("to_node_animation_part" + p));
					if (n != (null,null,null))
                        nodes.Add((n.Item1.Value, n.Item2.Value, n.Item3.Value));
					else
                        break;
                }

                var item = new Item() {
                    item_key = child.Attributes.GetNamedItem("item_key").Value,
                    mainPart = part.Value,
                    nodes = nodes.ToArray(),
                    hiding_option = graphic.Attributes.GetNamedItem("hiding_option")?.Value,
                    icon_image_name = graphic.Attributes.GetNamedItem("icon_image")?.Value,
                };
                string gender = genderNode.Value;
                switch (gender)
                {
                    case "unisex":
                        unisex[type].Add(item);
                        break;
                    case "woman":
                        female[type].Add(item);
                        break;
                    case "man":
                        male[type].Add(item);
                        break;
                    default:
                        break;
                }
            }
        }

        return true;
    }

    void InitLists()
	{
        unisex[PaperDoll.Type.hair] = new List<Item>();
        unisex[PaperDoll.Type.face] = new List<Item>();
        unisex[PaperDoll.Type.body] = new List<Item>();
        unisex[PaperDoll.Type.leg] = new List<Item>();
        unisex[PaperDoll.Type.hand] = new List<Item>();
        unisex[PaperDoll.Type.foot] = new List<Item>();
        unisex[PaperDoll.Type.acc] = new List<Item>();
        unisex[PaperDoll.Type.pet] = new List<Item>();
        unisex[PaperDoll.Type.NONE] = new List<Item>();

        female[PaperDoll.Type.hair] = new List<Item>();
        female[PaperDoll.Type.face] = new List<Item>();
        female[PaperDoll.Type.body] = new List<Item>();
        female[PaperDoll.Type.leg] = new List<Item>();
        female[PaperDoll.Type.hand] = new List<Item>();
        female[PaperDoll.Type.foot] = new List<Item>();
        female[PaperDoll.Type.acc] = new List<Item>();
        female[PaperDoll.Type.pet] = new List<Item>();
        female[PaperDoll.Type.NONE] = new List<Item>();

        male[PaperDoll.Type.hair] = new List<Item>();
        male[PaperDoll.Type.face] = new List<Item>();
        male[PaperDoll.Type.body] = new List<Item>();
        male[PaperDoll.Type.leg] = new List<Item>();
        male[PaperDoll.Type.hand] = new List<Item>();
        male[PaperDoll.Type.foot] = new List<Item>();
        male[PaperDoll.Type.acc] = new List<Item>();
        male[PaperDoll.Type.pet] = new List<Item>();
        male[PaperDoll.Type.NONE] = new List<Item>();

        weapons[PaperDoll.Type.melee] = new List<Item>();
        weapons[PaperDoll.Type.riffle] = new List<Item>();
        weapons[PaperDoll.Type.sniper] = new List<Item>();
        weapons[PaperDoll.Type.throwable] = new List<Item>();
        weapons[PaperDoll.Type.instalation] = new List<Item>();
        weapons[PaperDoll.Type.mind] = new List<Item>();
    }

    ScrollView GetItemList(PaperDoll.Type type)
    {
        if (type == PaperDoll.Type.hair)
        {
            return hairlist;
        }
        if (type == PaperDoll.Type.face)
        {
            return facelist;
        }
        if (type == PaperDoll.Type.body)
        {
            return bodylist;
        }
        if (type == PaperDoll.Type.leg)
        {
            return pantslist;
        }
        if (type == PaperDoll.Type.hand)
        {
            return handslist;
        }
        if (type == PaperDoll.Type.foot)
        {
            return feetlist;
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
            SelectClotheItem(PaperDoll.Type.hair, female[PaperDoll.Type.hair][0]);
            SelectClotheItem(PaperDoll.Type.body, female[PaperDoll.Type.body][0]);
            SelectClotheItem(PaperDoll.Type.leg, female[PaperDoll.Type.leg][2]);
            SelectClotheItem(PaperDoll.Type.face, female[PaperDoll.Type.face][0]);
            SelectClotheItem(PaperDoll.Type.hand, female[PaperDoll.Type.hand][0]);
            SelectClotheItem(PaperDoll.Type.foot, female[PaperDoll.Type.foot][0]);
        }
		else
		{

            SelectClotheItem(PaperDoll.Type.hair, male[PaperDoll.Type.hair][0]);
            SelectClotheItem(PaperDoll.Type.body, male[PaperDoll.Type.body][0]);
            SelectClotheItem(PaperDoll.Type.leg,  male[PaperDoll.Type.leg ][0]);
            SelectClotheItem(PaperDoll.Type.face, male[PaperDoll.Type.face][0]);
            SelectClotheItem(PaperDoll.Type.hand, male[PaperDoll.Type.hand][0]);
            SelectClotheItem(PaperDoll.Type.foot, male[PaperDoll.Type.foot][0]);
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

    void SwitchToClothesTab()
    {
        clothesTab.style.display =  DisplayStyle.Flex;
        weaponsTab.style.display = DisplayStyle.None;
        switchToClothesTab.SetEnabled(false);
        switchToWeaponsTab.SetEnabled(true);
    }
    void SwitchToWeaponsTab()
    {
        clothesTab.style.display = DisplayStyle.None;
        weaponsTab.style.display = DisplayStyle.Flex;
        switchToClothesTab.SetEnabled(true);
        switchToWeaponsTab.SetEnabled(false);
    }
}

struct Item
{
    public string item_key;
    public string mainPart;
    public (string, string, string)[] nodes;
    public string hiding_option;
    public string icon_image_name;
}