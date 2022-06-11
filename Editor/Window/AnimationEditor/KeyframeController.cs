using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using System.Collections.Generic;

public class KeyframeController : VisualElement
{

    public new class UxmlFactory : UxmlFactory<KeyframeController, UxmlTraits> {}

    public new class UxmlTraits : VisualElement.UxmlTraits
    {
        UxmlStringAttributeDescription m_Buttons = new UxmlStringAttributeDescription { name = "buttons", defaultValue = "" };
           

        public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
        {
            base.Init(ve, bag, cc);

            KeyframeController zv = ve as KeyframeController;
            //bag.
            zv.buttons = m_Buttons.GetValueFromBag(bag, cc);
            List<string> keyingButtonNames = new(zv.buttons.Split(',',System.StringSplitOptions.RemoveEmptyEntries));
            zv.Clear();
            zv.Init(keyingButtonNames);
        }
    }
    public string buttons { get; set; }

    public FrameController frameController;
    public float dopesheetMult;
    public float dopesheetOffset;

    Button prev;
    Button prevKey;
    Button play;
    Button next;
    Button nextKey;

    IntegerField frameField;
    Label totalFramesField;

    VisualElement dopesheet;
    VisualElement keyingButtonContainer;

    Dictionary<string, Button> keyingButtons;


    bool playing = false;

    public void Init(List<string> keyingButtonNames)
    {
        frameController = new FrameController();
        // Import UXML
        var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/ScnToolByAeven/Editor/Window/AnimationEditor/KeyframeController.uxml");
        Add(visualTree.Instantiate());

        GetGUIReferences();
        SetCallBacks();
        CreateKeyingButtons(keyingButtonNames);
    }

    void GetGUIReferences()
    {
        frameField = this.Q<IntegerField>("FrameField");
        totalFramesField = this.Q<Label>("TotalFrames");

        prev = this.Q<Button>("Prev");
        play = this.Q<Button>("Play");
        next = this.Q<Button>("Next");
        prevKey = this.Q<Button>("PrevKey");
        nextKey = this.Q<Button>("NextKey");

        dopesheet = this.Q("Dopesheet");
        keyingButtonContainer = this.Q("KeyingButtons");
    }

    void SetCallBacks()
    {
        frameController.onFrameChange += frameField.SetValueWithoutNotify;
        prev.clicked += frameController.PreviousFrame;
        play.clicked += Play;
        next.clicked += frameController.NextFrame;
        prevKey.clicked += frameController.PreviousKey;
        nextKey.clicked += frameController.NextKey;
    }

    void CreateKeyingButtons(List<string> keyingButtonNames)
	{
        keyingButtons = new();

        for (int i = 0; i < keyingButtonNames.Count; i++)
		{
            Button button = new Button();
            button.text = keyingButtonNames[i];


            button.style.borderBottomLeftRadius = new StyleLength(0f);
            button.style.borderTopLeftRadius = new StyleLength(0f);
            button.style.borderBottomRightRadius = new StyleLength(0f);
            button.style.borderTopRightRadius = new StyleLength(0f);

            button.style.marginRight = 0;
            button.style.marginLeft = 0;
            button.style.borderRightWidth = 0;
            if (i == 0)
            {
                button.style.borderBottomLeftRadius = new StyleLength(5);
                button.style.borderTopLeftRadius = new StyleLength(5);

            }
			if (i == keyingButtonNames.Count - 1)
			{

                button.style.borderBottomRightRadius = new StyleLength(5);
                button.style.borderTopRightRadius = new StyleLength(5);
                button.style.borderRightWidth = 1;
            }
            keyingButtonContainer.Add(button);
            keyingButtons.Add(keyingButtonNames[i], button);
        }
	}

    public void RegisterClickEventToKeyingButton(string buttonText, System.Action action)
    {
        if (keyingButtons.TryGetValue(buttonText, out Button button))
        {
            button.clicked += action;
        }
    }
    public void UnregisterClickEventToKeyingButton(string buttonText, System.Action action)
    {
        if (keyingButtons.TryGetValue(buttonText, out Button button))
        {
            button.clicked -= action;
        }
    }

    public void SetTotalFrames(int frames)
	{
        totalFramesField.text = frames.ToString();
    }

    public void SetDopesheet(List<int> frames)
	{
        frameController.keyframes = frames;
		foreach (var item in frames)
		{
            VisualElement f = MakeTick(item);
            dopesheet.Add(f);
        }

        VisualElement MakeTick( int left )
        {
            VisualElement f = new();
            dopesheet.Add(f);
            f.style.position = Position.Absolute;
            f.style.height = 7;
            f.style.width = 7;
            f.style.borderBottomLeftRadius = new StyleLength(new Length(50, LengthUnit.Percent));
            f.style.borderBottomRightRadius = new StyleLength(new Length(50, LengthUnit.Percent));
            f.style.borderTopLeftRadius = new StyleLength(new Length(50, LengthUnit.Percent));
            f.style.borderTopRightRadius = new StyleLength(new Length(50, LengthUnit.Percent));
            f.style.left = new StyleLength(new Length((left * dopesheetMult) + dopesheetOffset, LengthUnit.Pixel));
            f.style.top = new StyleLength(new Length(50, LengthUnit.Percent));
            return f;
        }
	}

    void Play()
    {
        if (playing)
        {
            playing = !playing;
            EditorApplication.update -= frameController.PlayAnimation;
            play.text = "Play!";
        }
        else
        {
            frameController.lastFrameTime = Time.realtimeSinceStartup;
            playing = !playing;
            EditorApplication.update += frameController.PlayAnimation;
            play.text = "Stop!";
        }

    }

    public void Disable()
	{

        prev.SetEnabled(false);
        play.SetEnabled(false);
        next.SetEnabled(false);
        prevKey.SetEnabled(false);
        nextKey.SetEnabled(false);
        totalFramesField.text = string.Empty;

        playing = false;
        EditorApplication.update -= frameController.PlayAnimation;
        play.text = "Play!";

        frameController.SetFrame(0);

		foreach (var item in keyingButtons)
		{
            item.Value.SetEnabled(false);
		}
    }

    public void Enable()
	{
        prev.SetEnabled(true);
        play.SetEnabled(true);
        next.SetEnabled(true);
        prevKey.SetEnabled(true);
        nextKey.SetEnabled(true);


        foreach (var item in keyingButtons)
        {
            item.Value.SetEnabled(true);
        }
    }
}