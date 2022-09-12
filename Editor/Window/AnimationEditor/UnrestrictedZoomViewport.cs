using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;

class UnrestrictedZoomViewport : VisualElement
{
    public new class UxmlFactory : UxmlFactory<UnrestrictedZoomViewport, UxmlTraits> { }

    public new class UxmlTraits : VisualElement.UxmlTraits
    {
        public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
        {
            base.Init(ve, bag, cc);

            UnrestrictedZoomViewport zv = ve as UnrestrictedZoomViewport;
            zv.Clear();
            zv.Init();
        }
    }

    VisualElement container;
    public float zoom;
    public Vector2 center;
    Label posLabel;

    public UnityEvent<float, Vector2> onViewportSet = new();

    public override VisualElement contentContainer => container;

    void Init()
    {
        var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(AevenScnTool.ScnToolData.RootPath + "Editor/Window/AnimationEditor/UnrestrictedZoomViewport.uxml");
        var template = visualTree.Instantiate();
        template.style.flexGrow = 1f;
        hierarchy.Add(template);

        GetGUIReferences();
        SetCallBacks();
    }
    void GetGUIReferences()
    {
        container = this.Q("Container");
        posLabel = this.Q<Label>("PosLabel");
    }


    void SetCallBacks()
    {
        container.RegisterCallback<WheelEvent>(KeyframeWheel);
        container.RegisterCallback<MouseMoveEvent>((e) => { posLabel.text = e.localMousePosition.x + "x - " + e.localMousePosition.y + "y"; }, TrickleDown.TrickleDown);
        container.RegisterCallback<MouseLeaveEvent>((e) => { posLabel.text = string.Empty; });

        SetKeyframeScrollCallback();
    }

    public void UpdateViewport()
    {
        onViewportSet.Invoke(zoom,center);
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
                    center += e.mouseDelta * zoom;
                }
            }
            );
        container.RegisterCallback<MouseUpEvent>((evnt) =>
        {
            if (evnt.button == 2)
            {
                container.ReleaseMouse();
                scrolling = false;
            }
        });
    }

    void KeyframeWheel(WheelEvent e)
    {
        if (e.ctrlKey)
        {
            zoom += e.delta.y * zoom;
        }
        else if (e.shiftKey)
        {
            center.x += e.delta.y * zoom;
        }
        else
        {
            center.y += e.delta.y * zoom;
        }
    }

}
