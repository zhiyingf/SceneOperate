using UnityEngine;
using UnityEditor;
[CustomEditor(typeof(AttachScriptable))]
public class AttachScriptableEditor : Editor
{
    public override void OnInspectorGUI()
    {
        AttachScriptable attachScri = target as AttachScriptable;
        attachScri.Scriptable = (ManagerScriptableObject)EditorGUILayout.ObjectField("Scriptable", attachScri.Scriptable, typeof(ManagerScriptableObject), true);
    }
}
