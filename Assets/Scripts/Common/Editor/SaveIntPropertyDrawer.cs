using UnityEngine;
using System;

#if UNITY_EDITOR
using UnityEditor;
[CustomPropertyDrawer(typeof(SaveInt))]
public class SaveIntPropertyDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

        var left = position; left.xMax -= 40;
        var right = position; right.xMin = left.xMax + 2;

        var value = property.FindPropertyRelative("value");
        EditorGUI.PropertyField(left, value, GUIContent.none);

        if (GUI.Button(right, "Save"))
        {
            var key = property.FindPropertyRelative("key").stringValue;
            if (string.IsNullOrEmpty(key) == false)
            {
                PlayerPrefs.SetInt(key, value.intValue);
                PlayerPrefs.Save();
            }
        }

        EditorGUI.EndProperty();
    }
}
#endif