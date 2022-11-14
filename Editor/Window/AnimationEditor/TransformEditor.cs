using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using System.Collections.Generic;
using UnityEngine.Events;

public class TransformEditor : VisualElement
{
    public new class UxmlFactory : UxmlFactory<TransformEditor, UxmlTraits> { }

    public new class UxmlTraits : VisualElement.UxmlTraits
    {
        public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
        {
            base.Init(ve, bag, cc);

            TransformEditor te = ve as TransformEditor;
            te.Clear();
            te.Init();
        }
    }

    public static float horizonalScale = 10f;
    public static float tranlateScale = 10f;
    public static float rotationScale = 100f;
    public static float scaleScale = 100f;
    public static float alphaScale = 100f;

    KeyframeController keyframeController;

    VisualElement keyframeContent;
    VisualElement keyLine;

    SliderInt frameSlider;

    public ZoomViewport zoomViewport;

    VisualElement message;
    VisualElement rulerHolder;

    bool playing;
    int lastRuleDrawCount = 0;

    Dictionary<S4Animations, S4Animation> bones = new();
    Dictionary<S4Animations, S4Animation> copies = new();
    Dictionary<TransformKeyData, TransformChannel> channels = new();

    public void Init()
    {
        // Import UXML
        var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(AevenScnTool.ScnToolData.RootPath + "Editor/Window/AnimationEditor/TransformEditor.uxml");
        visualTree.CloneTree(this);

        GetGUIReferences();
        SetCallbacks();
        Disable();
    }

    void GetGUIReferences()
	{
        zoomViewport        = this.Q<ZoomViewport>      ("Viewport");
        frameSlider         = this.Q<SliderInt>         ("FrameSlider");
        keyframeContent     = this.Q                    ("Keyframes");
        keyLine             = this.Q                    ("KeyLine");
        message             = this.Q                    ("Message");
        rulerHolder         = this.Q                    ("RulerHolder");
        keyframeController  = this.Q<KeyframeController>("KeyframeController");
    }

    void SetCallbacks()
    {
        keyframeController.frameController.onFrameChange += OnFrameSet;
        zoomViewport.onViewportSet.AddListener(OnViewportSet);

        keyframeController.RegisterClickEventToKeyingButton("Key All", KeyAll);
        keyframeController.RegisterClickEventToKeyingButton("P", KeyPos);
        keyframeController.RegisterClickEventToKeyingButton("R", KeyRot);
        keyframeController.RegisterClickEventToKeyingButton("S", KeySca);
        keyframeController.RegisterClickEventToKeyingButton("A", KeyAlp);

        frameSlider.RegisterValueChangedCallback((e) => { keyframeController.frameController.SetFrame(e.newValue); });

        keyLine[1].RegisterCallback<MouseOverEvent>(KeylineHover);
        keyLine[1].RegisterCallback<MouseLeaveEvent>(KeylineLeave);

        keyLine[1].RegisterCallback<MouseDownEvent>(KeylineCapture);
        keyLine[1].RegisterCallback<MouseUpEvent>(KeylineRelease);
        
    }

    void Disable()
    {
        message.style.display = DisplayStyle.Flex;
        zoomViewport.style.display = DisplayStyle.None;
        keyframeController.Disable();
    }
    /*
    void SetViewport(float hMin, float hMax, float vPos)
    {
        float dis = hMax - hMin;
        zoom = 100f / (dis);
        float zoomInfluence = 1f - zoom;

        float zoomPercentage = zoom * 100f;

        StyleLength zp = new StyleLength(new Length(zoomPercentage, LengthUnit.Percent));
        keyframeContent.style.width = zp;
        keyframeContent.style.height = zp;

        StyleLength hOffset = new StyleLength(new Length(-hMin * zoom, LengthUnit.Percent));
        keyframeContent.style.left = hOffset;

        keyframeContent.style.top = new StyleLength(new Length(vPos * zoomInfluence, LengthUnit.Percent));

        vertical.Adjust(dis / 100f);

        frameSlider.style.width = new StyleLength(new Length(zoom * frameController.totalFrames / 10f + 14f, LengthUnit.Pixel));
        frameSlider.style.left = hOffset;

        var length = new StyleLength(new Length(frameController.currentFrame * zoom / horizonalScale, LengthUnit.Pixel));
        keyLine.style.left = length;
        rulerHolder.style.left = hOffset;
        DrawRuler(frameController.totalFrames);
        DrawKeyframes();
    }
    */
    void OnViewportSet(float zoomPercentage, float hMin, float vPos, float zoomInfluence)
	{
        StyleLength zp = new StyleLength(new Length(zoomPercentage, LengthUnit.Percent));
        keyframeContent.style.width = zp;
        keyframeContent.style.height = zp;

        StyleLength hOffset = new StyleLength(new Length(-hMin * zoomViewport.zoom, LengthUnit.Percent));
        keyframeContent.style.left = hOffset;

        keyframeContent.style.top = new StyleLength(new Length(vPos * zoomInfluence, LengthUnit.Percent));

        frameSlider.style.width = new StyleLength(new Length(zoomViewport.zoom * keyframeController.frameController.totalFrames / 10f + 14f, LengthUnit.Pixel));
        frameSlider.style.left = hOffset;

        var length = new StyleLength(new Length(keyframeController.frameController.currentFrame * zoomViewport.zoom / horizonalScale, LengthUnit.Pixel));
        keyLine.style.left = length;
        rulerHolder.style.left = hOffset;
        DrawRuler(keyframeController.frameController.totalFrames);
        DrawKeyframes();
    }

