using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

public class MorphEditorWindow : EditorWindow
{
    S4Animations s4a;
    Mesh mesh;
    MorphEditor editor;
    Button attach;
    Button detach;

    SliderInt frameSlider;
    IntegerField frameField;

    int currentFrame = 0;
    int totalFrames = 0;
    List<int> keyframes;
    int animationIndex = -1;

    private void OnSelectionChange()
    {
        CheckSelectedObject();
    }

    private void OnHierarchyChange()
    {
        if (s4a == null)
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

        GetGUIReferences(root);
        SetCallBacks();
    }

    void GetGUIReferences(VisualElement root)
    {
        editor = root.Q("Editor") as MorphEditor;

        attach = root.Q("Attach") as Button;

        detach = root.Q("Detach") as Button;
    }

    void SetCallBacks()
	{
        attach.clicked += Attach;
        detach.clicked += Detach;

        frameSlider.RegisterValueChangedCallback((e) => { SetFrame(e.newValue); });
        frameField.RegisterValueChangedCallback((e) => { SetFrame(e.newValue); });
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

    void PreviousFrame()
    {
        int frame = currentFrame - 1;
        if (frame < 0) frame = totalFrames;

        SetFrame(frame);
    }
    void NextFrame()
    {
        int frame = currentFrame + 1;
        if (frame > totalFrames) frame = 0;

        SetFrame(frame);
    }
    void PreviousKey()
    {
        if (keyframes.Count == 1)
        {
            SetFrame(keyframes[0]);
            return;
        }
        for (int i = 0; i < keyframes.Count; i++)
        {
            if (currentFrame == keyframes[i])
            {
                if (i == 0)
                {
                    SetFrame(keyframes[keyframes.Count - 1]);
                    return;
                }
                else
                {
                    SetFrame(keyframes[i - 1]);
                    return;
                }
            }
            if (i == keyframes.Count - 1)
            {
                SetFrame(keyframes[i - 1]);
                return;
            }
            if (currentFrame > keyframes[i] && currentFrame < keyframes[i + 1])
            {
                SetFrame(keyframes[i]);
                return;
            }
        }
    }
    void NextKey()
    {
        if (keyframes.Count == 1)
        {
            SetFrame(keyframes[0]);
            return;
        }
        for (int i = 0; i < keyframes.Count; i++)
        {
            if (currentFrame == keyframes[i])
            {
                if (i == keyframes.Count - 1)
                {
                    SetFrame(keyframes[0]);
                    return;
                }
                else
                {
                    SetFrame(keyframes[i + 1]);
                    return;
                }
            }
            if (i == keyframes.Count - 1)
            {
                SetFrame(keyframes[0]);
                return;
            }
            if (currentFrame > keyframes[i] && currentFrame < keyframes[i + 1])
            {
                SetFrame(keyframes[i + 1]);
                return;
            }
        }
    }

    public void SetAnimation(int index)
	{
        currentFrame = 0;
        animationIndex = index;
        totalFrames = s4a.animations[animationIndex].TransformKeyData.duration;
        keyframes = new();
		for (int i = 0; i < s4a.animations[animationIndex].MorphKeys.Count; i++)
		{
            keyframes.Add(s4a.animations[animationIndex].MorphKeys[i].frame);
		}
    }

    void SetFrame( int frame )
	{
		if (animationIndex == -1)
		{
            return;
		}
        Vector2[] uvs = s4a.animations[animationIndex].SampleUVs(frame);
        editor.SetUVs(uvs);
	}

    void Attach()
	{
        s4a = Selection.activeGameObject.GetComponent<S4Animations>();
        var mf = Selection.activeGameObject.GetComponent<MeshFilter>();
        if (mf) mesh = mf.mesh;
		else
		{
            var smr = Selection.activeGameObject.GetComponent<SkinnedMeshRenderer>();
            if (smr) mesh = smr.sharedMesh;
			else
			{
                Debug.LogError("Whoopsie! For some reason this object doesnt have a mesh in it! How weird");
			}
        }
        attach.style.display = DisplayStyle.None;
        detach.style.display = DisplayStyle.Flex;

        editor.SetMesh(mesh);
    }

    void Detach()
	{
        detach.style.display = DisplayStyle.None;
        attach.style.display = DisplayStyle.Flex;

        animationIndex = -1;
    }

}
