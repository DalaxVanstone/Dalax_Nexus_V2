// ReadOnlyInspector.cs
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

// Attribute to make a field read-only in the Inspector.
// Useful for properties that are set at runtime or derived.
public class ReadOnlyInspectorAttribute : PropertyAttribute { }

// Custom Property Drawer for the ReadOnlyInspectorAttribute.
[CustomPropertyDrawer(typeof(ReadOnlyInspectorAttribute))]
public class ReadOnlyInspectorDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        // Save the current GUI enabled state
        bool wasEnabled = GUI.enabled;
        // Disable GUI
        GUI.enabled = false;
        // Draw the property
        EditorGUI.PropertyField(position, property, label, true);
        // Restore the previous GUI enabled state
        GUI.enabled = wasEnabled;
    }
}
#endif
