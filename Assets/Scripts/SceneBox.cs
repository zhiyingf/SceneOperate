using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

//manage scene box
static class Constants
{
    public const float Lenght = 10.0f;
    public const float Width = 10.0f;
    public const float Height = 10.0f;
    public const float Step = 0.1f;
}


public class SceneBox
{
    public Bounds sceneBox;
    public Vector3 center;
    public Vector3 size;
    public float[,,] boxMatrix;


    public SceneBox()
    {
        //initSceneBox
        center = new Vector3(0, 0, 0);
        size = new Vector3(Constants.Lenght, Constants.Width, Constants.Height);
        sceneBox = new Bounds(center, size);
        boxMatrix = new float[100, 100, 100];
        InitBoxMatrix();
    }


    public void UpdateSDF(Transform obj, ObjSdfTable sdfObj)
    {//加第一个物体 只有平移
        Vector3 pos = obj.position;
        pos = new Vector3(pos.x - sdfObj.width / 2.0f, pos.y - sdfObj.height / 2.0f, pos.z - sdfObj.Lenght / 2.0f);
        Vector3 posBegin = (pos - sceneBox.min) / sdfObj.step;
        posBegin = new Vector3(Mathf.Round(posBegin.x), Mathf.Round(posBegin.y), Mathf.Round(posBegin.z));
        Vector3 posEnd = new Vector3(sdfObj.width / sdfObj.step, sdfObj.height / sdfObj.step, sdfObj.Lenght / sdfObj.step);
        posEnd += posBegin;

        for (int i = (int)posBegin.x; i < (int)posEnd.x; i++)
        {
            for (int j = (int)posBegin.y; j < (int)posEnd.y; j++)
            {
                for (int k = (int)posBegin.z; k < (int)posEnd.z; k++)
                {
                    boxMatrix[i, j, k] = sdfObj.objsdf[i - (int)posBegin.x, j - (int)posBegin.y, k - (int)posBegin.z];
                }
            }
        }

        //for(int i = 0; i < 10; i++)
        //{
        //    for(int j = 0; j < 10; j++)
        //    {
        //        for(int k = 0; k < 10; k++)
        //        {
        //            boxMatrix[i, j, k] = sdfObj.objsdf[i, j, k];
        //        }
        //    }
        //}
    }



    //初始化box为无穷大
    public void InitBoxMatrix()
    {
        for (int i = 0; i < 100; i++)
        {
            for (int j = 0; j < 100; j++)
            {
                for (int k = 0; k < 100; k++)
                {
                    boxMatrix[i, j, k] = float.MaxValue;
                }
            }
        }
    }

}