    void DrawRuler(int duration)
	{
        //option 1 draw a line every 1000 frames
        //option 2 draw things smartly, idk how though

        int totalRulers = (duration / 500) + 1;
        if (totalRulers == lastRuleDrawCount)
        {
            for (int i = 0; i < totalRulers; i++)
            {
                rulerHolder[i].style.left = new StyleLength(new Length((i * 500f * zoomViewport.zoom / horizonalScale) + 6, LengthUnit.Pixel));
            }
        }
		else
		{
            lastRuleDrawCount = totalRulers;

            rulerHolder.Clear();
            for (int i = 0; i < totalRulers; i++)
            {
                var ruler = new VisualElement();
                ruler.style.position = Position.Absolute;
                ruler.style.height = new StyleLength(new Length(100, LengthUnit.Percent));
                ruler.style.width = new StyleLength(new Length(1, LengthUnit.Pixel));

                ruler.style.left = new StyleLength(new Length(i * 500f * zoomViewport.zoom / horizonalScale + 6, LengthUnit.Pixel));
                ruler.style.backgroundColor = new StyleColor(Color.gray * 0.5f);

                rulerHolder.Add(ruler);
            }
        }
    }

    void DrawKeyframes()
	{
		foreach (var item in channels)
		{
            item.Value.DrawChannels();
		}
	}

    void KeylineHover(MouseOverEvent e)
    {
        keyLine[1][0].style.visibility = Visibility.Visible;
    }
    void KeylineLeave(MouseLeaveEvent e)
    {
        keyLine[1][0].style.visibility = Visibility.Hidden;
    }
    void KeylineDrag(MouseMoveEvent e)
    {
        keyframeController.frameController.SetFrame(keyframeController.frameController.currentFrame + (int)(e.mouseDelta.x * 10f));
    }
    void KeylineCapture(MouseDownEvent e)
    {
        if (e.button == 0)
        {
            keyLine[1].CaptureMouse();
            keyLine[1].RegisterCallback<MouseMoveEvent>(KeylineDrag);
        }
    }
    void KeylineRelease(MouseUpEvent e)
    {
        if (e.button == 0)
        {
            keyLine[1].ReleaseMouse();
            keyLine[1].UnregisterCallback<MouseMoveEvent>(KeylineDrag);
        }
    }

