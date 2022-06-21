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

    MorphEditor editor;
    VisualElement message;
    VisualElement viewport;
    VisualElement texture;
    VisualElement pointContainer;

    SliderInt frameSlider;

    Dictionary<S4Animations, S4Animation> bones = new();
    Dictionary<S4Animations, S4Animation> copies = new();

    Dictionary<Mesh, Vector2[]> uvList = new();

    Dictionary<Mesh, List<UVControl>> controllPoints = new();

    Dictionary<Mesh, S4Animation> thing = new();

    List<UVControl> selectedPoints = new();

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
        zoomViewport = this.Q<ZoomViewport>("ZoomViewport");
        keyframeController = this.Q<KeyframeController>("KeyframeController");
        frameSlider = this.Q<SliderInt>("FrameSlider");
        message = this.Q("Message");
        viewport = this.Q("Viewport");
        texture = this.Q("Texture");
        pointContainer = this.Q("PointContainer");
    }

    void SetCallBacks()
    {
        keyframeController.frameController.onFrameChange += OnFrameSet;
        zoomViewport.onViewportSet.AddListener(OnViewportSet);

        keyframeController.RegisterClickEventToKeyingButton("KeyAll", KeyAll);
        keyframeController.RegisterClickEventToKeyingButton("KeySel", KeySel);

        frameSlider.RegisterValueChangedCallback((e) => { keyframeController.frameController.SetFrame(e.newValue); });

        SetSelectionLogic();
        SetMoveSelectionLogic();
    }

    void Disable()
    {
        message.style.display = DisplayStyle.Flex;
        viewport.style.display = DisplayStyle.None;
        keyframeController.Disable();
    }
    void OnViewportSet(float zoomPercentage, float hMin, float vPos, float zoomInfluence)
    {
        StyleLength zp = new StyleLength(new Length(zoomPercentage, LengthUnit.Pixel));
        texture.style.width = zp;
        texture.style.height = zp;

        //texture.style.top = new StyleLength(new Length((vPos * zoomInfluence), LengthUnit.Percent));
        texture.style.left = new StyleLength(new Length((zoomPercentage / 2f) - (hMin * zoomViewport.zoom), LengthUnit.Percent));
        texture.style.top = new StyleLength(new Length(50f + (vPos * zoomInfluence), LengthUnit.Percent));
        float offset = -(zoomPercentage / 4f);
        texture.style.translate = new StyleTranslate(new Translate(offset, 0, 0));

        SetControlPoints();
    }
    public void AnimationChanged(Dictionary<S4Animations, S4Animation> bones, Dictionary<S4Animations, S4Animation> copies = null)
    {
        keyframeController.frameController.keyframes.Clear();

        message.style.display = DisplayStyle.None;
        viewport.style.display = DisplayStyle.Flex;

        this.bones = bones;
        this.copies = copies;

        int duration = bones[new List<S4Animations>(bones.Keys)[0]].TransformKeyData.duration;

        frameSlider.highValue = duration;

        keyframeController.SetTotalFrames(duration);

        keyframeController.Enable();

        zoomViewport.UpdateViewport();
        List<Mesh> meshes = new();
		foreach (var item in bones)
		{
            MeshFilter mf = item.Key.gameObject.GetComponent<MeshFilter>();
			if (mf)
            {
                meshes.Add(mf.sharedMesh);
                thing.Add(mf.sharedMesh, item.Value);
            }
        }
        editor.SetMeshes(meshes);

        GenerateControlPoints(meshes);

        CalculateKeyframes();
        keyframeController.frameController.SetFrame(0);
    }

    void OnFrameSet( int frame )
    {
        frameSlider.SetValueWithoutNotify(frame);

        SampleUVS(frame);
        SetControlPoints();


        if (uvList.Count==0)
		{
            return;
		}
        editor.SetUVs(uvList);
	}

    void SampleUVS( int frame )
	{
        foreach (var item in bones)
        {
            MeshFilter mf = item.Key.gameObject.GetComponent<MeshFilter>();
            if (mf)
            {
                Mesh mesh = mf.sharedMesh;
                Vector2[] uvs = item.Value.SampleUVs(frame);
                uvList[mesh] = uvs;
            }
        }
    }

    void SetControlPoints()
	{
        foreach (var item in uvList)
        {
            if (controllPoints.TryGetValue(item.Key, out var points))
            {
                for (int i = 0; i < points.Count; i++)
                {
                    points[i].style.left = new StyleLength(new Length(item.Value[i].x * 100f, LengthUnit.Percent));
                    points[i].style.top = new StyleLength(new Length(item.Value[i].y * 100f, LengthUnit.Percent));
                }
            }
        }
    }

    void CalculateKeyframes()
    {
        List<int> keys = new List<int>();
        foreach (var bone in bones)
        {
            foreach (var morphkey in bone.Value.MorphKeys)
            {
                int value = morphkey.frame;
                if (keys.Contains(value) == false)
                {
                    keys.Add(value);
                }
            }
        }

        keyframeController.SetDopesheet(keys);
    }

    void SetSomeUVS(Dictionary<Mesh, List<(int,Vector2)>> list)
	{
		foreach (var item in list)
		{
			for (int i = 0; i < item.Value.Count; i++)
            {
                uvList[item.Key][item.Value[i].Item1] = item.Value[i].Item2;

            }
		}
        //Set Uv visuals
        editor.SetUVs(uvList);
    }

    void SetOneUV(Mesh mesh, int index, Vector2 pos)
	{
        uvList[mesh][index] = pos;

        editor.SetUVs(uvList);
    }

    void KeyAll()
    {
        Debug.Log("Keying all");
		foreach (var pair in uvList)
		{
            S4Animation sa = thing[pair.Key];
            Debug.Log(sa.KeyUVs(keyframeController.frameController.currentFrame, pair.Value));
		}
    }

    void KeySel()
    {
        
    }

    void GenerateControlPoints(List<Mesh> meshes)
    {
		foreach (var mesh in meshes)
		{
            var list = new List<UVControl>();
            controllPoints.Add(mesh, list);
			for (int i = 0; i < mesh.uv.Length; i++)
			{
                var ve = CreateDot(mesh.uv[i], mesh, i);
                pointContainer.Add(ve);
                list.Add(ve);
			}
		}

        UVControl CreateDot(Vector2 pos, Mesh mesh, int index )
		{
            var dot = new UVControl();
            dot.style.position = Position.Absolute;
            dot.style.width = 6f;
            dot.style.height = 6f;
            dot.style.translate = new StyleTranslate(new Translate(-3, -3, 0f));
            dot.style.backgroundColor = Color.red;

            dot.style.left = new StyleLength(new Length(pos.x * pointContainer.resolvedStyle.width, LengthUnit.Pixel));
            dot.style.top = new StyleLength(new Length(pos.y * pointContainer.resolvedStyle.height, LengthUnit.Pixel));

            dot.mesh = mesh;
            dot.index = index;

            dot.RegisterCallback<MouseDownEvent>(_ => { dot.CaptureMouse(); });
            dot.RegisterCallback<MouseUpEvent>(_ => { dot.ReleaseMouse(); });
			dot.RegisterCallback<MouseMoveEvent>(e =>
			{
				if (keyframeController.IsPlaying)
				{
                    return;
				}
				if (dot.HasMouseCapture())
                {
                    float newX = dot.resolvedStyle.left + e.mouseDelta.x;
                    float newY = dot.resolvedStyle.top + e.mouseDelta.y;
                    dot.style.left = new StyleLength(new Length(newX, LengthUnit.Pixel));
                    dot.style.top = new StyleLength(new Length(newY, LengthUnit.Pixel));

                    Vector2 pos = new Vector2(newX / pointContainer.resolvedStyle.width, newY / pointContainer.resolvedStyle.height);

                    SetOneUV( mesh, index, pos);
                }
            });

            return dot;
		}
    }

    void SetSelectionLogic()
	{
        VisualElement selection = null;
        Vector2 initialPos = Vector2.zero;
        zoomViewport.contentContainer.RegisterCallback<MouseDownEvent>(e => {
			if (e.button != 0)
			{
                return;
			}

            zoomViewport.contentContainer.CaptureMouse();
            selection = new VisualElement();
            selection.style.position = Position.Absolute;
            initialPos = e.localMousePosition;

            selection.style.borderBottomWidth = 1f;
            selection.style.borderTopWidth = 1f;
            selection.style.borderLeftWidth = 1f;
            selection.style.borderRightWidth = 1f;

            selection.style.borderBottomColor = Color.gray;
            selection.style.borderTopColor = Color.gray;
            selection.style.borderLeftColor = Color.gray;
            selection.style.borderRightColor = Color.gray;

            zoomViewport.Add(selection);
            selection.BringToFront();

        });
        zoomViewport.contentContainer.RegisterCallback<MouseMoveEvent>(e => {
			if (e.button != 0)
			{
                return;
			}
            if (zoomViewport.contentContainer.HasMouseCapture())
            {
                Vector2 min = new Vector2(Mathf.Min(initialPos.x, e.localMousePosition.x), Mathf.Min(initialPos.y, e.localMousePosition.y));
                Vector2 max = new Vector2(Mathf.Max(initialPos.x, e.localMousePosition.x), Mathf.Max(initialPos.y, e.localMousePosition.y));

                selection.style.left = min.x;
                selection.style.top = min.y;
                selection.style.width = max.x - min.x;
                selection.style.height = max.y - min.y;
            }
        });
        zoomViewport.contentContainer.RegisterCallback<MouseUpEvent>(e => {
            if (e.button != 0)
            {
                return;
            }
            Vector2 min = new Vector2(Mathf.Min(initialPos.x, e.localMousePosition.x), Mathf.Min(initialPos.y, e.localMousePosition.y));
            Vector2 max = new Vector2(Mathf.Max(initialPos.x, e.localMousePosition.x), Mathf.Max(initialPos.y, e.localMousePosition.y));
            zoomViewport.contentContainer.ReleaseMouse();
            zoomViewport.Remove(selection);

            SelectRect(min, max);
        });
    }

    void SelectRect(Vector2 min, Vector2 max)
    {
        min = new Vector2(min.x - texture.resolvedStyle.left - texture.style.translate.value.x.value, min.y  - texture.resolvedStyle.top - texture.style.translate.value.y.value);
        max = new Vector2(max.x - texture.resolvedStyle.left - texture.style.translate.value.x.value, max.y - texture.resolvedStyle.top - texture.style.translate.value.y.value);

        selectedPoints.Clear();
		foreach (var item1 in controllPoints)
		{
			foreach (var item2 in item1.Value)
			{
				if (item2.resolvedStyle.left >= min.x && item2.resolvedStyle.top >= min.y && item2.resolvedStyle.left <= max.x && item2.resolvedStyle.top <= max.y)
				{
                    selectedPoints.Add(item2);
                    item2.style.backgroundColor = Color.yellow;
				}
				else
				{

                    item2.style.backgroundColor = Color.red;
                }
			}
		}
	}

    void SetMoveSelectionLogic()
	{
        bool moving = false;
        zoomViewport.contentContainer.RegisterCallback<MouseDownEvent>(e => {
            if (e.button == 1) { zoomViewport.contentContainer.CaptureMouse();
                moving = true;
            }

        });
        zoomViewport.contentContainer.RegisterCallback<MouseMoveEvent>(e => {
            if (moving)
            {
                MoveSelected(e.mouseDelta);
            }
        });
        zoomViewport.contentContainer.RegisterCallback<MouseUpEvent>(e => {
            if (e.button == 1) { zoomViewport.contentContainer.ReleaseMouse();
                moving = false;
            }
        });
    }

    void MoveSelected(Vector2 delta)
	{
        if (keyframeController.IsPlaying)
        {
            return;
        }
        Dictionary<Mesh, List<(int, Vector2)>> thing = new();
		foreach (var item in selectedPoints)
		{
            float newX = item.resolvedStyle.left + delta.x;
            float newY = item.resolvedStyle.top + delta.y;
            item.style.left = new StyleLength(new Length(newX, LengthUnit.Pixel));
            item.style.top = new StyleLength(new Length(newY, LengthUnit.Pixel));

            Vector2 pos = new Vector2(newX / pointContainer.resolvedStyle.width, newY / pointContainer.resolvedStyle.height);

			if (!thing.ContainsKey(item.mesh))
			{
                thing.Add(item.mesh, new List<(int, Vector2)>());
			}
            thing[item.mesh].Add((item.index, pos));
        }

        SetSomeUVS(thing);

    }
}

class UVControl : VisualElement
{
    public Mesh mesh;
    public int index;
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