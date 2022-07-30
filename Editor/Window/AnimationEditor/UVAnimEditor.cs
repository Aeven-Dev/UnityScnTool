using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.Events;
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

    VisualElement R_S;
    VisualElement R_C;
    VisualElement R_P;

    SelectionElement selectionRectangle;

    FloatField scale_1;
    FloatField scale_2;
    Button SetScale_1;
    Button SetScale_2;
    Button SelectAllButton;
    Button SelectNoneButton;

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
        var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(AevenScnTool.IO.ScnFileImporter.RootPath + "Editor/Window/AnimationEditor/UVAnimEditor.uxml");
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
        R_S = this.Q("R_C_S");
        R_C = this.Q("R_C_T");
        scale_1 = this.Q<FloatField>("Scale_Selection");
        scale_2 = this.Q<FloatField>("Scale_Center");
        SetScale_1 = this.Q<Button>("SetScaleSelection");
        SetScale_2 = this.Q<Button>("SetScaleCenter");


        //SelectAllButton = this.Q<Button>("SelectAllButton");
        //SelectNoneButton = this.Q<Button>("SelectNoneButton");
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
        SetRotationCallbacks();
        SetScaleCallbacks();
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
		if (bones.Count == 0)
		{
            keyframeController.frameController.SetFrame(0);
            zoomViewport.UpdateViewport();
            return;
		}
        int duration = bones[new List<S4Animations>(bones.Keys)[0]].TransformKeyData.duration;

        frameSlider.highValue = duration;

        keyframeController.SetTotalFrames(duration);

        keyframeController.Enable();

        keyframeController.frameController.SetFrame(0);
        zoomViewport.UpdateViewport();
        List<Mesh> meshes = new();
        thing.Clear();
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
				if (uvs == null)
				{

                    uvList[mesh] = mesh.uv;
                }
				else
                {
                    uvList[mesh] = uvs;
                }
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
		foreach (var pair in uvList)
		{
            S4Animation sa = thing[pair.Key];
            sa.KeyUVs(keyframeController.frameController.currentFrame, pair.Value);
            //Debug.Log();
		}
    }

    void KeySel()
    {
        foreach (var pair in uvList)
        {
            S4Animation sa = thing[pair.Key];
            sa.KeyUVs(keyframeController.frameController.currentFrame, pair.Value);
            //Debug.Log();
        }
    }


    void GenerateControlPoints(List<Mesh> meshes)
    {
        ClearControlPoints();
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
            var dot = new UVControl(mesh, index, new Vector2(pos.x * pointContainer.resolvedStyle.width, pos.y * pointContainer.resolvedStyle.height));
            dot.onMove.AddListener((m,i,p)=> { SetOneUV(m, i, new Vector2(p.x / pointContainer.resolvedStyle.width, p.y / pointContainer.resolvedStyle.height)); });

            return dot;
		}
    }

    void ClearControlPoints()
	{
        pointContainer.Clear();
        controllPoints.Clear();

    }


    void SetSelectionLogic()
	{
        selectionRectangle = new SelectionElement(zoomViewport.contentContainer, SelectRect);
        zoomViewport.contentContainer.RegisterCallback<MouseDownEvent>(selectionRectangle.SelectionDown);
        zoomViewport.contentContainer.RegisterCallback<MouseMoveEvent>(selectionRectangle.SelectionMove);
        zoomViewport.contentContainer.RegisterCallback<MouseUpEvent>(selectionRectangle.SelectionUp);

        SelectAllButton.clicked += SelectAll;
        SelectNoneButton.clicked += SelectNone;
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

    void SelectAll()
    {
        selectedPoints.Clear();
        foreach (var item1 in controllPoints)
        {
            foreach (var item2 in item1.Value)
            {
                selectedPoints.Add(item2);
                item2.style.backgroundColor = Color.yellow;
            }
        }
    }

    void SelectNone()
    {
        selectedPoints.Clear();
        foreach (var item1 in controllPoints)
        {
            foreach (var item2 in item1.Value)
            {
                item2.style.backgroundColor = Color.red;
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

    void SetRotationCallbacks()
    {
        ((Button)R_S[0]).clicked += () => RotateSelected(-90, CenterOfSelected());
        ((Button)R_S[1]).clicked += () => RotateSelected(-10, CenterOfSelected());
        ((Button)R_S[2]).clicked += () => RotateSelected(-1, CenterOfSelected());
        ((Button)R_S[3]).clicked += () => RotateSelected(1, CenterOfSelected());
        ((Button)R_S[4]).clicked += () => RotateSelected(10, CenterOfSelected());
        ((Button)R_S[5]).clicked += () => RotateSelected(90, CenterOfSelected());

        ((Button)R_C[0]).clicked += () => RotateSelected(-90, Center());
        ((Button)R_C[1]).clicked += () => RotateSelected(-10, Center());
        ((Button)R_C[2]).clicked += () => RotateSelected(-1, Center());
        ((Button)R_C[3]).clicked += () => RotateSelected(1, Center());
        ((Button)R_C[4]).clicked += () => RotateSelected(10, Center());
        ((Button)R_C[5]).clicked += () => RotateSelected(90, Center());
    }

    void SetScaleCallbacks()
    {
        SetScale_1.clicked += () => ScaleSelected(scale_1.value, CenterOfSelected());
        SetScale_2.clicked += () => ScaleSelected(scale_2.value, Center());
    }
    Vector2 Center()
    {
        return new Vector2(0.5f,0.5f);
        //return new Vector2(pointContainer.resolvedStyle.width / 2f, pointContainer.resolvedStyle.height / 2f);
    }

    Vector2 CenterOfSelected()
	{
        Vector2 center = new Vector2(0, 0);
		foreach (var item in selectedPoints)
		{
            center += uvList[item.mesh][item.index];
        }
        center /= selectedPoints.Count;
        return center;
	}

    void RotateSelected(float angle, Vector2 center)
    {
        if (keyframeController.IsPlaying)
        {
            return;
        }
        Dictionary<Mesh, List<(int, Vector2)>> thing = new();
        foreach (var item in selectedPoints)
        {
            Vector2 point = uvList[item.mesh][item.index];
            Vector2 pos = (Vector2)(Quaternion.Euler(0f, 0f, angle) * (point - center)) + center;

            item.style.left = new StyleLength(new Length(pos.x * pointContainer.resolvedStyle.width, LengthUnit.Pixel));
            item.style.top = new StyleLength(new Length(pos.y * pointContainer.resolvedStyle.height, LengthUnit.Pixel));

            if (!thing.ContainsKey(item.mesh))
            {
                thing.Add(item.mesh, new List<(int, Vector2)>());
            }
            thing[item.mesh].Add((item.index, pos));
        }

        SetSomeUVS(thing);
    }
    void ScaleSelected(float amount, Vector2 center)
    {
        if (keyframeController.IsPlaying)
        {
            return;
        }
        Dictionary<Mesh, List<(int, Vector2)>> thing = new();
        foreach (var item in selectedPoints)
        {
            Vector2 point = uvList[item.mesh][item.index];
            Vector2 dir = point - center;
            dir *= amount;
            Vector2 pos = center + dir;

            item.style.left = new StyleLength(new Length(pos.x * pointContainer.resolvedStyle.width, LengthUnit.Pixel));
            item.style.top = new StyleLength(new Length(pos.y * pointContainer.resolvedStyle.height, LengthUnit.Pixel));

            if (!thing.ContainsKey(item.mesh))
            {
                thing.Add(item.mesh, new List<(int, Vector2)>());
            }
            thing[item.mesh].Add((item.index, pos));
        }

        SetSomeUVS(thing);
    }

    public void RegisterSetTotalFramesCallback(EventCallback<ChangeEvent<int>> callback)
    {
        keyframeController.RegisterSetTotalFramesCallback(callback);
    }
}

class UVControl : VisualElement
{
    public Mesh mesh;
    public int index;

    public UnityEvent<Mesh, int, Vector2> onMove;

    public UVControl(Mesh _mesh, int _index, Vector2 position)
	{
        onMove = new UnityEvent<Mesh, int, Vector2>();
        style.position = Position.Absolute;
        style.width = 6f;
        style.height = 6f;
        style.translate = new StyleTranslate(new Translate(-3, -3, 0f));
        style.backgroundColor = Color.red;

        style.left = new StyleLength(new Length(position.x, LengthUnit.Pixel));
        style.top = new StyleLength(new Length(position.y, LengthUnit.Pixel));

        mesh = _mesh;
        index = _index;

        RegisterCallback<MouseDownEvent>(_ => { this.CaptureMouse(); });
        RegisterCallback<MouseUpEvent>(_ => { this.ReleaseMouse(); });
        RegisterCallback<MouseMoveEvent>(e =>
        {
            if (this.HasMouseCapture())
            {
                float newX = resolvedStyle.left + e.mouseDelta.x;
                float newY = resolvedStyle.top + e.mouseDelta.y;
                style.left = new StyleLength(new Length(newX, LengthUnit.Pixel));
                style.top = new StyleLength(new Length(newY, LengthUnit.Pixel));

                Vector2 pos = new Vector2(newX , newY );

                onMove.Invoke(mesh, index, pos);
            }
        });
    }
}

class SelectionElement : VisualElement
{
    Vector2 initialPos = Vector2.zero;
    UnityEvent<Vector2,Vector2> callback;
    VisualElement targetParent;
    public SelectionElement(VisualElement targetParent, UnityAction<Vector2, Vector2> callback)
	{
        this.targetParent = targetParent;
        this.callback = new UnityEvent<Vector2, Vector2>();
        this.callback.AddListener(callback);
	}

    public void SelectionDown(MouseDownEvent e)
    {
        if (e.button != 0)
        {
            return;
        }
        style.left = e.localMousePosition.x;
        style.top = e.localMousePosition.y;
        style.width = 0;
        style.height = 0;

        targetParent.CaptureMouse();

        style.position = Position.Absolute;
        initialPos = e.localMousePosition;

        style.borderBottomWidth = 1f;
        style.borderTopWidth = 1f;
        style.borderLeftWidth = 1f;
        style.borderRightWidth = 1f;

        style.borderBottomColor = Color.gray;
        style.borderTopColor = Color.gray;
        style.borderLeftColor = Color.gray;
        style.borderRightColor = Color.gray;

        targetParent.Add(this);
        BringToFront();
    }

    public void SelectionMove(MouseMoveEvent e)
	{
        if (e.button != 0)
        {
            return;
        }
        if (targetParent.HasMouseCapture())
        {
            Vector2 min = new Vector2(Mathf.Min(initialPos.x, e.localMousePosition.x), Mathf.Min(initialPos.y, e.localMousePosition.y));
            Vector2 max = new Vector2(Mathf.Max(initialPos.x, e.localMousePosition.x), Mathf.Max(initialPos.y, e.localMousePosition.y));

            style.left = min.x;
            style.top = min.y;
            style.width = max.x - min.x;
            style.height = max.y - min.y;
        }
    }

    public void SelectionUp(MouseUpEvent e)
	{
        if (e.button != 0)
        {
            return;
        }
        Vector2 min = new Vector2(Mathf.Min(initialPos.x, e.localMousePosition.x), Mathf.Min(initialPos.y, e.localMousePosition.y));
        Vector2 max = new Vector2(Mathf.Max(initialPos.x, e.localMousePosition.x), Mathf.Max(initialPos.y, e.localMousePosition.y));
        targetParent.ReleaseMouse();
        targetParent.Remove(this);


        callback.Invoke(min, max);
    }
}

