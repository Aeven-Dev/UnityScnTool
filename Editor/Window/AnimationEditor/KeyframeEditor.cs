using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using System.Collections.Generic;
using UnityEngine.Events;

public class KeyframeEditor : VisualElement
{
    public new class UxmlFactory : UxmlFactory<KeyframeEditor, UxmlTraits> { }

    public new class UxmlTraits : VisualElement.UxmlTraits
    {
        public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
        {
            base.Init(ve, bag, cc);

            KeyframeEditor kfe = ve as KeyframeEditor;
            kfe.Init();
        }
    }

    public static float horizonalScale = 10f;
    public static float tranlateScale = 10f;
    public static float rotationScale = 100f;
    public static float scaleScale = 100f;
    public static float alphaScale = 100f;

    public static float zoom;

    public int currentFrame;
    public int totalFrames;
    public UnityEvent onFrameChange;

    VisualElement keyframeContent;
    VisualElement keyLine;

    Scroller vertical;
    SliderInt frameSlider;
    MinMaxSlider horizontal_zoom;
    IntegerField frameField;
    Label totalFramesField;


    VisualElement message;
    VisualElement viewportRoot;
    VisualElement rulerHolder;

    Button prev;
    Button prevKey;
    Button play;
    Button next;
    Button nextKey;

    Button keyAll;
    Button keyPos;
    Button keyRot;
    Button keySca;
    Button keyAlp;

    Label posLabel;

    bool playing;
    float lastFrameTime;
    int lastRuleDrawCount = 0;

    Dictionary<S4Animations, TransformKeyData> bones = new();
    Dictionary<S4Animations, TransformKeyData> copies = new();
    Dictionary<TransformKeyData, TransformChannel> channels = new();
    List<int> keyframes = new();

    public void Init()
    {
        currentFrame = 0;
        // Import UXML
        var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/ScnToolByAeven/Editor/Window/AnimationEditor/KeyframeEditor.uxml");
        visualTree.CloneTree(this);

        frameSlider = this.Q<SliderInt>("FrameSlider");
        vertical = this.Q<Scroller>("Vertical");
        keyframeContent = this.Q("Keyframes");
        keyLine = this.Q("KeyLine");
        horizontal_zoom = this.Q<MinMaxSlider>("Hori_Zoom");
        frameField = this.Q<IntegerField>("FrameField");
        totalFramesField = this.Q<Label>("TotalFrames");

        posLabel = this.Q<Label>("PosLabel");

        prev = this.Q<Button>("Prev");
        play = this.Q<Button>("Play");
        next = this.Q<Button>("Next");
        prevKey = this.Q<Button>("PrevKey");
        nextKey = this.Q<Button>("NextKey");

        keyAll = this.Q<Button>("KeyAll");
        keyPos = this.Q<Button>("KeyPos");
        keyRot = this.Q<Button>("KeyRot");
        keySca = this.Q<Button>("KeySca");
        keyAlp = this.Q<Button>("KeyAlp");

        message = this.Q("Message");
        viewportRoot = this.Q("ViewportRoot");
        rulerHolder = this.Q("RulerHolder");

        SetCallbacks();
        Disable();
        message.style.display = DisplayStyle.Flex;
        viewportRoot.style.display = DisplayStyle.None;
    }