    bool SetTransformToFrame( int frame)
	{
        bool state = true;
        foreach (var part in bones.Keys)
		{
            if (part == null)
            {
                state = false;
                continue;
            }
            if (copies.TryGetValue(part, out S4Animation copy))
			{

                Vector3 oldPos = part.transform.localPosition;
                Vector3 newPos = copy.TransformKeyData.SamplePosition(frame);
                if (oldPos != newPos)
                    part.transform.localPosition = newPos;

                Quaternion oldRot = part.transform.localRotation;
                Quaternion newRot = copy.TransformKeyData.SampleRotation(frame);
                if (oldRot != newRot)
                    part.transform.localRotation = newRot;

                Vector3 oldSca = part.transform.localScale;
                Vector3 newSca = copy.TransformKeyData.SampleScale(frame);
                if (oldSca != newSca)
                    part.transform.localScale = newSca;
            }
			else
            {
                S4Animation tkd = bones[part];
                Vector3 oldPos = part.transform.localPosition;
                Vector3 newPos = tkd.TransformKeyData.SamplePosition(frame);
                if (oldPos != newPos)
                    part.transform.localPosition = newPos;

                Quaternion oldRot = part.transform.localRotation;
                Quaternion newRot = tkd.TransformKeyData.SampleRotation(frame);
                if (oldRot != newRot)
                    part.transform.localRotation = newRot;

                Vector3 oldSca = part.transform.localScale;
                Vector3 newSca = tkd.TransformKeyData.SampleScale(frame);
                //Debug.Log(oldSca + " - " + newSca);
                if (oldSca != newSca)
                    part.transform.localScale = newSca;
            }
        }
        return state;
    }

    public void AddChannel(TransformKeyData tkd)
    {
        channels.Add(tkd, new TransformChannel(tkd, this));
    }

    public void ClearChannels()
	{
        foreach (var item in channels.Values)
        {
            item.DeleteChannel(TransformChannel.Channel.Position);
            item.DeleteChannel(TransformChannel.Channel.Rotation);
            item.DeleteChannel(TransformChannel.Channel.Scale);
            item.DeleteChannel(TransformChannel.Channel.Alpha);
        }
        channels.Clear();
    }

    public void AnimationChanged(Dictionary<S4Animations, S4Animation> bones, Dictionary<S4Animations, S4Animation> copies = null)
    {
        message.style.display = DisplayStyle.None;
        zoomViewport.style.display = DisplayStyle.Flex;

        this.bones = bones;
        this.copies = copies;

        int duration = bones[new List<S4Animations>(bones.Keys)[0]].TransformKeyData.duration;

        frameSlider.highValue = duration;
        keyframeController.frameController.SetFrame(0);

        keyframeController.SetTotalFrames( duration);
        frameSlider.style.width = new StyleLength(new Length(zoomViewport.zoom * duration / 10f + 14f, LengthUnit.Pixel));

        StyleLength zp = new StyleLength(new Length(zoomViewport.zoom * 100f, LengthUnit.Percent));
        keyframeContent.style.width = zp;
        keyframeContent.style.height = zp;

        keyframeController.Enable();

        DrawRuler(keyframeController.frameController.totalFrames);
        zoomViewport.UpdateViewport();
    }

    public void AnimationCleared()
	{
        ClearChannels();
        this.bones = new Dictionary<S4Animations, S4Animation>();
        Disable();
    }

    public void OnFrameSet(int newFrame)
    {
        var length = new StyleLength(new Length(newFrame * zoomViewport.zoom / horizonalScale, LengthUnit.Pixel));
        keyLine.style.left = length;
        frameSlider.SetValueWithoutNotify(newFrame);

		if (!SetTransformToFrame(newFrame))
		{
            var b = new List<S4Animations>(bones.Keys);
            for (int i = 0; i < b.Count; i++)
            {
                if (b[i] == null)
                {
                    bones.Remove(b[i]);
                }
            }
            var c = new List<S4Animations>(copies.Keys);
            for (int i = 0; i < copies.Count; i++)
            {
                if (c[i] == null)
                {
                    copies.Remove(c[i]);
                }
            }
        }
    }
    public void SetChannel(TransformKeyData part, bool state, TransformChannel.Channel channel)
    {
        if (state)
        {
            channels[part].PopulateChannel(channel, keyframeContent[0]);
            DrawKeyframes();
        }
        else
            channels[part].DeleteChannel(channel);

        CalculateKeyframes();
    }
    public void SetAllChannels(TransformKeyData part, bool state)
    {
        if (state)
        {
            channels[part].PopulateAllChannels(keyframeContent[0]);
            DrawKeyframes();
        }
        else
            channels[part].DeleteAllChannels();

        CalculateKeyframes();
    }

