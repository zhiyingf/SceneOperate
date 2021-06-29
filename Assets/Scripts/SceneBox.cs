using System.Collections;
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
    public Vector3 localBoxMin;
    public Vector3Int testBen;
    public Vector3Int testEnd;


    public SceneBox()
    {
        //initSceneBox
        center = new Vector3(0, 0, 0);
        size = new Vector3(Constants.Width, Constants.Height, Constants.Lenght);
        sceneBox = new Bounds(center, size);
    }


    public void UpdateSDF(Transform obj, ObjSdfTable sdfObj)
    {//加第一个物体 只有平移
        Vector3 sizeHalf = sdfObj.Whl / 2.0f;
        Vector3 pos = (obj.position - sizeHalf - sceneBox.min) / Constants.Step;
        Vector3Int posBegin = new Vector3Int(Mathf.RoundToInt(pos.x), Mathf.RoundToInt(pos.y), Mathf.RoundToInt(pos.z));
        Vector3Int posEnd = sdfObj.Ncells + posBegin;

        //SdfAssign(posBegin, posEnd, boxMatrix, sdfObj.Objsdf);

    }

    public void UpdateSDF(MeshFilter objA, ObjSdfTable sdfObjA,MeshFilter objB,ObjSdfTable sdfObjB, BooleanType type,ComputeShader sdfShader)
    {
        //加两个物体
        //UpdateSDF(objA, sdfObjA);

        //Vector3 pos = objA.transform.position;
        //Vector3 sizeHalf = sdfObjA.Whl / 2.0f;
        //Vector3 objAmin = pos - sizeHalf;
        //Vector3 objAmax = pos + sizeHalf;

        //pos = objB.transform.position;
        //sizeHalf = sdfObjB.Whl / 2.0f;
        //Vector3 objBmin = pos - sizeHalf;
        //Vector3 objBmax = pos + sizeHalf;

        Bounds boundsA = objA.GetComponent<Renderer>().bounds;
        Bounds boundsB = objB.GetComponent<Renderer>().bounds;
        Vector3 objAmin = boundsA.min;
        Vector3 objAmax = boundsA.max;
        Vector3 objBmin = boundsB.min;
        Vector3 objBmax = boundsB.max;
        Vector3 pos;


        //local box min max
        localBoxMin = new Vector3(Mathf.Min(objAmin.x, objBmin.x) - 2 * Constants.Step, Mathf.Min(objAmin.y, objBmin.y) - 2 * Constants.Step, Mathf.Min(objAmin.z, objBmin.z) - 2 * Constants.Step);
        Vector3 localBoxMax = new Vector3(Mathf.Max(objAmax.x, objBmax.x) + 3 * Constants.Step, Mathf.Max(objAmax.y, objBmax.y) + 3 * Constants.Step, Mathf.Max(objAmax.z, objBmax.z) + 3 * Constants.Step);

        Vector3 boxSizef = (localBoxMax - localBoxMin) / Constants.Step;

        //
        ncells = new Vector3Int((int)boxSizef.x, (int)boxSizef.y, (int)boxSizef.z);

        //boxA
        boxMatrix = new float[ncells.x + 1, ncells.y + 1, ncells.z + 1];
        InitBoxMatrix(boxMatrix, ncells);
        //objA in box begin position
        pos = (objAmin - localBoxMin) / Constants.Step;
        Vector3Int posBegin = new Vector3Int(Mathf.RoundToInt(pos.x), Mathf.RoundToInt(pos.y), Mathf.RoundToInt(pos.z));
        //Vector3Int posEnd = sdfObjA.Ncells + posBegin;
        pos = (objAmax - objAmin) / Constants.Step;
        Vector3Int posEnd = new Vector3Int(Mathf.CeilToInt(pos.x), Mathf.CeilToInt(pos.y), Mathf.CeilToInt(pos.z)) + posBegin;

        testBen = posBegin;
        testEnd = posEnd;
        SdfAssign(objA.transform, objAmin, posBegin, posEnd, ref boxMatrix, sdfObjA.Objsdf);


        //boxB
        float[,,] box = new float[ncells.x + 1, ncells.y + 1, ncells.z + 1];
        InitBoxMatrix(box, ncells);

        //objB in boxB begin position
        pos = (objBmin - localBoxMin) / Constants.Step;
        posBegin = new Vector3Int(Mathf.RoundToInt(pos.x), Mathf.RoundToInt(pos.y), Mathf.RoundToInt(pos.z));
        //posEnd = sdfObjB.Ncells + posBegin;
        pos = (objBmax - objBmin) / Constants.Step;
        posEnd = new Vector3Int(Mathf.CeilToInt(pos.x), Mathf.CeilToInt(pos.y), Mathf.CeilToInt(pos.z)) + posBegin;


        SdfAssign(objB.transform, objBmin, posBegin, posEnd, ref box, sdfObjB.Objsdf);

        SdfCompute(type, box);

        //use sdf shader
        //UseSdfShader useSdf = new UseSdfShader(ncells, sdfShader, boxMatrix, box);
        //useSdf.ComputeSDF(boxMatrix, type);
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


    void SdfAssign(in Transform transform,in Vector3 objMin,in Vector3Int posBegin,in Vector3Int posEnd,ref float[,,] boxMatrix ,in float[] objsdf)
    {
        Vector3Int ixyz = new Vector3Int(51, 51, 51);
        Bounds bounds = new Bounds(new Vector3(0,0,0),new Vector3(1,1,1));
        for (int i = posBegin.x, ii = 0; i <= posEnd.x; i++, ii++)
        {
            for (int j = posBegin.y, jj = 0; j <= posEnd.y; j++, jj++)
            {
                for (int k = posBegin.z, kk = 0; k <= posEnd.z; k++, kk++)
                {
                    Vector3 t = objMin + new Vector3(ii, jj, kk) * Constants.Step;
                    t = transform.InverseTransformPoint(t);

                    if (bounds.Contains(t))
                    {
                        t = (t - bounds.min) / Constants.Step;
                        Vector3Int ti = new Vector3Int(Mathf.RoundToInt(t.x), Mathf.RoundToInt(t.y), Mathf.RoundToInt(t.z));
                        int idx = ti.x + ti.y * ixyz.x + ti.z * ixyz.y * ixyz.x;
                        boxMatrix[i, j, k] = objsdf[idx];
                    }
                    
                    
                }
            }
        }
    }

    void SdfCompute(BooleanType type, float[,,] box)
    {
        if (type == BooleanType.Intersection)
        {
            for (int i = 0; i <= ncells.x; i++)
            {
                for (int j = 0; j <= ncells.y; j++)
                {
                    for (int k = 0; k <= ncells.z; k++)
                    {
                        boxMatrix[i, j, k] = Mathf.Max(boxMatrix[i, j, k], box[i, j, k]);
                    }
                }
            }
        }
        else if (type == BooleanType.Subtract)
        {
            for (int i = 0; i <= ncells.x; i++)
            {
                for (int j = 0; j <= ncells.y; j++)
                {
                    for (int k = 0; k <= ncells.z; k++)
                    {
                        boxMatrix[i, j, k] = Mathf.Max(boxMatrix[i, j, k], -box[i, j, k]);
                    }
                }
            }
        }
        else if (type == BooleanType.Union)
        {
            for (int i = 0; i <= ncells.x; i++)
            {
                for (int j = 0; j <= ncells.y; j++)
                {
                    for (int k = 0; k <= ncells.z; k++)
                    {
                        boxMatrix[i, j, k] = Mathf.Min(boxMatrix[i, j, k], box[i, j, k]);
                    }
                }
            }
        }
    }

}
