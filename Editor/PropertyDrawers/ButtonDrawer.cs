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
        ButtonAction ba = (ButtonAction)(fieldInfo.GetValue(property.serializedObject.targetObject));
        EditorGUI.BeginProperty(position, label, property);
        bool button = GUI.Button(position, ba.label);
        EditorGUI.EndProperty();
        if (button)
        {
            ba.action.Invoke();
            GUIUtility.ExitGUI();
        }
        return;
    }
}