    void SetCallbacks()
    {
        keyframeContent.RegisterCallback<WheelEvent>(KeyframeWheel);
        keyframeContent.RegisterCallback<MouseMoveEvent>((e) => { posLabel.text = e.localMousePosition.x + "x - " + e.localMousePosition.y + "y"; },TrickleDown.TrickleDown);
        keyframeContent.RegisterCallback<MouseLeaveEvent>((e) => { posLabel.text = string.Empty; });
        keyframeContent.RegisterCallback<MouseDownEvent>((evnt) =>
        {
            if (evnt.button == 2) keyframeContent.CaptureMouse();
        });
        keyframeContent.RegisterCallback<MouseMoveEvent>(KeyframeScroll);
        keyframeContent.RegisterCallback<MouseUpEvent>((evnt) =>
        {
            if (evnt.button == 2) keyframeContent.ReleaseMouse();
        });

        vertical.valueChanged += (pos) =>
        {

            SetViewport(horizontal_zoom.value.x, horizontal_zoom.value.y, pos);
        };

        horizontal_zoom.RegisterValueChangedCallback((evnt) =>
        {
            //set zoom
            SetViewport(evnt.newValue.x, evnt.newValue.y, vertical.value);
        });

        prev.clicked += PreviousFrame;
        play.clicked += Play;
        next.clicked += NextFrame;
        prevKey.clicked += PreviousKey;
        nextKey.clicked += NextKey;

        keyAll.clicked += KeyAll;
        keyPos.clicked += KeyPos;
        keyRot.clicked += KeyRot;
        keySca.clicked += KeySca;
        keyAlp.clicked += KeyAlp;

        frameSlider.RegisterValueChangedCallback((e) => { SetFrame(e.newValue); });
        frameField.RegisterValueChangedCallback((e) => { SetFrame(e.newValue); });

        keyLine[1].RegisterCallback<MouseOverEvent>((evnt) =>
        {
            keyLine[1][0].style.visibility = Visibility.Visible;
        });
        keyLine[1].RegisterCallback<MouseLeaveEvent>((evnt) =>
        {
            keyLine[1][0].style.visibility = Visibility.Hidden;
        });

        keyLine[1].RegisterCallback<MouseDownEvent>((evnt) =>
        {
            if (evnt.button == 0)
            {
                keyLine[1].CaptureMouse();
                keyLine[1].RegisterCallback<MouseMoveEvent>(KeylineDrag);
            }
        });
        keyLine[1].RegisterCallback<MouseUpEvent>((evnt) =>
        {
            if (evnt.button == 0)
            {
                keyLine[1].ReleaseMouse();
                keyLine[1].UnregisterCallback<MouseMoveEvent>(KeylineDrag);
            }
        });
        
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
		//There's many options
		//Option 1: Next global key, expensive, confusing for the user, denied
		//Option 2: Next activated key, simpler, could be wonky, but stable
		//Option 3: Next key of selected, also simple, can be frustrating if
		if (keyframes.Count == 1)
		{
            SetFrame(keyframes[0]);
            return;
		}
		for (int i = 0; i < keyframes.Count; i++)
		{
			if (currentFrame == keyframes[i])
			{
				if ( i == 0)
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
			if (i == keyframes.Count -1)
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
        //There's many options
        //Option 1: Next global key, expensive, confusing for the user, denied
        //Option 2: Next activated key, simpler, could be wonky, but stable
        //Option 3: Next key of selected, also simple, can be frustrating if
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

    void Disable()
	{
        prev.SetEnabled(false);
        play.SetEnabled(false);
        next.SetEnabled(false);
        prevKey.SetEnabled(false);
        nextKey.SetEnabled(false);
        posLabel.text = string.Empty;
        totalFramesField.text = string.Empty;
        keyAll.SetEnabled(false);
        keyPos.SetEnabled(false);
        keyRot.SetEnabled(false);
        keySca.SetEnabled(false);
        keyAlp.SetEnabled(false);

        playing = !playing;
        EditorApplication.update -= PlayAnimation;
        play.text = "Play!";

        SetFrame(0);
    }

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

        frameSlider.style.width = new StyleLength(new Length(zoom * totalFrames / 10f + 14f, LengthUnit.Pixel));
        frameSlider.style.left = hOffset;

        var length = new StyleLength(new Length(currentFrame * zoom / horizonalScale, LengthUnit.Pixel));
        keyLine.style.left = length;
        rulerHolder.style.left = hOffset;
        DrawRuler(totalFrames);
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
                rulerHolder[i].style.left = new StyleLength(new Length((i * 500f * zoom / horizonalScale) + 6, LengthUnit.Pixel));
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

                ruler.style.left = new StyleLength(new Length(i * 500f * zoom / horizonalScale + 6, LengthUnit.Pixel));
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

    void KeyframeScroll(MouseMoveEvent e)
	{
        if (keyframeContent.HasMouseCapture())
        {
            var val = new Vector2(horizontal_zoom.value.x - e.mouseDelta.x * 0.1f, horizontal_zoom.value.y - e.mouseDelta.x * 0.1f);
            if (val.y < horizontal_zoom.highLimit && val.x > horizontal_zoom.lowLimit)
            {
                horizontal_zoom.value = val;
            }
            vertical.value -= e.mouseDelta.y * 0.5f;
        }
    }

    void KeyframeWheel(WheelEvent e)
	{
        if (e.ctrlKey)
        {
            horizontal_zoom.value = new Vector2(horizontal_zoom.value.x - e.delta.y * 0.5f, horizontal_zoom.value.y + e.delta.y * 0.5f);
        }
        else if (e.shiftKey)
        {
            var val = new Vector2(horizontal_zoom.value.x + e.delta.y * 0.5f, horizontal_zoom.value.y + e.delta.y * 0.5f);
            if (val.y < horizontal_zoom.highLimit && val.x > horizontal_zoom.lowLimit)
            {
                horizontal_zoom.value = val;
            }
        }
        else
        {
            vertical.value += e.delta.y * 0.5f;
        }
    }

    void KeylineDrag(MouseMoveEvent e)
	{
        SetFrame(currentFrame + (int)(e.mouseDelta.x * 10f));
    }

    void Play()
    {
        if (playing)
        {
            playing = !playing;
            EditorApplication.update -= PlayAnimation;
            play.text = "Play!";
        }
        else
        {
            lastFrameTime = Time.realtimeSinceStartup;
            playing = !playing;
            EditorApplication.update += PlayAnimation;
            play.text = "Stop!";
        }

    }

    void PlayAnimation()
    {
        if (Time.realtimeSinceStartup - lastFrameTime >= 0.0166666666666667f)
        {
            lastFrameTime = Time.realtimeSinceStartup;

            SetFrame( currentFrame + (int)(1000f / 30f));
            if (currentFrame > new List<TransformKeyData>(bones.Values)[0].duration)
            {
                SetFrame(0);
            }
        }
    }
    
    void SetTransformToFrame( int frame)
	{
        foreach (var part in bones.Keys)
		{
			if (copies.TryGetValue(part, out TransformKeyData copy))
			{

                part.transform.localPosition = copy.SamplePosition(frame);
                part.transform.localRotation = copy.SampleRotation(frame);
                part.transform.localScale = copy.SampleScale(frame);
            }
			else
            {
                TransformKeyData tkd = bones[part];
                part.transform.localPosition = tkd.SamplePosition(frame);
                part.transform.localRotation = tkd.SampleRotation(frame);
                part.transform.localScale = tkd.SampleScale(frame);
            }
        }
    }

    public void AddChannel(TransformKeyData tkd)
    {
        channels.Add(tkd, new TransformChannel(tkd));
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

    public void AnimationChanged(Dictionary<S4Animations, TransformKeyData> bones, Dictionary<S4Animations, TransformKeyData> copies = null)
    {
        keyframes.Clear();

        message.style.display = DisplayStyle.None;
        viewportRoot.style.display = DisplayStyle.Flex;

        this.bones = bones;
        this.copies = copies;

        int duration = bones[new List<S4Animations>(bones.Keys)[0]].duration;

        frameSlider.highValue = duration;
        SetFrame(0);

        totalFrames = duration;
        frameSlider.style.width = new StyleLength(new Length(zoom * totalFrames / 10f + 14f, LengthUnit.Pixel));

        StyleLength zp = new StyleLength(new Length(zoom * 100f, LengthUnit.Percent));
        keyframeContent.style.width = zp;
        keyframeContent.style.height = zp;

        totalFramesField.text = duration.ToString();

        prev.SetEnabled(true);
        play.SetEnabled(true);
        next.SetEnabled(true);
        prevKey.SetEnabled(true);
        nextKey.SetEnabled(true);

        DrawRuler(totalFrames);
        SetViewport(horizontal_zoom.value.x, horizontal_zoom.value.y, vertical.value);
    }

    public void AnimationCleared()
	{
        ClearChannels();
        this.bones = new Dictionary<S4Animations, TransformKeyData>();
        message.style.display = DisplayStyle.Flex;
        viewportRoot.style.display = DisplayStyle.None;
        Disable();
    }

    public void SetFrame(int newFrame)
    {
		if (newFrame < 0)
		{
            return;
		}
        currentFrame = newFrame;

        var length = new StyleLength(new Length(currentFrame * zoom / horizonalScale, LengthUnit.Pixel));
        keyLine.style.left = length;
        frameSlider.SetValueWithoutNotify(currentFrame);
        frameField.SetValueWithoutNotify(currentFrame);

        SetTransformToFrame(currentFrame);
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
        keyframes.Clear();
		foreach (var item in channels)
		{
            foreach (var item2 in item.Value.positionChannel)
            {
                int value = item2.GetValue();
                if (keyframes.Contains(value) == false)
                {
                    keyframes.Add(value);
                }
            }
            foreach (var item2 in item.Value.rotationChannel)
            {
                int value = item2.GetValue();
                if (keyframes.Contains(value) == false)
                {
                    keyframes.Add(value);
                }
            }
            foreach (var item2 in item.Value.scaleChannel)
            {
                int value = item2.GetValue();
                if (keyframes.Contains(value) == false)
                {
                    keyframes.Add(value);
                }
            }
            foreach (var item2 in item.Value.alphaChannel)
            {
                int value = item2.GetValue();
                if (keyframes.Contains(value) == false)
                {
                    keyframes.Add(value);
                }
            }
        }

        keyframes.Sort();
        Debug.Log("Keyframes calculated! " + keyframes.Count);
    }

    public void ToggleKeying( bool state)
	{

        keyAll.SetEnabled(state);
        keyPos.SetEnabled(state);
        keyRot.SetEnabled(state);
        keySca.SetEnabled(state);
        keyAlp.SetEnabled(state);
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
		if (bones.TryGetValue(s4a, out TransformKeyData tkd))
		{
            if (position)
            {
                if(tkd.KeyTranslation(s4a.transform.localPosition, currentFrame))
				{
                    channels[tkd].PopulateChannel(TransformChannel.Channel.Position, keyframeContent[0]);
				}
            }
            if (rotation)
            {
                if(tkd.KeyRotation(s4a.transform.localRotation, currentFrame))
                {
                    channels[tkd].PopulateChannel(TransformChannel.Channel.Rotation, keyframeContent[0]);
                }
            }
            if (scale)
            {
                if (tkd.KeyScale(s4a.transform.localScale, currentFrame))
				{
					channels[tkd].PopulateChannel(TransformChannel.Channel.Scale, keyframeContent[0]);
				}
            }
            if (alpha)
            {

            }
		}
	}
}

public class TransformChannel
{
    public enum Channel { Position, Rotation, Scale, Alpha }

    TransformKeyData part;

    public List<TickElement> positionChannel = new List<TickElement>();
    public List<TickElement> rotationChannel = new List<TickElement>();
    public List<TickElement> scaleChannel = new List<TickElement>();
    public List<TickElement> alphaChannel = new List<TickElement>();

    public TransformChannel(TransformKeyData part)
    {
        this.part = part;
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
            tick.Redraw();
            ((KeyElement)(tick[0])).Redraw(KeyframeEditor.tranlateScale);
            ((KeyElement)(tick[1])).Redraw(KeyframeEditor.tranlateScale);
            ((KeyElement)(tick[2])).Redraw(KeyframeEditor.tranlateScale);
        }
	}

    TickElement CreatePosKeys(int index)
    {
        TickElement tick = new TickElement(part, index, 0);

        var xKey = new KeyElement(Color.red, part, index, 0, 0, PosCallback);
        var yKey = new KeyElement(Color.green, part, index,0, 1, PosCallback);
        var zKey = new KeyElement(Color.blue, part, index, 0, 2, PosCallback);

        xKey.Redraw(KeyframeEditor.tranlateScale);
        yKey.Redraw(KeyframeEditor.tranlateScale);
        zKey.Redraw(KeyframeEditor.tranlateScale);

        tick.Add(xKey);
        tick.Add(yKey);
        tick.Add(zKey);

        return tick;
    }

    void PosCallback(MouseMoveEvent evnt)
    {
        KeyElement keyElem = evnt.target as KeyElement;
        float newX = MoveParent(keyElem.parent, evnt.mouseDelta.x);

        float newValue = keyElem.style.top.value.value + evnt.mouseDelta.y;

        var tKey = part.TransformKey.TKey[keyElem.index];
        Vector3 pos = tKey.Translation;
		switch (keyElem.variable)
        {
            case 0:
                pos.x = newValue * KeyframeEditor.tranlateScale;
                break;
            case 1:
                pos.y = newValue * KeyframeEditor.tranlateScale;
                break;
            case 2:
                pos.z = newValue * KeyframeEditor.tranlateScale;
                break;
            default:
				break;
		}

        tKey.duration = (int)((newX + 3));
        tKey.Translation = pos;
        part.TransformKey.TKey[keyElem.index] = tKey;

        keyElem.Redraw(KeyframeEditor.tranlateScale);
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
        float newX = MoveParent(keyElem.parent, evnt.mouseDelta.x);

        float newValue = keyElem.style.top.value.value + evnt.mouseDelta.y;
        keyElem.style.top = new StyleLength(new Length(newValue));

        var rKey = part.TransformKey.RKey[keyElem.index];
        Quaternion rot = rKey.Rotation;
        switch (keyElem.variable)
        {
            case 0:
                rot.x = newValue / KeyframeEditor.rotationScale;
                break;
            case 1:
                rot.y = newValue / KeyframeEditor.rotationScale;
                break;
            case 2:
                rot.z = newValue / KeyframeEditor.rotationScale;
                break;
            case 3:
                rot.w = newValue / KeyframeEditor.rotationScale;
                break;
            default:
                break;
        }

        rKey.duration = (int)((newX + 3) * KeyframeEditor.horizonalScale);
        rKey.Rotation = rot;
        part.TransformKey.RKey[keyElem.index] = rKey;

    }


    TickElement CreateScaKeys(int index)
    {
        TickElement tick = new TickElement(part,index,2);

        Vector3 pos = part.TransformKey.SKey[index].Scale / KeyframeEditor.tranlateScale;

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
        float newX = MoveParent(keyElem.parent, evnt.mouseDelta.x);

        float newValue = keyElem.style.top.value.value + evnt.mouseDelta.y;
        keyElem.style.top = new StyleLength(new Length(newValue));

        var sKey = part.TransformKey.SKey[keyElem.index];
        Vector3 sca = sKey.Scale;
        switch (keyElem.variable)
        {
            case 0:
                sca.x = newValue / KeyframeEditor.scaleScale;
                break;
            case 1:
                sca.y = newValue / KeyframeEditor.scaleScale;
                break;
            case 2:
                sca.z = newValue / KeyframeEditor.scaleScale;
                break;
            default:
                break;
        }
        sKey.duration = (int)((newX + 3) / KeyframeEditor.horizonalScale);
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
        float newX = MoveParent(keyElem.parent, evnt.mouseDelta.x);

        float newValue = keyElem.style.top.value.value + evnt.mouseDelta.y;
        keyElem.style.top = new StyleLength(new Length(newValue));


        var fKey = part.FloatKeys[keyElem.index];
        fKey.duration = (int)((newX + 3) * KeyframeEditor.horizonalScale);
        fKey.Alpha = newValue / KeyframeEditor.alphaScale;
        part.FloatKeys[keyElem.index] = fKey;

    }



    float MoveParent(VisualElement parent, float deltaX)
    {
        float value = (parent.style.left.value.value + deltaX);
        float newX = value * KeyframeEditor.horizonalScale / KeyframeEditor.zoom;
        if (newX < 0) {
            newX = 0;
            value = 0;
        }
        if (newX > part.duration)
        {
            newX = part.duration;
            value = part.duration * KeyframeEditor.zoom / KeyframeEditor.horizonalScale;
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
        Redraw();
    }

    public int GetValue()
    {
        switch (type)
        {
            case 0:
                return data.TransformKey.TKey[index].duration;
            case 1:
                return data.TransformKey.RKey[index].duration;
            case 2:
                return data.TransformKey.SKey[index].duration;
            case 3:
                return data.FloatKeys[index].duration;
            default:
                return 0;
        }
    }

    public void Redraw()
    {
        style.left = new StyleLength(new Length((GetValue() * KeyframeEditor.zoom / KeyframeEditor.horizonalScale) - 3, LengthUnit.Pixel));
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

    public void Redraw( float scale)
	{
        style.top = new StyleLength(new Length(GetValue() * KeyframeEditor.zoom / scale, LengthUnit.Pixel));
    }
}