    void CalculateKeyframes()
	{
        List<int> keys = new List<int>();
		foreach (var item in channels)
		{
            foreach (var item2 in item.Value.positionChannel)
            {
                int value = item2.GetValue();
                if (keys.Contains(value) == false)
                {
                    keys.Add(value);
                }
            }
            foreach (var item2 in item.Value.rotationChannel)
            {
                int value = item2.GetValue();
                if (keys.Contains(value) == false)
                {
                    keys.Add(value);
                }
            }
            foreach (var item2 in item.Value.scaleChannel)
            {
                int value = item2.GetValue();
                if (keys.Contains(value) == false)
                {
                    keys.Add(value);
                }
            }
            foreach (var item2 in item.Value.alphaChannel)
            {
                int value = item2.GetValue();
                if (keys.Contains(value) == false)
                {
                    keys.Add(value);
                }
            }
        }

        keyframeController.SetDopesheet(keys);
    }

    public void ToggleKeying( bool state)
	{
        keyframeController.ToggleKeying(state);
    }

    void KeyAll()
    {
        var gos = Selection.gameObjects;
        foreach (var item in gos)
        {
            var s4a = item.GetComponent<S4Animations>();
            if (s4a)
            {
                TransformChanged(s4a, true, true, true, true);
            }
        }
    }
    void KeyPos()
    {
        var gos = Selection.gameObjects;
        foreach (var item in gos)
        {
            var s4a = item.GetComponent<S4Animations>();
            if (s4a)
            {
                TransformChanged(s4a, true, false, false, false);
            }
        }
    }
    void KeyRot()
    {
        var gos = Selection.gameObjects;
        foreach (var item in gos)
        {
            var s4a = item.GetComponent<S4Animations>();
            if (s4a)
            {
                TransformChanged(s4a, false, true, false, false);
            }
        }
    }
    void KeySca()
    {
        var gos = Selection.gameObjects;
        foreach (var item in gos)
        {
            var s4a = item.GetComponent<S4Animations>();
            if (s4a)
            {
                TransformChanged(s4a, false, false, true, false);
            }
        }
    }
    void KeyAlp()
    {
        var gos = Selection.gameObjects;
        foreach (var item in gos)
        {
            var s4a = item.GetComponent<S4Animations>();
            if (s4a)
            {
                TransformChanged(s4a, false, false, false, true);
            }
        }
    }

    void TransformChanged(S4Animations s4a, bool position, bool rotation, bool scale, bool alpha)
    {
		if (bones.TryGetValue(s4a, out S4Animation tkd))
		{
            if (position)
            {
                if(tkd.TransformKeyData.KeyTranslation(s4a.transform.localPosition, keyframeController.frameController.currentFrame))
				{
                    channels[tkd.TransformKeyData].PopulateChannel(TransformChannel.Channel.Position, keyframeContent[0]);
				}
            }
            if (rotation)
            {
                if(tkd.TransformKeyData.KeyRotation(s4a.transform.localRotation, keyframeController.frameController.currentFrame))
                {
                    channels[tkd.TransformKeyData].PopulateChannel(TransformChannel.Channel.Rotation, keyframeContent[0]);
                }
            }
            if (scale)
            {
                if (tkd.TransformKeyData.KeyScale(s4a.transform.localScale, keyframeController.frameController.currentFrame))
				{
					channels[tkd.TransformKeyData].PopulateChannel(TransformChannel.Channel.Scale, keyframeContent[0]);
				}
            }
            if (alpha)
            {

            }
		}
	}

    public void RegisterSetTotalFramesCallback(EventCallback<ChangeEvent<int>> callback)
    {
        keyframeController.RegisterSetTotalFramesCallback(callback);
    }

    public void OnDestroy()
	{
        keyframeController.OnDestroy();
	}
}

public class TransformChannel
{
    TransformEditor editor;
    public enum Channel { Position, Rotation, Scale, Alpha }

    TransformKeyData part;

    public List<TickElement> positionChannel = new List<TickElement>();
    public List<TickElement> rotationChannel = new List<TickElement>();
    public List<TickElement> scaleChannel = new List<TickElement>();
    public List<TickElement> alphaChannel = new List<TickElement>();

    public TransformChannel(TransformKeyData part, TransformEditor editor)
    {
        this.part = part;
        this.editor = editor;
    }

