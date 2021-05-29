﻿using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class SceneBox
{
    public Bounds sceneBox;
    public Vector3 center;
    public Vector3 size;
    public Vector3Int ncells;
    public float[,,] boxMatrix;

    //sdf局部更新---MC局部更新
    public Vector3Int posBegin;
    public Vector3Int posEnd;
    public Vector3 localBoxMin;
    public Vector3 localBoxMax;


    public SceneBox()
    {
        //initSceneBox
        center = new Vector3(0, 0, 0);
        size = new Vector3(Constants.Width, Constants.Height, Constants.Lenght);
        sceneBox = new Bounds(center, size);
        ncells = new Vector3Int(Mathf.CeilToInt(size.x / Constants.Step), Mathf.CeilToInt(size.y / Constants.Step), Mathf.CeilToInt(size.z / Constants.Step));
        boxMatrix = new float[ncells.x+1,ncells.y+1,ncells.z+1];
        InitBoxMatrix(boxMatrix, ncells);
    }


    public void UpdateSDF(Transform obj, ObjSdfTable sdfObj)
    {//加第一个物体 只有平移
        Vector3 sizeHalf = sdfObj.Whl / 2.0f;
        Vector3 pos = (obj.position - sizeHalf - sceneBox.min) / Constants.Step;
        Vector3Int posBegin = new Vector3Int(Mathf.RoundToInt(pos.x), Mathf.RoundToInt(pos.y), Mathf.RoundToInt(pos.z));
        Vector3Int posEnd = sdfObj.Ncells + posBegin;

        SdfAssign(posBegin, posEnd, boxMatrix, sdfObj.Objsdf);

    }

    public void UpdateSDF(Transform objA, ObjSdfTable sdfObjA,Transform objB,ObjSdfTable sdfObjB, BooleanType type)
    {
        //加两个物体
        UpdateSDF(objA, sdfObjA);

        Vector3 pos = objA.position;
        Vector3 sizeHalf = sdfObjA.Whl / 2.0f;
        Vector3 objAmin = pos - sizeHalf;
        Vector3 objAmax = pos + sizeHalf;

        pos = objB.position;
        sizeHalf = sdfObjB.Whl / 2.0f;
        Vector3 objBmin = pos - sizeHalf;
        Vector3 objBmax = pos + sizeHalf;

        //local box min max
        localBoxMin = new Vector3(Mathf.Min(objAmin.x, objBmin.x) - 2 * Constants.Step, Mathf.Min(objAmin.y, objBmin.y) - 2 * Constants.Step, Mathf.Min(objAmin.z, objBmin.z) - 2 * Constants.Step);
        localBoxMax = new Vector3(Mathf.Max(objAmax.x, objBmax.x) + 2 * Constants.Step, Mathf.Max(objAmax.y, objBmax.y) + 2 * Constants.Step, Mathf.Max(objAmax.z, objBmax.z) + 2 * Constants.Step);

        Vector3 boxSizef = (localBoxMax - localBoxMin) / Constants.Step;
        Vector3Int boxSize = new Vector3Int((int)boxSizef.x, (int)boxSizef.y, (int)boxSizef.z);//bias = 4

        float[,,] box = new float[boxSize.x + 1, boxSize.y + 1, boxSize.z + 1];
        InitBoxMatrix(box, boxSize);

        //objB in box begin position
        pos = (objBmin - localBoxMin) / Constants.Step;
        posBegin = new Vector3Int(Mathf.RoundToInt(pos.x), Mathf.RoundToInt(pos.y), Mathf.RoundToInt(pos.z));
        posEnd = sdfObjB.Ncells + posBegin;

        SdfAssign(posBegin, posEnd, box, sdfObjB.Objsdf);

        //box in sceneBox
        pos = (localBoxMin - sceneBox.min) / Constants.Step;
        posBegin = new Vector3Int(Mathf.RoundToInt(pos.x), Mathf.RoundToInt(pos.y), Mathf.RoundToInt(pos.z));
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


    void SdfAssign(Vector3Int posBegin, Vector3Int posEnd, float[,,] boxMatrix ,float[] objsdf)
    {
        Vector3Int ixyz = posEnd - posBegin + new Vector3Int(1, 1, 1);

        for (int i = posBegin.x, ii = 0; i <= posEnd.x; i++, ii++)
        {
            for (int j = posBegin.y, jj = 0; j <= posEnd.y; j++, jj++)
            {
                for (int k = posBegin.z, kk = 0; k <= posEnd.z; k++, kk++)
                {
                    //int idx = ii * ixyz.y * ixyz.z + jj * ixyz.z + kk;
                    int idx = ii + jj * ixyz.x + kk * ixyz.y * ixyz.x;
                    
                    boxMatrix[i, j, k] = objsdf[idx];
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
