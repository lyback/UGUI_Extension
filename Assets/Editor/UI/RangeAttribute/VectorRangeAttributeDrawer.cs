using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(VectorRangeAttribute))]
public class VectorRangeAttributeDrawer : PropertyDrawer
{
    const int helpHeight = 30;
    const int textHeight = 16;
    VectorRangeAttribute rangeAttribute { get { return (VectorRangeAttribute)attribute; } }
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        Color previous = GUI.color;
        GUI.color = !IsValid(property) ? Color.red : Color.white;
        Rect textFieldPosition = position;
        textFieldPosition.width = position.width;
        textFieldPosition.height = position.height;
        EditorGUI.BeginChangeCheck();
        Vector2 val = EditorGUI.Vector2Field(textFieldPosition, label, property.vector2Value);
        if (EditorGUI.EndChangeCheck())
        {
            if (rangeAttribute.bClamp)
            {
                val.x = Mathf.Clamp(val.x, rangeAttribute.fMinX, rangeAttribute.fMaxX);
                val.y = Mathf.Clamp(val.y, rangeAttribute.fMinY, rangeAttribute.fMaxY);
            }
            property.vector2Value = val;
        }
        Rect helpPosition = position;
        helpPosition.y += 16;
        helpPosition.height = 16;
        DrawHelpBox(helpPosition, property);
        GUI.color = previous;
    }
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        if (!IsValid(property))
        {
            return 32;
        }
        return base.GetPropertyHeight(property, label);
    }
    void DrawHelpBox(Rect position, SerializedProperty prop)
    {
        // No need for a help box if the pattern is valid.
        if (IsValid(prop))
            return;

        EditorGUI.HelpBox(position, string.Format("Invalid Range X [{0}]-[{1}] Y [{2}]-[{3}]", rangeAttribute.fMinX, rangeAttribute.fMaxX, rangeAttribute.fMinY, rangeAttribute.fMaxY), MessageType.Error);
    }
    bool IsValid(SerializedProperty prop)
    {
        Vector2 vector = prop.vector2Value;
        return vector.x >= rangeAttribute.fMinX && vector.x <= rangeAttribute.fMaxX && vector.y >= rangeAttribute.fMinY && vector.y <= rangeAttribute.fMaxY;
    }
}