using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(SceneSDF))]
public class SceneSDFEditor : Editor
{
    public override void OnInspectorGUI()
    {
        SceneSDF scensdf = target as SceneSDF;
        GUILayout.Space(8.0f);
        scensdf.name = EditorGUILayout.TextField("Name", scensdf.name);
        scensdf.operationType = (BooleanType)EditorGUILayout.EnumPopup("Operation", scensdf.operationType);
        scensdf.operationA = (Transform)EditorGUILayout.ObjectField("OperandA", scensdf.operationA, typeof(Transform), true);

        GUILayout.Space(8.0f);

        if (GUILayout.Button("UpdateMesh"))
        {
            scensdf.ExecuteOnClick();
        }

        if (GUI.changed)
        {
            EditorUtility.SetDirty(target);
        }
    }

}
