using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

public class UVAnimEditor : VisualElement
{
    public new class UxmlFactory : UxmlFactory<UVAnimEditor, UxmlTraits> { }

    public new class UxmlTraits : VisualElement.UxmlTraits
    {
        public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
        {
            base.Init(ve, bag, cc);

            UVAnimEditor uvae = ve as UVAnimEditor;
            uvae.Init();
        }
    }

    public ZoomViewport zoomViewport;

    KeyframeController keyframeController;

    S4Animations s4a;
    Mesh mesh;
    MorphEditor editor;
    VisualElement message;

    SliderInt frameSlider;

    int animationIndex = -1;

    Dictionary<S4Animations, S4Animation> bones = new();
    Dictionary<S4Animations, S4Animation> copies = new();

    public void Init()
    {
        //Init------------------------------
        var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/ScnToolByAeven/Editor/Window/AnimationEditor/UVAnimEditor.uxml");
        visualTree.CloneTree(this);

        GetGUIReferences();
        SetCallBacks();
        Disable();
    }

    void GetGUIReferences()
    {
        editor = this.Q<MorphEditor>("Editor");
        zoomViewport = this.Q<ZoomViewport>("Viewport");
        keyframeController = this.Q<KeyframeController>("KeyframeController");
    }

    void SetCallBacks()
    {
        keyframeController.frameController.onFrameChange += OnFrameSet;

        keyframeController.RegisterClickEventToKeyingButton("KeyAll", KeyAll);
        keyframeController.RegisterClickEventToKeyingButton("KeySel", KeySel);

        frameSlider.RegisterValueChangedCallback((e) => { keyframeController.frameController.SetFrame(e.newValue); });
    }

    void Disable()
    {
        keyframeController.Disable();
    }

    public void AnimationChanged(Dictionary<S4Animations, S4Animation> bones, Dictionary<S4Animations, S4Animation> copies = null)
    {
        keyframeController.frameController.keyframes.Clear();

        message.style.display = DisplayStyle.None;
        zoomViewport.style.display = DisplayStyle.Flex;

        this.bones = bones;
        this.copies = copies;

        int duration = bones[new List<S4Animations>(bones.Keys)[0]].TransformKeyData.duration;

        frameSlider.highValue = duration;
        keyframeController.frameController.SetFrame(0);

        keyframeController.SetTotalFrames(duration);

        zoomViewport.UpdateViewport();
    }

    public void SetAnimation(int index)
	{
        keyframeController.frameController.currentFrame = 0;
        animationIndex = index;
        keyframeController.frameController.totalFrames = s4a.animations[animationIndex].TransformKeyData.duration;
        List<int> keyframes = new();
		for (int i = 0; i < s4a.animations[animationIndex].MorphKeys.Count; i++)
		{
            keyframes.Add(s4a.animations[animationIndex].MorphKeys[i].frame);
		}
        keyframeController.SetDopesheet(keyframes);
    }

    void OnFrameSet( int frame )
	{
		if (animationIndex == -1)
		{
            return;
		}
        Vector2[] uvs = s4a.animations[animationIndex].SampleUVs(frame);
        editor.SetUVs(uvs);
	}

    void KeyAll()
    {
        
    }

    void KeySel()
    {
        
    }
}

/*
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
 */