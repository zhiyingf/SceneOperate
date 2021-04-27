using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(SceneSDF))]
public class SceneSDFEditor : Editor
{
    public override void OnInspectorGUI()
    {
        SceneSDF scenesdf = target as SceneSDF;
        GUILayout.Space(8.0f);
        scenesdf.name = EditorGUILayout.TextField("Name", scenesdf.name);
        scenesdf.operationType = (BooleanType)EditorGUILayout.EnumPopup("Operation", scenesdf.operationType);
        scenesdf.operationA = (Transform)EditorGUILayout.ObjectField("OperandA", scenesdf.operationA, typeof(Transform), true);
        scenesdf.operationB = (Transform)EditorGUILayout.ObjectField("OperandB", scenesdf.operationB, typeof(Transform), true);

        GUILayout.Space(8.0f);

        if (GUILayout.Button("UpdateMesh"))
        {
            scenesdf.ExecuteOnClick();
        }

        if (GUI.changed)
        {
            EditorUtility.SetDirty(target);
        }
    }

}
