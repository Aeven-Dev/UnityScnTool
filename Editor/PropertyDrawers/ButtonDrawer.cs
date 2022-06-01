using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
[CustomPropertyDrawer(typeof(ButtonAttribute))]
public class ButtonDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        ButtonAttribute button = (attribute as ButtonAttribute);
        float height = base.GetPropertyHeight(property, label);
        Rect originalPos = new Rect(position);
        originalPos.height = height;
        Rect buttonPos = new Rect(position);
        buttonPos.y = originalPos.y + originalPos.height;
        buttonPos.height = 25f;

        EditorGUI.PropertyField(originalPos, property,label);
		if (GUI.Button(buttonPos, button.label))
		{

		}
    }

	public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
	{
		return base.GetPropertyHeight(property, label) + 25f;
	}
}
