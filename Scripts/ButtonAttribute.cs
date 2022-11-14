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
    public Action action;
    public ButtonAction( Action action)
	{
        this.action = action;

    }
}
