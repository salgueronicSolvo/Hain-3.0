//C# Example (LookAtPointEditor.cs)
using AnythingWorld.Utilities;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ModelDataInspector))]
[CanEditMultipleObjects]
public class ModelDataInspectorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        var t = (target as ModelDataInspector);
        DrawField("Name: ", t.guid);
        DrawField("Author:", t.author);
        DrawField("Entity:", t.entity);
        DrawField("Behaviour:", t.behaviour);
        DrawField("Tags:", t.tags);
        if(t.habitats!=null) DrawField("Habitats:", t.habitats);
        if (t.scales != null)
        {
            foreach (var kvp in t.scales)
            {
                DrawField(System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(kvp.Key) + ":", kvp.Value.ToString() + "m");
            }
        }
        if (t.movement != null)
        {
            foreach (var kvp in t.movement)
            {
                var key = System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(kvp.Key.Replace("_", " "));
                DrawField(key + ":", kvp.Value.ToString() + "m/s");
            }
        }
    }

    public static void DrawField(string label, Vector3 data)
    {
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField(label);
        EditorGUILayout.Vector3Field(label, data);
        EditorGUILayout.EndHorizontal();
    }
    public static void DrawField(string label, string data = "")
    {
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField(label, GUILayout.Width(70));
        EditorGUILayout.LabelField(data);
        EditorGUILayout.EndHorizontal();
    }
    public static void DrawField(string label, string[] data)
    {
        var dataStringList = string.Join(", ", data);
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField(label, GUILayout.Width(70));
        EditorGUILayout.LabelField(dataStringList, GUILayout.ExpandHeight(true));
        EditorGUILayout.EndHorizontal();
    }
}