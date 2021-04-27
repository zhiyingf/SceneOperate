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
        InitBoxMatrix(boxMatrix, ncells);
    }


    public void UpdateSDF(Transform obj, ObjSdfTable sdfObj)
    {//加第一个物体 只有平移
        Vector3 pos = obj.position;
        pos = new Vector3(pos.x - sdfObj.Width / 2.0f, pos.y - sdfObj.Height / 2.0f, pos.z - sdfObj.Lenght / 2.0f);
        pos = (pos - sceneBox.min) / sdfObj.Step;
        Vector3Int posBegin = new Vector3Int((int)Mathf.Round(pos.x), (int)Mathf.Round(pos.y), (int)Mathf.Round(pos.z));
        Vector3Int posEnd = sdfObj.Ncells + posBegin;

        SdfAssign(posBegin, posEnd, boxMatrix, sdfObj.Objsdf);

    }

    public void UpdateSDF(Transform objA, ObjSdfTable sdfObjA,Transform objB,ObjSdfTable sdfObjB, BooleanType type)
    {
        //加两个物体
        UpdateSDF(objA, sdfObjA);

        Vector3 pos = objA.position;
        Vector3 sizeHalf = new Vector3(sdfObjA.Width / 2.0f, sdfObjA.Height / 2.0f, sdfObjA.Lenght / 2.0f);
        Vector3 objAmin = pos - sizeHalf;
        Vector3 objAmax = pos + sizeHalf;

        pos = objB.position;
        sizeHalf = new Vector3(sdfObjB.Width / 2.0f, sdfObjB.Height / 2.0f, sdfObjB.Lenght / 2.0f);
        Vector3 objBmin = pos - sizeHalf;
        Vector3 objBmax = pos + sizeHalf;

        Vector3 boxMin = new Vector3(Mathf.Min(objAmin.x, objBmin.x), Mathf.Min(objAmin.y, objBmin.y), Mathf.Min(objAmin.z, objBmin.z));
        Vector3 boxMax = new Vector3(Mathf.Max(objAmax.x, objBmax.x), Mathf.Max(objAmax.y, objBmax.y), Mathf.Max(objAmax.z, objBmax.z));

        Vector3 boxSizef = (boxMax - boxMin) / sdfObjB.Step;
        Vector3Int boxSize = new Vector3Int((int)boxSizef.x+5, (int)boxSizef.y+5, (int)boxSizef.z+5);

        float[,,] box = new float[boxSize.x + 1, boxSize.y + 1, boxSize.z + 1];
        InitBoxMatrix(box, boxSize);

        //objB in box begin position
        pos = (objBmin - boxMin) / sdfObjA.Step;
        Vector3Int posBegin = new Vector3Int((int)Mathf.Round(pos.x), (int)Mathf.Round(pos.y), (int)Mathf.Round(pos.z));
        Vector3Int posEnd = sdfObjB.Ncells + posBegin;

        SdfAssign(posBegin, posEnd, box, sdfObjB.Objsdf);

        //box in sceneBox
        pos = (boxMin - sceneBox.min) / sdfObjA.Step;
        posBegin = new Vector3Int((int)Mathf.Round(pos.x), (int)Mathf.Round(pos.y), (int)Mathf.Round(pos.z));
        posEnd = boxSize + posBegin;

        SdfCompute(posBegin, posEnd, type, box);


    }



    //初始化box为无穷大
    public void InitBoxMatrix(float[,,] box, Vector3Int size)
    {
        for(int i = 0; i<=size.x; i++)
        {
            for(int j = 0; j <= size.y; j++)
            {
                for(int k = 0; k <= size.z; k++)
                {
                    box[i, j, k] = float.MaxValue;
                }
            }
        }
    }


    void SdfAssign(Vector3Int posBegin, Vector3Int posEnd, float[,,] boxMatrix ,float[,,] objsdf)
    {
        for (int i = posBegin.x, ii = 0; i <= posEnd.x; i++, ii++)
        {
            for (int j = posBegin.y, jj = 0; j <= posEnd.y; j++, jj++)
            {
                for (int k = posBegin.z, kk = 0; k <= posEnd.z; k++, kk++)
                {
                    boxMatrix[i, j, k] = objsdf[ii, jj, kk];
                }
            }
        }
    }

    void SdfCompute(Vector3Int posBegin, Vector3Int posEnd, BooleanType type, float[,,] box)
    {
        if (type == BooleanType.Intersection)
        {
            for (int i = posBegin.x, ii = 0; i <= posEnd.x; i++, ii++)
            {
                for (int j = posBegin.y, jj = 0; j <= posEnd.y; j++, jj++)
                {
                    for (int k = posBegin.z, kk = 0; k <= posEnd.z; k++, kk++)
                    {
                        boxMatrix[i, j, k] = Mathf.Max(boxMatrix[i, j, k], box[ii, jj, kk]);
                    }
                }
            }
        }
        else if (type == BooleanType.Subtract)
        {
            for (int i = posBegin.x, ii = 0; i <= posEnd.x; i++, ii++)
            {
                for (int j = posBegin.y, jj = 0; j <= posEnd.y; j++, jj++)
                {
                    for (int k = posBegin.z, kk = 0; k <= posEnd.z; k++, kk++)
                    {
                        boxMatrix[i, j, k] = Mathf.Max(boxMatrix[i, j, k], -box[ii, jj, kk]);
                    }
                }
            }
        }
        else if (type == BooleanType.Union)
        {
            for (int i = posBegin.x, ii = 0; i <= posEnd.x; i++, ii++)
            {
                for (int j = posBegin.y, jj = 0; j <= posEnd.y; j++, jj++)
                {
                    for (int k = posBegin.z, kk = 0; k <= posEnd.z; k++, kk++)
                    {
                        boxMatrix[i, j, k] = Mathf.Min(boxMatrix[i, j, k], box[ii, jj, kk]);
                    }
                }
            }
        }
    }

}
