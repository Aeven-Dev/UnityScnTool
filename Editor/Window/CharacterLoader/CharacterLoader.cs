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
    ScrollView legslist;
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

    ItemFile item_file;
    WeaponFile weapon_file;
    WeaponAttachFile weapon_attach_file;

    string s4_folder = "";
    string S4_Folder
    {
        get { return s4_folder; }
        set {
            s4_folder = value;
            if (paperdoll != null)
            {
                paperdoll.path = value;
            }
        }
    }

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
		if (pd.path!= null && pd.path != string.Empty)
		{
            wnd.SetS4Folder(pd.path);


        }
        wnd.folder_text.text = pd.path;
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
        var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(AevenScnTool.ScnToolData.RootPath + "Editor/Window/CharacterLoader/CharacterLoader.uxml");
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
            if (!CheckS4Folder())
            {
                return;
            }
            SpawnBiped(false);
            Root.style.left = new StyleLength(new Length(-100, LengthUnit.Percent));
        };
        girlButton.clicked += () => {
            if (!CheckS4Folder())
            {
                return;
            }
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
        legslist = root.Q<ScrollView>("PantsList");
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

        switchToClothesTab = root.Q<Button>("SwitchToClothesTab");
        switchToWeaponsTab = root.Q<Button>("SwitchToWeaponsTab");
        switchToClothesTab.SetEnabled(false);
        switchToClothesTab.style.backgroundColor = Color.HSVToRGB(0, 0, 0.3f);

        switchToClothesTab.clicked += SwitchToClothesTab;
        switchToWeaponsTab.clicked += SwitchToWeaponsTab;

        clothesTab = root.Q("ClothesTab");
        weaponsTab = root.Q("WeaponsTab");

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
        SetS4Folder( path);


    }
    void SetS4Folder(string path)
	{
        var res = ReadFiles(path);

        if (res != 0)
        {
            var message = "";
            switch (res)
            {
                case 1:
                    message = path + "\\xml\\item.x7 cant be found!";
                    break;
                case 2:
                    message = path + "\\xml\\_eu_weapon.x7 cant be found!";
                    break;
                case 3:
                    message = path + "\\xml\\weapon_attach.x7 cant be found!";
                    break;
            }
            EditorUtility.DisplayDialog("Goodness!", "I couldnt find a item.x7 in a xml folder in that S4 client! Maybe the file is missing or it could be that you extracted the resources diferently!\n" + message, "D:");
            return;
        }
        Root.style.display = DisplayStyle.Flex;
        folder_text.text = path;
        S4_Folder = path;
    }
    bool CheckS4Folder()
	{
        if (S4_Folder == string.Empty || paperdoll == null || paperdoll.path == null || paperdoll.path == string.Empty)
        {
            if (EditorUtility.DisplayDialog("Oooopsie!", "Haha, you have to select an s4 client folder first, silly!", "Alright, let me choose now! :)", "Oppsie! x3"))
            {
                SelectS4Folder();
            }
            return S4_Folder == string.Empty || paperdoll == null || paperdoll.path == null || paperdoll.path == string.Empty;
        }
        return true;
    }

    void SelectClothes(PaperDoll.Type type)
    {
        if (!CheckS4Folder())
        {
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
            if (item.graphic.icon_image != null && item.graphic.icon_image != string.Empty)
            {
                var file = item.graphic.icon_image.Replace(".tga", ".dds");
                string path = paperdoll.path + $@"\resources\image\costume\{file}";
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
        if (!CheckS4Folder())
        {
            return;
        }
        ItemSelector.Clear();

        var w = weapons[type];

        for (int i = 0; i < w.Count; i++)
        {
            var item = w[i];

            Texture2D tex = null;
            if (item.graphic.icon_image != null && item.graphic.icon_image != string.Empty)
            {
                var file = item.graphic.icon_image.Replace(".tga", ".dds");
                string path = paperdoll.path + $@"\resources\image\costume\{file}";
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

        paperdoll.SelectClotheItem(type, paperdoll.path, item.graphic.to_part_scene_file, item.graphic.nodes, item.graphic.hiding_option, item.graphic.icon_image);

        RedrawItems(type);
    }

    void SelectWeaponItem(PaperDoll.Type type, Item item)
	{
        var weap = weapon_file.GetWeapon(item.item_key);
        var attach = weapon_attach_file.GetWeaponAttach(weap.ability.type);
        (string, string, string)[] values = new (string, string, string)[weap.scene.values.Length];
		for (int i = 0; i < weap.scene.values.Length; i++)
		{
            var a = attach.Find(x => x.sceneIndex == i.ToString());
            values[i] = (weap.scene.values[i], a.attackAttach, a.idleAttach);
		}
        paperdoll.SelectWeapon(type, paperdoll.path, values, item.graphic.icon_image);
	}


    void DeleteItem(PaperDoll.Container item)
    {
        paperdoll.DeleteItem(item);
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
            scrollview.Add(CreateIcon(list[i].icon, () => { DeleteItem(parts); }, 40));
        }
    }



    int ReadFiles(string folder)
    {
        XmlDocument item_doc = new XmlDocument();
        if (!File.Exists(folder + "\\xml\\item.x7"))
        {
            return 1;
        }
        item_doc.Load(folder + "\\xml\\item.x7");

        XmlDocument _eu_weapon_doc = new XmlDocument();
        if (!File.Exists(folder + "\\xml\\_eu_weapon.x7"))
        {
            return 2;
        }
        _eu_weapon_doc.Load(folder + "\\xml\\_eu_weapon.x7");

        XmlDocument weapon_attach_doc = new XmlDocument();
        if (!File.Exists(folder + "\\xml\\weapon_attach.x7"))
        {
            return 3;
        }
        weapon_attach_doc.Load(folder + "\\xml\\weapon_attach.x7");

        InitLists();

        item_file = new() { items = new() };
        for (int i = 0; i < item_doc.DocumentElement.ChildNodes.Count; i++)
        {
            var child = item_doc.DocumentElement.ChildNodes[i];
            var it = new Item(){item_key = child.Attributes.GetNamedItem("item_key")?.Value};

            var graphic = child.SelectSingleNode("./child::graphic");
            if (graphic != null)
            {
                List<(string, string, string)> nodes = new();
                for (int p = 1; true; p++)
                {
                    var s = graphic.Attributes.GetNamedItem("to_node_scene_file" + p);
                    if (s == null) break;

                    var pa = graphic.Attributes.GetNamedItem("to_node_parent_node" + p);
                    if (pa == null) break;

                    var a = graphic.Attributes.GetNamedItem("to_node_animation_part" + p);
                    if (a == null) break;

                    nodes.Add((s.Value, pa.Value, a.Value));
                }

                it.graphic = new Item.Graphic()
                {
                    to_part_scene_file = graphic.Attributes.GetNamedItem("to_part_scene_file")?.Value,
                    nodes = nodes.ToArray(),
                    icon_image = graphic.Attributes.GetNamedItem("icon_image")?.Value,
                    hiding_option = graphic.Attributes.GetNamedItem("hiding_option")?.Value
                };
            }
                
            var baseNode = child.SelectSingleNode("./child::base");
            if (baseNode != null)
            {
                it.@base = new Item.Base()
                {
                    name = baseNode.Attributes.GetNamedItem("name")?.Value,
                    name_key = baseNode.Attributes.GetNamedItem("name_key")?.Value,
                    atribute_comment_key = baseNode.Attributes.GetNamedItem("atribute_comment_key")?.Value,
                    feature_comment_key = baseNode.Attributes.GetNamedItem("feature_comment_key")?.Value,
                    sex = baseNode.Attributes.GetNamedItem("sex")?.Value,

                };
            }
            item_file.items.Add(it);
        }

        weapon_file = new() { weapons = new()};
        for (int i = 0; i < _eu_weapon_doc.DocumentElement.ChildNodes.Count; i++)
        {
            var child = _eu_weapon_doc.DocumentElement.ChildNodes[i];
            var weapon = new Weapon() { item_key = child.Attributes.GetNamedItem("item_key")?.Value };

            var ability = child.SelectSingleNode("./child::ability");
            if (ability != null)
            {
                weapon.ability = new()
                {
                    type = ability.Attributes.GetNamedItem("type")?.Value,
                    category = ability.Attributes.GetNamedItem("category")?.Value,
                    rate_of_fire = ability.Attributes.GetNamedItem("rate_of_fire")?.Value,
                    power = ability.Attributes.GetNamedItem("power")?.Value,
                    move_speed_rate = ability.Attributes.GetNamedItem("move_speed_rate")?.Value,
                    attack_move_speed_rate = ability.Attributes.GetNamedItem("attack_move_speed_rate")?.Value,
                    magazine_capacity = ability.Attributes.GetNamedItem("magazine_capacity")?.Value,
                    cracked_magazine_capacity = ability.Attributes.GetNamedItem("cracked_magazine_capacity")?.Value,
                    max_ammo = ability.Attributes.GetNamedItem("max_ammo")?.Value,
                    max_ammo_limit = ability.Attributes.GetNamedItem("max_ammo_limit")?.Value,
                    accuracy = ability.Attributes.GetNamedItem("accuracy")?.Value,
                    range = ability.Attributes.GetNamedItem("range")?.Value,
                };
            }
            var scene = child.SelectSingleNode("./child::scene");
            if (scene != null)
            {
                List<string> values = new();
                for (int p = 1; true; p++)
                {
                    var s = scene.Attributes.GetNamedItem("value" + p);
                    if (s == null) break;

                    values.Add(s.Value);
                }
                weapon.scene = new() { values = values.ToArray() };
            }
            var resources = child.SelectSingleNode("./child::resources");
            if (resources != null)
            {
                weapon.resources = new()
                {
                    crosshair_file = resources.Attributes.GetNamedItem("crosshair_file")?.Value,
                    crosshair_zoomin_file = resources.Attributes.GetNamedItem("crosshair_zoomin_file")?.Value,
                    reload_sound_file = resources.Attributes.GetNamedItem("reload_sound_file")?.Value,
                    slot_image_file = resources.Attributes.GetNamedItem("slot_image_file")?.Value
                };
            }
            weapon_file.weapons.Add(weapon);
        }

        weapon_attach_file = new() { weapon_attachments = new()};
        for (int i = 0; i < weapon_attach_doc.DocumentElement.ChildNodes.Count; i++)
        {
            var child = weapon_attach_doc.DocumentElement.ChildNodes[i];
            var weapon_attach = new WeaponAttach()
            {
                type = child.Attributes.GetNamedItem("type")?.Value,
                sceneIndex = child.Attributes.GetNamedItem("sceneIndex")?.Value,
                attackAttach = child.Attributes.GetNamedItem("attackAttach")?.Value,
                link = child.Attributes.GetNamedItem("link")?.Value,
                idleAttach = child.Attributes.GetNamedItem("idleAttach")?.Value
            };

            weapon_attach_file.weapon_attachments.Add(weapon_attach);
        }

		foreach (var item in item_file.items)
		{
            PaperDoll.Type type = PaperDoll.GetType(item.item_key.Substring(0, 3));
			if (type == PaperDoll.Type.NONE)
			{
                continue;
			}
            if (item.item_key.StartsWith("2"))
            {
                if (weapons.ContainsKey(type))
                {
                    weapons[type].Add(item);
                }
            }
            else
            {
                switch (item.@base.sex)
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
                        Debug.Log("Haha");
                        break;
                }
            }
        }

        return 0;
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
        return type switch
        {
            PaperDoll.Type.hair => hairlist,
            PaperDoll.Type.face => facelist,
            PaperDoll.Type.body => bodylist,
            PaperDoll.Type.leg => legslist,
            PaperDoll.Type.hand => handslist,
            PaperDoll.Type.foot => feetlist,
            PaperDoll.Type.acc => accesorylist,
            PaperDoll.Type.pet => petlist,
            PaperDoll.Type.melee => meleelist,
            PaperDoll.Type.riffle => rifflelist,
            PaperDoll.Type.sniper => sniperlist,
            PaperDoll.Type.instalation => instalationlist,
            PaperDoll.Type.throwable => throwablelist,
            PaperDoll.Type.mind => mindlist,
            _ => null
        };
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

            ScnData sceneObj = ScnFileImporter.LoadModel(S4_Folder + $@"\resources\model\character\{(isGirl ? "female" : "male")}_bip.scn");
            paperdoll = sceneObj.gameObject.AddComponent<PaperDoll>();
            paperdoll.isGirl = isGirl;
            paperdoll.path = S4_Folder;
            if (false)
            {
                //load the other bip with newer animations
                ScnData extrAnimations = ScnFileImporter.LoadModel(S4_Folder + $@"\resources\model\character\bip_{(paperdoll.isGirl ? "female" : "male")}\{(paperdoll.isGirl ? "female" : "male")}_bip_0000.scn");
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

        switchToWeaponsTab.style.marginTop = 10;
        switchToClothesTab.style.marginTop = 0;

        switchToClothesTab.style.backgroundColor = Color.HSVToRGB(0,0, 0.3f);
        switchToWeaponsTab.style.backgroundColor = Color.HSVToRGB(0, 0, 0.23f);
        switchToWeaponsTab.style.borderBottomWidth = 1;
        switchToClothesTab.style.borderBottomWidth = 0;
    }
    void SwitchToWeaponsTab()
    {
        clothesTab.style.display = DisplayStyle.None;
        weaponsTab.style.display = DisplayStyle.Flex;
        switchToClothesTab.SetEnabled(true);
        switchToWeaponsTab.SetEnabled(false);

        switchToWeaponsTab.style.marginTop = 0;
        switchToClothesTab.style.marginTop = 10;

        switchToClothesTab.style.backgroundColor = Color.HSVToRGB(0, 0, 0.23f);
        switchToWeaponsTab.style.backgroundColor = Color.HSVToRGB(0, 0, 0.3f);

        switchToWeaponsTab.style.borderBottomWidth = 0;
        switchToClothesTab.style.borderBottomWidth = 1;
    }
}

class ItemFile
{
    public List<Item> items;

}

class WeaponFile
{
    public List<Weapon> weapons;

    public Weapon GetWeapon(string item_key)
	{
        return weapons.Find(x => x.item_key == item_key);
	}
}

class WeaponAttachFile
{
    public List<WeaponAttach> weapon_attachments;

    public List<WeaponAttach> GetWeaponAttach(string type)
	{
        return weapon_attachments.FindAll(x => x.type == type);
	}
}

public struct Item
{
    public string item_key;
    public Base @base;
    public Graphic graphic;

    public struct Base
	{
        public string name;
        public string name_key;
        public string atribute_comment_key;
        public string feature_comment_key;
        public string sex;
    }

    public struct Graphic
	{
        public string icon_image;
        public string to_part_scene_file;
        public (string to_node_scene_file, string to_node_parent_node, string to_node_animation_part)[] nodes;
        public string hiding_option;
    }
}

struct Weapon
{
    public string item_key;
    public Ability ability;
    public Scene scene;
    public Resources resources;
    public struct Ability
    {
        public string type;
        public string category;
        public string rate_of_fire;
        public string power;
        public string move_speed_rate;
        public string attack_move_speed_rate;
        public string magazine_capacity;
        public string cracked_magazine_capacity;
        public string max_ammo;
        public string max_ammo_limit;
        public string accuracy;
        public string range;
    }
    public struct Scene
	{
        public string[] values;
	}
    public struct Resources
    {
        public string reload_sound_file;
        public string slot_image_file;
        public string crosshair_file;
        public string crosshair_zoomin_file;

    }
}

struct WeaponAttach
{
    public string type;
    public string sceneIndex;
    public string attackAttach;
    public string link;
    public string idleAttach;
}