    public void PopulateChannel(Channel channel, VisualElement parent)
    {
        DeleteChannel(channel);
        switch (channel)
        {
            case Channel.Position:
                for (int i = 0; i < part.TransformKey.TKey.Count; i++)
                {
                    var tick = CreatePosKeys(i);
                    parent.Add(tick);
                    positionChannel.Add(tick);
                }
                break;
            case Channel.Rotation:
                for (int i = 0; i < part.TransformKey.RKey.Count; i++)
                {
                    var tick = CreateRotKeys(i);
                    parent.Add(tick);
                    rotationChannel.Add(tick);
                }
                break;
            case Channel.Scale:
                for (int i = 0; i < part.TransformKey.SKey.Count; i++)
                {
                    var tick = CreateScaKeys(i);
                    parent.Add(tick);
                    scaleChannel.Add(tick);
                }
                break;
            case Channel.Alpha:
                for (int i = 0; i < part.FloatKeys.Count; i++)
                {
                    var tick = CreateAlpKeys(i);
                    parent.Add(tick);
                    alphaChannel.Add(tick);
                }
                break;
        }
    }
    public void DeleteChannel(Channel channel)
    {
        switch (channel)
        {
            case Channel.Position:
                foreach (var item in positionChannel)
                {
                    item.parent.Remove(item);
                }
                positionChannel.Clear();
                break;
            case Channel.Rotation:
                foreach (var item in rotationChannel)
                {
                    item.parent.Remove(item);
                }
                rotationChannel.Clear();
                break;
            case Channel.Scale:
                foreach (var item in scaleChannel)
                {
                    item.parent.Remove(item);
                }
                scaleChannel.Clear();
                break;
            case Channel.Alpha:
                foreach (var item in alphaChannel)
                {
                    item.parent.Remove(item);
                }
                alphaChannel.Clear();
                break;
        }
    }

    public void PopulateAllChannels(VisualElement parent)
    {
        DeleteAllChannels();
        for (int i = 0; i < part.TransformKey.TKey.Count; i++)
        {
            var tick = CreatePosKeys(i);
            parent.Add(tick);
            positionChannel.Add(tick);
        }
        for (int i = 0; i < part.TransformKey.RKey.Count; i++)
        {
            var tick = CreateRotKeys(i);
            parent.Add(tick);
            rotationChannel.Add(tick);
        }
        for (int i = 0; i < part.TransformKey.SKey.Count; i++)
        {
            var tick = CreateScaKeys(i);
            parent.Add(tick);
            scaleChannel.Add(tick);
        }
        for (int i = 0; i < part.FloatKeys.Count; i++)
        {
            var tick = CreateAlpKeys(i);
            parent.Add(tick);
            alphaChannel.Add(tick);
        }
    }
    public void DeleteAllChannels()
    {
        foreach (var item in positionChannel)
        {
            item.parent.Remove(item);
        }
        foreach (var item in rotationChannel)
        {
            item.parent.Remove(item);
        }
        foreach (var item in scaleChannel)
        {
            item.parent.Remove(item);
        }
        foreach (var item in alphaChannel)
        {
            item.parent.Remove(item);
        }
        positionChannel.Clear();
        rotationChannel.Clear();
        scaleChannel.Clear();
        alphaChannel.Clear();
    }

    public void DrawChannels()
	{
		for (int i = 0; i < positionChannel.Count; i++)
        {
            var tick = positionChannel[i];
            tick.Redraw(editor.zoomViewport.zoom);
            ((KeyElement)(tick[0])).Redraw(TransformEditor.tranlateScale, editor.zoomViewport.zoom);
            ((KeyElement)(tick[1])).Redraw(TransformEditor.tranlateScale, editor.zoomViewport.zoom);
            ((KeyElement)(tick[2])).Redraw(TransformEditor.tranlateScale, editor.zoomViewport.zoom);
        }
	}

    TickElement CreatePosKeys(int index)
    {
        TickElement tick = new TickElement(part, index, 0);
        tick.Redraw(editor.zoomViewport.zoom);

        var xKey = new KeyElement(Color.red, part, index, 0, 0, PosCallback);
        var yKey = new KeyElement(Color.green, part, index,0, 1, PosCallback);
        var zKey = new KeyElement(Color.blue, part, index, 0, 2, PosCallback);

        xKey.Redraw(TransformEditor.tranlateScale, editor.zoomViewport.zoom);
        yKey.Redraw(TransformEditor.tranlateScale, editor.zoomViewport.zoom);
        zKey.Redraw(TransformEditor.tranlateScale, editor.zoomViewport.zoom);

        tick.Add(xKey);
        tick.Add(yKey);
        tick.Add(zKey);

        return tick;
    }

