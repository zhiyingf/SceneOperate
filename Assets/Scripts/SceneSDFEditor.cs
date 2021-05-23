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
        GUILayout.Space(8.0f);
        bool executeflag = EditorGUILayout.Toggle("Execute in Editor", scenesdf.isEditor, new GUILayoutOption[0]);
        if (executeflag != scenesdf.isEditor)
        {
            scenesdf.isEditor = executeflag;
            scenesdf.living = false;
            scenesdf.StopAllCoroutines();
            //scenesdf.Init();
        }
        scenesdf.operationType = (BooleanType)EditorGUILayout.EnumPopup("Operation", scenesdf.operationType);
        scenesdf.operationA = (Transform)EditorGUILayout.ObjectField("OperandA", scenesdf.operationA, typeof(Transform), true);
        scenesdf.operationB = (Transform)EditorGUILayout.ObjectField("OperandB", scenesdf.operationB, typeof(Transform), true);
        scenesdf.McShader = (ComputeShader)EditorGUILayout.ObjectField("McShader", scenesdf.McShader, typeof(ComputeShader), true);

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
