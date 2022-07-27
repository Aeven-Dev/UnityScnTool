using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
[CustomPropertyDrawer(typeof(ButtonAction))]
public class ButtonDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
		if (property.managedReferenceValue is not ButtonAction)
		{
            EditorGUILayout.LabelField("Not a button action");
            return;
		}

        float height = base.GetPropertyHeight(property, label);
        Rect originalPos = new Rect(position);
        originalPos.height = height;
        Rect buttonPos = new Rect(position);
        buttonPos.y = originalPos.y + originalPos.height;
        buttonPos.height = 25f;

        EditorGUI.PropertyField(originalPos, property,label); 
        ButtonAction ba = (ButtonAction)property.managedReferenceValue;
        if (GUILayout.Button( ba.label))
		{
            ba.action.Invoke();
        }
    }

	public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
	{
		return base.GetPropertyHeight(property, label) + 25f;
	}
}
