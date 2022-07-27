using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ButtonAttribute : PropertyAttribute
{
    public string label;
    public ButtonAttribute(string label) { this.label = label; }
}

[Serializable]
public class ButtonAction
{
    public string label;
    public Action action;
    public ButtonAction(string label, Action action)
	{
        this.label = label;
        this.action = action;

    }
}
