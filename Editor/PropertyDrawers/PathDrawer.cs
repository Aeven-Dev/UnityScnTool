using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(PathAttribute))]

public class PathDrawer : PropertyDrawer
{
    // Draw the property inside the given rect
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        string extension = (attribute as PathAttribute).extension;
        if (property.propertyType == SerializedPropertyType.String)
        {
            EditorGUI.BeginProperty(position, label, property);
            Rect textPos = new Rect(position.position, new Vector2(position.width -50f, position.height));
            Rect buttonPos = new Rect(new Vector2(position.x + position.width - 50f, position.y), new Vector2(50f, position.height));
            property.stringValue = EditorGUI.TextField(textPos, label.text, property.stringValue);
            bool button = GUI.Button(buttonPos, "Open");
            EditorGUI.EndProperty();
            if (button)
            {
                property.stringValue = EditorUtility.OpenFilePanel("Select a path", "",extension);

                property.serializedObject.ApplyModifiedProperties();
                GUIUtility.ExitGUI();
            }
        }
        else
        {
            EditorGUI.LabelField(position, label.text, "Use path only with string.");
        }
    }
}