    void PosCallback(MouseMoveEvent evnt)
    {
        KeyElement keyElem = evnt.target as KeyElement;
        float newX = MoveParent(keyElem.parent, evnt.mouseDelta.x, editor.zoomViewport.zoom);

        float newValue = keyElem.style.top.value.value + evnt.mouseDelta.y;

        var tKey = part.TransformKey.TKey[keyElem.index];
        Vector3 pos = tKey.Translation;
		switch (keyElem.variable)
        {
            case 0:
                pos.x = newValue * TransformEditor.tranlateScale;
                break;
            case 1:
                pos.y = newValue * TransformEditor.tranlateScale;
                break;
            case 2:
                pos.z = newValue * TransformEditor.tranlateScale;
                break;
            default:
				break;
		}

        tKey.frame = (int)((newX + 3));
        tKey.Translation = pos;
        part.TransformKey.TKey[keyElem.index] = tKey;

        keyElem.Redraw(TransformEditor.tranlateScale, editor.zoomViewport.zoom);
    }


    TickElement CreateRotKeys(int index)
    {
        TickElement tick = new TickElement(part, index, 1);

        Quaternion rot = part.TransformKey.RKey[index].Rotation;

        var xKey = new KeyElement(Color.red, part, index, 1, 0, RotCallback);
        var yKey = new KeyElement(Color.red, part, index, 1, 1, RotCallback);
        var zKey = new KeyElement(Color.red, part, index, 1, 2, RotCallback);
        var wKey = new KeyElement(Color.red, part, index, 1, 3, RotCallback);

        return tick;
    }

    void RotCallback(MouseMoveEvent evnt)
    {
        KeyElement keyElem = evnt.target as KeyElement;
        float newX = MoveParent(keyElem.parent, evnt.mouseDelta.x, editor.zoomViewport.zoom);

        float newValue = keyElem.style.top.value.value + evnt.mouseDelta.y;
        keyElem.style.top = new StyleLength(new Length(newValue));

        var rKey = part.TransformKey.RKey[keyElem.index];
        Quaternion rot = rKey.Rotation;
        switch (keyElem.variable)
        {
            case 0:
                rot.x = newValue / TransformEditor.rotationScale;
                break;
            case 1:
                rot.y = newValue / TransformEditor.rotationScale;
                break;
            case 2:
                rot.z = newValue / TransformEditor.rotationScale;
                break;
            case 3:
                rot.w = newValue / TransformEditor.rotationScale;
                break;
            default:
                break;
        }

        rKey.frame = (int)((newX + 3) * TransformEditor.horizonalScale);
        rKey.Rotation = rot;
        part.TransformKey.RKey[keyElem.index] = rKey;

    }


    TickElement CreateScaKeys(int index)
    {
        TickElement tick = new TickElement(part,index,2);

        Vector3 pos = part.TransformKey.SKey[index].Scale / TransformEditor.tranlateScale;

        var xKey = new KeyElement(Color.red, part, index, 2, 0, ScaCallback);
        var yKey = new KeyElement(Color.green, part, index, 2, 1, ScaCallback);
        var zKey = new KeyElement(Color.blue, part, index, 2, 2, ScaCallback);

        tick.Add(xKey);
        tick.Add(yKey);
        tick.Add(zKey);

        return tick;
    }

    void ScaCallback(MouseMoveEvent evnt)
    {
        KeyElement keyElem = evnt.target as KeyElement;
        float newX = MoveParent(keyElem.parent, evnt.mouseDelta.x, editor.zoomViewport.zoom);

        float newValue = keyElem.style.top.value.value + evnt.mouseDelta.y;
        keyElem.style.top = new StyleLength(new Length(newValue));

        var sKey = part.TransformKey.SKey[keyElem.index];
        Vector3 sca = sKey.Scale;
        switch (keyElem.variable)
        {
            case 0:
                sca.x = newValue / TransformEditor.scaleScale;
                break;
            case 1:
                sca.y = newValue / TransformEditor.scaleScale;
                break;
            case 2:
                sca.z = newValue / TransformEditor.scaleScale;
                break;
            default:
                break;
        }
        sKey.frame = (int)((newX + 3) / TransformEditor.horizonalScale);
        sKey.Scale = sca;
        part.TransformKey.SKey[keyElem.index] = sKey;
    }


