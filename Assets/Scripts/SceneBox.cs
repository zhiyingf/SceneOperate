using System.Collections;
using System.Collections.Generic;
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
        pos = new Vector3(Mathf.Round(pos.x - sdfObj.width / 2.0f), Mathf.Round(pos.y - sdfObj.height / 2.0f), Mathf.Round(pos.z - sdfObj.Lenght / 2.0f));
        Vector3 posBegin = (pos - sceneBox.min) / sdfObj.step;
        Vector3 posEnd = new Vector3(sdfObj.width / sdfObj.step, sdfObj.height / sdfObj.step, sdfObj.Lenght / sdfObj.step);
        posEnd += posBegin;
        int ii = 0, jj = 0, kk = 0;
        for (int i = (int)posBegin.x; i < (int)posEnd.x; i++, ii++)
        {
            for (int j = (int)posBegin.y; i < (int)posEnd.y; j++, jj++)
            {
                for (int k = (int)posBegin.z; i < (int)posEnd.z; k++, kk++)
                {
                    boxMatrix[i, j, k] = sdfObj.objsdf[ii, jj, kk];
                }
            }
        }
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
