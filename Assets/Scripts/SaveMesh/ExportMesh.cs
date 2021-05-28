//Refer Link : https://blog.csdn.net/liangZG_2011/article/details/51924113

using UnityEngine;
using System.Collections;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;

/// <summary>
/// 描述：导出网格数据到.Obj文件
/// </summary>
public sealed class ExportMesh
{


    public static void ExportMeshToObj(GameObject gameObj,string path)
    {
        //GameObject[] roots = EditorUtil.GetRootGameObjects();

        //bool isSuccess = false;
        //foreach (GameObject gObj in roots)
        //{
        //    if (ExportGameObjectMesh(gObj)) isSuccess = true;
        //}

        //EditorUtility.DisplayDialog("提示", isSuccess ? "已成功导出Mesh到.obj文件" : "当前场景没有找到可导出的资源", "OK");

        bool isSuccess = false;
        if (ExportGameObjectMesh(gameObj, path)) isSuccess = true;
        EditorUtility.DisplayDialog("提示", isSuccess ? "已成功导出Mesh到.obj文件" : "当前场景没有找到可导出的资源", "OK");


    }

    /// <summary>
    /// 导出网格信息
    /// </summary>
    /// <param name="gObj"></param>
    private static bool ExportGameObjectMesh(GameObject gObj,string path)
    {
        MeshFilter[] meshFilters = gObj.GetComponentsInChildren<MeshFilter>();
        if (meshFilters == null || meshFilters.Length <= 0) return false;

        int offsetVertice = 0;
        StringBuilder buf = new StringBuilder();
        foreach (MeshFilter mf in meshFilters)
        {
            offsetVertice += parseMeshFilter(buf, mf, offsetVertice);
        }

        writeObjFile(buf,path);
        return true;
    }

    /// <summary>
    /// 解析MeshFilter数据
    /// </summary>
    /// <param name="buf"></param>
    /// <param name="mf"></param>
    /// <param name="offsetVertices">顶点偏移量</param>
    private static int parseMeshFilter(StringBuilder buf, MeshFilter mf, int offsetVertices)
    {
        Mesh mesh = mf.sharedMesh;
        if (!mesh)
        {
            //Debugger.LogError("<<ExportMesh , parseMeshFilter>> Error !!! Cant find Mesh ! name is " + mf.gameObject.name);
            return 0;
        }
        buf.AppendFormat("# {0}.obj", mf.name);
        buf.AppendLine("#" + System.DateTime.Now.ToLongDateString());
        buf.AppendLine("#" + System.DateTime.Now.ToLongTimeString());
        buf.AppendLine("#-------\n\n");

        buf.AppendFormat("g {0} \n", mf.name);
        Transform meshTrans = mf.transform;

        Vector3[] allVertices = mesh.vertices;
        foreach (Vector3 vertice in allVertices)
        {
            Vector3 v = meshTrans.TransformPoint(vertice);
            buf.AppendFormat("v {0} {1} {2} \n", v.x, v.y, -v.z);
        }


        foreach (Vector3 normal in mesh.normals)
        {
            Vector3 vn = meshTrans.TransformDirection(normal);
            buf.AppendFormat("vn {0} {1} {2} \n", -vn.x, -vn.y, vn.z);
        }

        //foreach (Vector2 uv in mesh.uv)
        //{
        //    buf.AppendFormat("vt {0} {1}\n", uv.x, uv.y);
        //}

        //Material[] mats = meshTrans.GetComponent<Renderer>().sharedMaterials;
        for (int i = 0; i < mesh.subMeshCount; i++)
        {
            //buf.Append("usemtl ").Append(mats[i].name).Append("\n");
            //buf.Append("usemap ").Append(mats[i].name).Append("\n");

            int[] triangles = mesh.GetTriangles(i);
            for (int j = 0; j < triangles.Length; j += 3)
            {
                buf.AppendFormat("f {0}/{0}/{0} {1}/{1}/{1} {2}/{2}/{2}\n",
                    triangles[j] + 1 + offsetVertices, triangles[j + 1] + 1 + offsetVertices, triangles[j + 2] + 1 + offsetVertices);
            }
        }

        return allVertices.Length;
    }

    /// <summary>
    /// 保持OBJ文件
    /// </summary>
    /// <param name="buf"></param>
    private static void writeObjFile(StringBuilder buf,string path)
    {
        string filePath = Path.Combine(path, EditorSceneManager.GetActiveScene().name + ".obj");
        EditorUtil.SwapnDirectory(filePath);

        using (StreamWriter writer = new StreamWriter(filePath))
        {
            writer.Write(buf.ToString());
        }
    }
}
