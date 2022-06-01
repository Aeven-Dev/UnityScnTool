using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class MorphEditorWindow : EditorWindow
{
    S4Animations animations;
    MorphEditor editor;
    Button attach;
    Button detach;

    Slider frameSlider;

    private void OnSelectionChange()
    {
        CheckSelectedObject();
    }

    private void OnHierarchyChange()
    {
        if (animations == null)
        {
            Detach();
        }
    }

    [MenuItem("Window/Animation Window! :D")]
    public static void Init()
    {
        AnimationWindow wnd = GetWindow<AnimationWindow>();
        wnd.titleContent = new GUIContent("AnimationWindow");
    }

    void CreateGUI()
    {
        //Init------------------------------
        VisualElement root = rootVisualElement;
        var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/ScnToolByAeven/Editor/Window/AnimationEditor/MorphEditorWindow.uxml");
        visualTree.CloneTree(root);

        editor = root.Q("Editor") as MorphEditor;

        attach = root.Q("Attach") as Button;
        attach.clicked += Attach;

        detach = root.Q("Detach") as Button;
        detach.clicked += Detach;
    }

    void CheckSelectedObject()
    {
        if (Selection.activeGameObject)
        {
            if (Selection.activeGameObject.GetComponent<S4Animations>())
            {
                if (Selection.activeGameObject.GetComponent<MeshRenderer>())
                {
                    attach.SetEnabled(true);
                }
				else if (Selection.activeGameObject.GetComponent<SkinnedMeshRenderer>())
                {
                    attach.SetEnabled(true);
                }
            }
        }
        else
        {
            attach.SetEnabled(false);
        }
    }

    void Attach()
	{
        animations = Selection.activeGameObject.GetComponent<S4Animations>();
        attach.style.display = DisplayStyle.None;
        detach.style.display = DisplayStyle.Flex;
    }

    void Detach()
	{
        detach.style.display = DisplayStyle.None;
        attach.style.display = DisplayStyle.Flex;
    }

}
