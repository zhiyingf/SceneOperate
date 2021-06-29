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
        scenesdf.operationA = (MeshFilter)EditorGUILayout.ObjectField("OperandA", scenesdf.operationA, typeof(MeshFilter), true);
        scenesdf.operationB = (MeshFilter)EditorGUILayout.ObjectField("OperandB", scenesdf.operationB, typeof(MeshFilter), true);

        GUILayout.Space(8.0f);
        scenesdf.McShader = (ComputeShader)EditorGUILayout.ObjectField("McShader", scenesdf.McShader, typeof(ComputeShader), true);
        scenesdf.SdfShader = (ComputeShader)EditorGUILayout.ObjectField("SdfShader", scenesdf.SdfShader, typeof(ComputeShader), true);

        GUILayout.Space(8.0f);

        if (GUILayout.Button("Update Mesh"))
        {
            scenesdf.ExecuteOnClick();
        }


        if (GUILayout.Button("Save Mesh", new GUILayoutOption[0]))//Save Prefab
        {
            //string text = EditorUtility.SaveFilePanelInProject("Save Result", scenesdf.gameObject.name, "prefab", "Please select where do you want to save the result?");
            string text = EditorUtility.SaveFilePanelInProject("Save Result", scenesdf.gameObject.name, "obj", "Please select where do you want to save the result?");

            if (text != null)
            {
                //保存预制体
                //GameObject gameObject = (GameObject)Instantiate(scenesdf.gameObject);
                //DestroyImmediate(gameObject.GetComponent<SceneSDF>());
                //AssetDatabase.Refresh();
                //Mesh sharedMesh = scenesdf.GetComponent<MeshFilter>().sharedMesh;
                //if (!EditorUtility.IsPersistent(sharedMesh))
                //{
                //    if (sharedMesh.name == "")
                //    {
                //        sharedMesh.name = "Result";
                //    }
                //    AssetDatabase.CreateAsset(sharedMesh, text.Replace(".prefab", "_Mesh.asset"));
                //}
                //PrefabUtility.SaveAsPrefabAsset(gameObject, text);
                //AssetDatabase.Refresh();
                //DestroyImmediate(gameObject);


                //保存网格.obj
                GameObject gameObject = (GameObject)Instantiate(scenesdf.gameObject);
                ExportMesh.ExportMeshToObj(gameObject, text);
            }
        }

        if (GUI.changed)
        {
            EditorUtility.SetDirty(target);
        }
    }

}
