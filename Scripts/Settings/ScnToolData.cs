using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class ScnToolData : ScriptableObject
{
	static string rootPath = null;
	public static string RootPath
	{
		get
		{
			if (rootPath == null)
			{
				GetRootPath();
			}
			return rootPath;
		}
	}
	public string s4_folder_path = "";
	public string s4_startup_file = "";
	public float scale = 1;
	public string main_animation_name = "DANCE_(>w<)";

	public bool uv_flipVertical = false;
	public bool uv_flipHorizontal = false;

	public Material base_mat;
	Material GetMatFromShader(RenderFlag shader)
	{


		/*
		if (shader.HasFlag(NetsphereScnTool.Scene.Shader.None))
		{

		}
		if (shader.HasFlag(NetsphereScnTool.Scene.Shader.NoLight))
		{

		}
		if (shader.HasFlag(NetsphereScnTool.Scene.Shader.Transparent))
		{

		}
		if (shader.HasFlag(NetsphereScnTool.Scene.Shader.Cutout))
		{

		}
		if (shader.HasFlag(NetsphereScnTool.Scene.Shader.NoCulling))
		{

		}
		if (shader.HasFlag(NetsphereScnTool.Scene.Shader.Billboard))
		{

		}
		if (shader.HasFlag(NetsphereScnTool.Scene.Shader.Flare))
		{

		}
		if (shader.HasFlag(NetsphereScnTool.Scene.Shader.ZWriteOff))
		{

		}
		if (shader.HasFlag(NetsphereScnTool.Scene.Shader.Shader))
		{

		}
		if (shader.HasFlag(NetsphereScnTool.Scene.Shader.NoFog))
		{

		}
		if (shader.HasFlag(NetsphereScnTool.Scene.Shader.NoMipmap))
		{

		}
		if (shader.HasFlag(NetsphereScnTool.Scene.Shader.Shadow))
		{

		}
		if (shader.HasFlag(NetsphereScnTool.Scene.Shader.Water))
		{

		}
		if (shader.HasFlag(NetsphereScnTool.Scene.Shader.Distortion))
		{

		}
		if (shader.HasFlag(NetsphereScnTool.Scene.Shader.Dark))
		{

		}
		*/


		return base_mat;
	}

	static ScnToolData instance;
	public static ScnToolData Instance
	{
		get
		{
			if (!instance)
			{
				instance = AssetDatabase.LoadAssetAtPath<ScnToolData>(RootPath + "Editor/Data/Data.asset");
				if (!instance)
				{
					instance = CreateInstance<ScnToolData>(); 
					AssetDatabase.CreateAsset(instance, RootPath + "Editor/Data/Data.asset");
				}
			}
			return instance;
		} }
	static void GetRootPath()
	{
		if (rootPath != null)
		{
			return;
		}
		var files = AssetDatabase.FindAssets("t:script").Select(AssetDatabase.GUIDToAssetPath);
		foreach (var item in files)
		{
			if (item.EndsWith("ScnFileIO.cs"))
			{
				rootPath = item.Replace("ScnFileIO.cs", "");
				return;
			}
		}
	}
	public static string GetRandomName()
	{
		return names[Random.Range(0, names.Length)];
	}
	static string[] names = new string[]{
		"Michael",
		"Christopher",
		"Jessica",
		"Matthew",
		"Ashley",
		"Jennifer",
		"Joshua",
		"Amanda",
		"Daniel",
		"David",
		"James",
		"Robert",
		"John",
		"Joseph",
		"Andrew",
		"Ryan",
		"Brandon",
		"Jason",
		"Justin",
		"Sarah",
		"William",
		"Jonathan",
		"Stephanie",
		"Brian",
		"Nicole",
		"Nicholas",
		"Anthony",
		"Heather",
		"Eric",
		"Elizabeth",
		"Adam",
		"Megan",
		"Melissa",
		"Kevin",
		"Steven",
		"Thomas",
		"Timothy",
		"Christina",
		"Kyle",
		"Rachel",
		"Laura",
		"Lauren",
		"Amber",
		"Brittany",
		"Danielle",
		"Richard",
		"Kimberly",
		"Jeffrey",
		"Amy",
		"Crystal",
		"Michelle",
		"Tiffany",
		"Jeremy",
		"Benjamin",
		"Mark",
		"Emily",
		"Aaron",
		"Charles",
		"Rebecca",
		"Jacob",
		"Stephen",
		"Patrick",
		"Sean",
		"Erin",
		"Zachary",
		"Jamie",
		"Kelly",
		"Samantha",
		"Nathan",
		"Sara",
		"Dustin",
		"Paul",
		"Angela",
		"Tyler",
		"Scott",
		"Katherine",
		"Andrea",
		"Gregory",
		"Erica",
		"Mary",
		"Travis",
		"Lisa",
		"Kenneth",
		"Bryan",
		"Lindsey",
		"Kristen",
		"Jose",
		"Alexander",
		"Jesse",
		"Katie",
		"Lindsay",
		"Shannon",
		"Vanessa",
		"Courtney",
		"Christine",
		"Alicia",
		"Cody",
		"Allison",
		"Bradley",
		"Samuel",
		"Shawn",
		"April",
		"Derek",
		"Kathryn",
		"Kristin",
		"Chad",
		"Jenna",
		"Tara",
		"Maria",
		"Krystal",
		"Jared",
		"Anna",
		"Edward",
		"Julie",
		"Peter",
		"Holly",
		"Marcus",
		"Kristina",
		"Natalie",
		"Jordan",
		"Victoria",
		"Jacqueline",
		"Corey",
		"Keith",
		"Monica",
		"Juan",
		"Donald",
		"Cassandra",
		"Meghan	",
		"Joel",
		"Shane",
		"Phillip",
		"Patricia",
		"Brett",
		"Ronald",
		"Catherine",
		"George",
		"Antonio",
		"Cynthia",
		"Stacy",
		"Kathleen",
		"Raymond",
		"Carlos",
		"Brandi",
		"Douglas",
		"Nathaniel",
		"Ian",
		"Craig",
		"Brandy	",
		"Alex",
		"Valerie",
		"Veronica",
		"Cory",
		"Whitney",
		"Gary",
		"Derrick",
		"Philip	",
		"Luis",
		"Diana",
		"Chelsea",
		"Leslie",
		"Caitlin",
		"Leah",
		"Natasha",
		"Erika",
		"Casey",
		"Latoya",
		"Erik",
		"Dana",
		"Victor",
		"Brent",
		"Dominique",
		"Frank",
		"Brittney",
		"Evan",
		"Gabriel",
		"Julia",
		"Candice",
		"Karen",
		"Melanie",
		"Adrian",
		"Stacey",
		"Margaret",
		"Sheena	",
		"Wesley",
		"Vincent",
		"Alexandra",
		"Katrina",
		"Bethany",
		"Nichole",
		"Larry",
		"Jeffery",
		"Curtis",
		"Carrie",
		"Todd",
		"Blake",
		"Christian",
		"Randy",
		"Dennis",
		"Alison",
		"Trevor",
		"Seth",
		"Kara"
	};
}