    TickElement CreateAlpKeys( int index)
    {
        TickElement tick = new TickElement(part,index,3);

        var aKey = new KeyElement(Color.cyan, part, index, 3, 0, AlpCallback);

        return tick;
    }


    void AlpCallback(MouseMoveEvent evnt)
    {
        KeyElement keyElem = evnt.target as KeyElement;
        float newX = MoveParent(keyElem.parent, evnt.mouseDelta.x, editor.zoomViewport.zoom);

        float newValue = keyElem.style.top.value.value + evnt.mouseDelta.y;
        keyElem.style.top = new StyleLength(new Length(newValue));


        var fKey = part.FloatKeys[keyElem.index];
        fKey.frame = (int)((newX + 3) * TransformEditor.horizonalScale);
        fKey.Alpha = newValue / TransformEditor.alphaScale;
        part.FloatKeys[keyElem.index] = fKey;

    }

    float MoveParent(VisualElement parent, float deltaX, float zoom)
    {
        float value = (parent.style.left.value.value + deltaX);
        float newX = value * TransformEditor.horizonalScale / zoom;
        if (newX < 0) {
            newX = 0;
            value = 0;
        }
        if (newX > part.duration)
        {
            newX = part.duration;
            value = part.duration * zoom / TransformEditor.horizonalScale;
        }

        parent.style.left = new StyleLength(new Length(value));

        return newX;
    }
}

public class TickElement : VisualElement
{
    TransformKeyData data;
    public int index;
    public int type;
    public TickElement(TransformKeyData data, int index, int type)
    {
        this.data = data;
        this.index = index;
        this.type = type;
        style.position = Position.Absolute;
    }

    public int GetValue()
    {
        switch (type)
        {
            case 0:
                return data.TransformKey.TKey[index].frame;
            case 1:
                return data.TransformKey.RKey[index].frame;
            case 2:
                return data.TransformKey.SKey[index].frame;
            case 3:
                return data.FloatKeys[index].frame;
            default:
                return 0;
        }
    }

    public void Redraw(float zoom)
    {
        style.left = new StyleLength(new Length((GetValue() * zoom / TransformEditor.horizonalScale) - 3, LengthUnit.Pixel));
    }
}

class KeyElement : VisualElement
{
    TransformKeyData data;
    public int index;
    public int type;
    public int variable;
    public KeyElement(Color color, TransformKeyData data, int index, int type, int variable, EventCallback<MouseMoveEvent> e)
	{
        style.position = Position.Absolute;
        style.height = 7;
        style.width = 7;
        style.backgroundColor = color;
        style.rotate = new StyleRotate(new Rotate(new Angle(45f, AngleUnit.Degree)));

        RegisterCallback<MouseDownEvent>((evnt) => {
            this.CaptureMouse();
            this.RegisterCallback(e);
        });
        RegisterCallback<MouseUpEvent>((evnt) => { 
            this.ReleaseMouse();
            this.UnregisterCallback(e);
        });

        this.data = data;
        this.index = index;
        this.type = type;
        this.variable = variable;
    }

    float GetValue()
    {
        switch (type)
        {
            case 0:
                switch (variable)
                {
                    case 0:
                        return data.TransformKey.TKey[index].Translation.x;
                    case 1:
                        return data.TransformKey.TKey[index].Translation.y;
                    case 2:
                        return data.TransformKey.TKey[index].Translation.z;
                    default:
                        return 0;
                }
            case 1:
                switch (variable)
                {
                    case 0:
                        return data.TransformKey.RKey[index].Rotation.x;
                    case 1:
                        return data.TransformKey.RKey[index].Rotation.y;
                    case 2:
                        return data.TransformKey.RKey[index].Rotation.z;
                    case 3:
                        return data.TransformKey.RKey[index].Rotation.w;
                    default:
                        return 0;
                }
            case 2:
                switch (variable)
                {
                    case 0:
                        return data.TransformKey.SKey[index].Scale.x;
                    case 1:
                        return data.TransformKey.SKey[index].Scale.y;
                    case 2:
                        return data.TransformKey.SKey[index].Scale.z;
                    default:
                        return 0;
                }
            case 3:
                return data.FloatKeys[index].Alpha;
            default:
                return 0;
        }
	}

    public void Redraw( float scale, float zoom)
	{
        style.top = new StyleLength(new Length(GetValue() * zoom / scale, LengthUnit.Pixel));
    }
}