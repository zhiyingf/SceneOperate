using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

/// <summary>
/// 描述：工具开发常用工具集
/// </summary>
public sealed class EditorUtil
{

    /// <summary>
    /// 获得当前场景的所有根结点
    /// </summary>
    /// <returns></returns>
    public static GameObject[] GetRootGameObjects()
    {
        List<GameObject> goList = new List<GameObject>();
        GameObject[] objArr = Object.FindObjectsOfType<GameObject>();
        foreach (GameObject obj in objArr)
        {
            if (obj.transform.parent == null)
                goList.Add(obj);
        }
        return goList.ToArray();
    }

    /// <summary>
    /// 检测目录是否存在，如果不存在，则创建目录
    /// </summary>
    /// <param name="path"></param>
    public static void SwapnDirectory(string path)
    {
        string dir = Path.GetDirectoryName(path);
        if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
    }
}