using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;

public class ZoomViewport : VisualElement
{
    public new class UxmlFactory : UxmlFactory<ZoomViewport, UxmlTraits> { }

    public new class UxmlTraits : VisualElement.UxmlTraits
    {
        public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
        {
            base.Init(ve, bag, cc);

            ZoomViewport zv = ve as ZoomViewport;
            zv.Clear();
            zv.Init();
        }
    }

    //public VisualElement container;
    VisualElement container;
    public float zoom;
    MinMaxSlider horizontal_zoom;
    Scroller vertical;
    Label posLabel;

    public UnityEvent<float, float, float, float> onViewportSet = new();

	public override VisualElement contentContainer => container;

	void Init()
	{
        var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/ScnToolByAeven/Editor/Window/AnimationEditor/ZoomViewport.uxml");
        var template = visualTree.Instantiate();
        template.style.flexGrow = 1f;
        hierarchy.Add(template);

        GetGUIReferences();
        SetCallBacks();
    }
    void GetGUIReferences()
    {
        vertical = this.Q<Scroller>("Vertical");
        horizontal_zoom = this.Q<MinMaxSlider>("Hori_Zoom");
        container = this.Q("Container");
        posLabel = this.Q<Label>("PosLabel");
    }


    void SetCallBacks()
    {
        vertical.valueChanged += (pos) =>
        {
            SetViewport(horizontal_zoom.value.x, horizontal_zoom.value.y, pos);
        };

        horizontal_zoom.RegisterValueChangedCallback((evnt) =>
        {
            //set zoom
            SetViewport(evnt.newValue.x, evnt.newValue.y, vertical.value);
        });

        container.RegisterCallback<WheelEvent>(KeyframeWheel);
        container.RegisterCallback<MouseMoveEvent>((e) => { posLabel.text = e.localMousePosition.x + "x - " + e.localMousePosition.y + "y"; }, TrickleDown.TrickleDown);
        container.RegisterCallback<MouseLeaveEvent>((e) => { posLabel.text = string.Empty; });

        SetKeyframeScrollCallback();
    }

    public void UpdateViewport()
	{
        SetViewport(horizontal_zoom.value.x, horizontal_zoom.value.y, vertical.value);
    }

    void SetViewport(float hMin, float hMax, float vPos)
    {
        float dis = hMax - hMin;


        vertical.Adjust(dis / 100f);

        zoom = 100f / (dis);
        float zoomInfluence = 1f - zoom;

        float zoomPercentage = zoom * 100f;

        onViewportSet.Invoke(zoomPercentage, hMin, vPos, zoomInfluence);
    }

    void SetKeyframeScrollCallback()
	{
        bool scrolling = false;
        container.RegisterCallback<MouseDownEvent>((evnt) =>
        {
            if (evnt.button == 2)
            {
                container.CaptureMouse();
                scrolling = true;
            }
        });
        container.RegisterCallback<MouseMoveEvent>(
            e => {
                if (scrolling)
                {
                    var val = new Vector2(horizontal_zoom.value.x - e.mouseDelta.x * 0.1f, horizontal_zoom.value.y - e.mouseDelta.x * 0.1f);
                    if (val.y < horizontal_zoom.highLimit && val.x > horizontal_zoom.lowLimit)
                    {
                        horizontal_zoom.value = val;
                    }
                    vertical.value -= e.mouseDelta.y * 0.5f;
                }
            }
            );
        container.RegisterCallback<MouseUpEvent>((evnt) =>
        {
            if (evnt.button == 2) { container.ReleaseMouse();
                scrolling = false;
            }
        });
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

}
