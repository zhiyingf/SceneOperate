using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

//manage scene box
static class Constants
{
    public const float Lenght = 10f;
    public const float Width = 10f;
    public const float Height = 10f;
    public const float Step = 0.1f;
}


public class SceneBox
{
    public Bounds sceneBox;
    public Vector3 center;
    public Vector3 size;
    public Vector3Int ncells;
    public float[,,] boxMatrix;


    public SceneBox()
    {
        //initSceneBox
        center = new Vector3(0, 0, 0);
        size = new Vector3(Constants.Width, Constants.Height, Constants.Lenght);
        sceneBox = new Bounds(center, size);
        ncells = new Vector3Int((int)Mathf.Round(size.x / Constants.Step), (int)Mathf.Round(size.y / Constants.Step), (int)Mathf.Round(size.z / Constants.Step));
        boxMatrix = new float[ncells.x+1,ncells.y+1,ncells.z+1]; 
        //boxMatrix = new float[100, 100, 100];
        InitBoxMatrix();
    }


    public void UpdateSDF(Transform obj, ObjSdfTable sdfObj)
    {//加第一个物体 只有平移
        Vector3 pos = obj.position;
        pos = new Vector3(pos.x - sdfObj.Width / 2.0f, pos.y - sdfObj.Height / 2.0f, pos.z - sdfObj.Lenght / 2.0f);
        pos = (pos - sceneBox.min) / sdfObj.Step;
        Vector3Int posBegin = new Vector3Int((int)Mathf.Round(pos.x), (int)Mathf.Round(pos.y), (int)Mathf.Round(pos.z));
        Vector3Int posEnd = sdfObj.Ncells + posBegin;

        for (int i = posBegin.x; i <= posEnd.x; i++)
        {
            for (int j = posBegin.y; j <= posEnd.y; j++)
            {
                for (int k = posBegin.z; k <= posEnd.z; k++)
                {
                    boxMatrix[i, j, k] = sdfObj.Objsdf[i - posBegin.x, j - posBegin.y, k - posBegin.z];
                }
            }
        }

    }



    //初始化box为无穷大
    public void InitBoxMatrix()
    {
        for (int i = 0; i <= 100; i++)
        {
            for (int j = 0; j <= 100; j++)
            {
                for (int k = 0; k <= 100; k++)
                {
                    boxMatrix[i, j, k] = float.MaxValue;
                }
            }
        }
    }

}
