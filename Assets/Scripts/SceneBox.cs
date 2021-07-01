using System;
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


    //public void UpdateSDF(Transform obj, ObjSdfTable sdfObj)
    //{//加第一个物体 只有平移
    //    Vector3 sizeHalf = sdfObj.Whl / 2.0f;
    //    Vector3 pos = (obj.position - sizeHalf - sceneBox.min) / Constants.Step;
    //    Vector3Int posBegin = new Vector3Int(Mathf.RoundToInt(pos.x), Mathf.RoundToInt(pos.y), Mathf.RoundToInt(pos.z));
    //    Vector3Int posEnd = sdfObj.Ncells + posBegin;

    //    //SdfAssign(posBegin, posEnd, boxMatrix, sdfObj.Objsdf);

    //}

    public void UpdateSDF(MeshFilter objA, MeshFilter objB,BooleanType type,ComputeShader sdfShader)
    {
        Bounds boundsA = objA.GetComponent<Renderer>().bounds;
        Bounds boundsB = objB.GetComponent<Renderer>().bounds;
        Vector3 objAmin = boundsA.min;
        Vector3 objAmax = boundsA.max;
        Vector3 objBmin = boundsB.min;
        Vector3 objBmax = boundsB.max;
        


        //local box min max
        //包围盒总是非常紧凑的，但是对于SDF，我们不希望他这么紧凑，所以要加一个偏值量
        localBoxMin = new Vector3(Mathf.Min(objAmin.x, objBmin.x) - 3 * Constants.Step, Mathf.Min(objAmin.y, objBmin.y) - 3 * Constants.Step, Mathf.Min(objAmin.z, objBmin.z) - 3 * Constants.Step);
        Vector3 localBoxMax = new Vector3(Mathf.Max(objAmax.x, objBmax.x) + 3 * Constants.Step, Mathf.Max(objAmax.y, objBmax.y) + 3 * Constants.Step, Mathf.Max(objAmax.z, objBmax.z) + 3 * Constants.Step);

        Vector3 boxSizef = (localBoxMax - localBoxMin) / Constants.Step;

        ncells = new Vector3Int((int)boxSizef.x, (int)boxSizef.y, (int)boxSizef.z);

        //boxA
        boxMatrix = new float[ncells.x + 1, ncells.y + 1, ncells.z + 1];

        //way1
        /////////////////
        SdfCompute(type, objA.transform, objB.transform, localBoxMin, ncells);


        //wey3
        /////////////////
        //switch (type)
        //{
        //    case BooleanType.Union:
        //        SdfCompute(UnionCompute, objA.transform, objB.transform, localBoxMin, ncells, sdfObjA.Objsdf, sdfObjB.Objsdf);
        //        break;
        //    case BooleanType.Intersection:
        //        SdfCompute(IntersectionCompute, objA.transform, objB.transform, localBoxMin, ncells, sdfObjA.Objsdf, sdfObjB.Objsdf);
        //        break;
        //    case BooleanType.Subtract:
        //        SdfCompute(SubtractCompute, objA.transform, objB.transform, localBoxMin, ncells, sdfObjA.Objsdf, sdfObjB.Objsdf);
        //        break;
        //}


        //way1
        /////////////////
        //{
        //    InitBoxMatrix(boxMatrix, ncells);

        //    //objA in box begin position
        //    Vector3 pos = (objAmin - localBoxMin) / Constants.Step;
        //    Vector3Int posBegin = new Vector3Int(Mathf.RoundToInt(pos.x), Mathf.RoundToInt(pos.y), Mathf.RoundToInt(pos.z));
        //    //Vector3Int posEnd = sdfObjA.Ncells + posBegin;
        //    pos = (objAmax - objAmin) / Constants.Step;
        //    Vector3Int posEnd = new Vector3Int(Mathf.CeilToInt(pos.x), Mathf.CeilToInt(pos.y), Mathf.CeilToInt(pos.z)) + posBegin;

        //    testBen = posBegin;
        //    testEnd = posEnd;
        //    SdfAssign(objA.transform, objAmin, posBegin, posEnd, ref boxMatrix, sdfObjA.Objsdf);


        //    //boxB
        //    float[,,] box = new float[ncells.x + 1, ncells.y + 1, ncells.z + 1];
        //    InitBoxMatrix(box, ncells);

        //    //objB in boxB begin position
        //    pos = (objBmin - localBoxMin) / Constants.Step;
        //    posBegin = new Vector3Int(Mathf.RoundToInt(pos.x), Mathf.RoundToInt(pos.y), Mathf.RoundToInt(pos.z));
        //    //posEnd = sdfObjB.Ncells + posBegin;
        //    pos = (objBmax - objBmin) / Constants.Step;
        //    posEnd = new Vector3Int(Mathf.CeilToInt(pos.x), Mathf.CeilToInt(pos.y), Mathf.CeilToInt(pos.z)) + posBegin;


        //    SdfAssign(objB.transform, objBmin, posBegin, posEnd, ref box, sdfObjB.Objsdf);

        //    SdfCompute(type, box);
        //}


        ////////////
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


    void SdfAssign(in Transform transform,in Vector3 objMin,in Vector3Int posBegin,in Vector3Int posEnd,ref float[,,] boxMatrix)
    {
        ManagerScriptableObject attachScr = transform.GetComponent<AttachScriptable>().Scriptable;
        Texture3D texture3D = attachScr.SDFTexture;
        Bounds bounds = attachScr.Bounds;
        var objsdf = texture3D.GetPixelData<float>(0);
        Vector3Int npoint = attachScr.Size;
        int xyz = npoint.x * npoint.y * npoint.z;

        //Bounds bounds = new Bounds(new Vector3(0,0,0),new Vector3(1,1,1));
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
                        //临近插值（就近插值）
                        //t = (t - bounds.min) / Constants.Step;
                        //Vector3Int ti = new Vector3Int(Mathf.RoundToInt(t.x), Mathf.RoundToInt(t.y), Mathf.RoundToInt(t.z));
                        //int idx = ti.x + ti.y * ixyz.x + ti.z * ixyz.y * ixyz.x;
                        //boxMatrix[i, j, k] = objsdf[idx];
                        //boxMatrix[i, j, k] = texture3D.GetPixelData<RFloat>(0);

                        //三线性插值（还是双线性插值）
                        t = (t - bounds.min) / Constants.Step;
                        Vector3Int tInt = new Vector3Int(Mathf.FloorToInt(t.x), Mathf.FloorToInt(t.y), Mathf.FloorToInt(t.z));
                        Vector3 tDecimal = t - tInt;

                        int idx = tInt.x + tInt.y * npoint.x + tInt.z * npoint.y * npoint.x;

                        if ((idx + 1 + npoint.x + npoint.y * npoint.x)>xyz)
                        {//正确运行的前提可能是限定模型的包围盒要比紧凑的包围盒大一些，三线性插值才不会出错，否则得加这个判断
                            boxMatrix[i, j, k] = objsdf[idx];
                        }
                        else
                        {
                            Vector4 U = new Vector4(objsdf[idx], objsdf[idx + 1], objsdf[idx + npoint.x], objsdf[idx + npoint.x + 1]);
                            idx += npoint.y * npoint.x;
                            Vector4 V = new Vector4(objsdf[idx], objsdf[idx + 1], objsdf[idx + npoint.x], objsdf[idx + npoint.x + 1]);

                            Vector2 u = new Vector2(Mathf.Lerp(U.x, U.y, tDecimal.x), Mathf.Lerp(U.z, U.w, tDecimal.x));
                            float f1 = Mathf.Lerp(u.x, u.y, tDecimal.y);
                            Vector2 v = new Vector2(Mathf.Lerp(V.x, V.y, tDecimal.x), Mathf.Lerp(V.z, V.w, tDecimal.x));
                            float f2 = Mathf.Lerp(v.x, v.y, tDecimal.y);

                            boxMatrix[i, j, k] = Mathf.Lerp(f1, f2, tDecimal.z);
                        }
                    }


                }
            }
        }
    }

    //way1 分别单独更新两张SDF表，然后再做交并补运算
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

    //way2
    //直接做交并补运算，只需要一张SDF表
    void SdfCompute(BooleanType type, in Transform transformA, in Transform transformB, in Vector3 objMin, in Vector3Int ncell)
    {//, in float[] objsdfA, in float[] objsdfB
        ManagerScriptableObject attachScr = transformA.GetComponent<AttachScriptable>().Scriptable;
        Texture3D texture3DA = attachScr.SDFTexture;
        Bounds boundsA = attachScr.Bounds;
        Vector3Int npointA = attachScr.Size;
        var objsdfA = texture3DA.GetPixelData<float>(0);
        

        attachScr = transformB.GetComponent<AttachScriptable>().Scriptable;
        Texture3D texture3DB = attachScr.SDFTexture;
        Bounds boundsB = attachScr.Bounds;
        Vector3Int npointB = attachScr.Size;
        var objsdfB = texture3DB.GetPixelData<float>(0);

        int xyz = npointA.x * npointA.y * npointA.z;

        for (int i = 0; i <= ncell.x; i++)
        {
            for (int j = 0; j <= ncell.y; j++)
            {
                for (int k = 0; k <= ncell.z; k++)
                {
                    Vector3 t0 = objMin + new Vector3(i, j, k) * Constants.Step;

                    Vector3 tA = transformA.InverseTransformPoint(t0);
                    Vector3 tB = transformB.InverseTransformPoint(t0);

                    float tempA = float.MaxValue;
                    {
                        if (boundsA.Contains(tA))
                        {
                            Vector3 tStep = (tA - boundsA.min) / Constants.Step;
                            Vector3Int tInt = new Vector3Int(Mathf.FloorToInt(tStep.x), Mathf.FloorToInt(tStep.y), Mathf.FloorToInt(tStep.z));
                            Vector3 tDecimal = tStep - tInt;

                            int idx = tInt.x + tInt.y * npointA.x + tInt.z * npointA.y * npointA.x;
                            if ((idx + 1 + npointA.x + npointA.y * npointA.x) > xyz)
                            {
                                tempA = objsdfA[idx];
                            }
                            else
                            {
                                Vector4 U = new Vector4(objsdfA[idx], objsdfA[idx + 1], objsdfA[idx + npointA.x], objsdfA[idx + npointA.x + 1]);

                                idx += npointA.y * npointA.x;
                                Vector4 V = new Vector4(objsdfA[idx], objsdfA[idx + 1], objsdfA[idx + npointA.x], objsdfA[idx + npointA.x + 1]);

                                Vector2 u = new Vector2(Mathf.Lerp(U.x, U.y, tDecimal.x), Mathf.Lerp(U.z, U.w, tDecimal.x));
                                float f1 = Mathf.Lerp(u.x, u.y, tDecimal.y);
                                Vector2 v = new Vector2(Mathf.Lerp(V.x, V.y, tDecimal.x), Mathf.Lerp(V.z, V.w, tDecimal.x));
                                float f2 = Mathf.Lerp(v.x, v.y, tDecimal.y);

                                tempA = Mathf.Lerp(f1, f2, tDecimal.z);
                            }
                        }
                    }

                    float tempB = float.MaxValue;
                    {
                        if (boundsB.Contains(tB))
                        {
                            Vector3 tStep = (tB - boundsB.min) / Constants.Step;
                            Vector3Int tInt = new Vector3Int(Mathf.FloorToInt(tStep.x), Mathf.FloorToInt(tStep.y), Mathf.FloorToInt(tStep.z));
                            Vector3 tDecimal = tStep - tInt;

                            int idx = tInt.x + tInt.y * npointB.x + tInt.z * npointB.y * npointB.x;
                            if ((idx + 1 + npointB.x + npointB.y * npointB.z) > idx)
                            {
                                tempB = objsdfB[idx];
                            }
                            else
                            {
                                Vector4 U = new Vector4(objsdfB[idx], objsdfB[idx + 1], objsdfB[idx + npointB.x], objsdfB[idx + npointB.x + 1]);

                                idx += npointB.y * npointB.x;
                                Vector4 V = new Vector4(objsdfB[idx], objsdfB[idx + 1], objsdfB[idx + npointB.x], objsdfB[idx + npointB.x + 1]);

                                Vector2 u = new Vector2(Mathf.Lerp(U.x, U.y, tDecimal.x), Mathf.Lerp(U.z, U.w, tDecimal.x));
                                float f1 = Mathf.Lerp(u.x, u.y, tDecimal.y);
                                Vector2 v = new Vector2(Mathf.Lerp(V.x, V.y, tDecimal.x), Mathf.Lerp(V.z, V.w, tDecimal.x));
                                float f2 = Mathf.Lerp(v.x, v.y, tDecimal.y);

                                tempB = Mathf.Lerp(f1, f2, tDecimal.z);
                            }
                        }
                    }

                    switch (type)
                    {
                        case BooleanType.Intersection:
                            boxMatrix[i, j, k] = Mathf.Max(tempA, tempB);
                            break;
                        case BooleanType.Subtract:
                            boxMatrix[i, j, k] = Mathf.Max(tempA, -tempB);
                            break;
                        case BooleanType.Union:
                            boxMatrix[i, j, k] = Mathf.Min(tempA, tempB);
                            break;
                    }
                }
            }
        }
    }


    //way3 way2的改进？其实也没有什么效率提高，就当学习一下C#委托（相当于C++中的函数指针）
    float UnionCompute(float tempA,float tempB)
    {
        return Mathf.Min(tempA, tempB);
    }

    float SubtractCompute(float tempA, float tempB)
    {
        return Mathf.Max(tempA, -tempB);
    }

    float IntersectionCompute(float tempA, float tempB)
    {
        return Mathf.Max(tempA, tempB);
    }

    delegate float BooleanCompute(float tempA, float tempB);
    void SdfCompute(BooleanCompute booleanCompute, in Transform transformA, in Transform transformB, in Vector3 objMin, in Vector3Int ncell, in float[] objsdfA, in float[] objsdfB)
    {
        ManagerScriptableObject attachScr = transformA.GetComponent<AttachScriptable>().Scriptable;
        Texture3D texture3DA = attachScr.SDFTexture;
        Bounds boundsA = attachScr.Bounds;

        attachScr = transformB.GetComponent<AttachScriptable>().Scriptable;
        Texture3D texture3DB = attachScr.SDFTexture;
        Bounds boundsB = attachScr.Bounds;

        Vector3Int ixyz = new Vector3Int(51, 51, 51);
        int xyz = 51 * 51 * 51;

        for (int i = 0; i <= ncell.x; i++)
        {
            for (int j = 0; j <= ncell.y; j++)
            {
                for (int k = 0; k <= ncell.z; k++)
                {
                    Vector3 t0 = objMin + new Vector3(i, j, k) * Constants.Step;

                    Vector3 tA = transformA.InverseTransformPoint(t0);
                    Vector3 tB = transformB.InverseTransformPoint(t0);

                    float tempA = float.MaxValue;
                    {
                        if (boundsA.Contains(tA))
                        {
                            Vector3 tStep = (tA - boundsA.min) / Constants.Step;
                            Vector3Int tInt = new Vector3Int(Mathf.FloorToInt(tStep.x), Mathf.FloorToInt(tStep.y), Mathf.FloorToInt(tStep.z));
                            Vector3 tDecimal = tStep - tInt;

                            int idx = tInt.x + tInt.y * ixyz.x + tInt.z * ixyz.y * ixyz.x;
                            if ((idx + 1 + ixyz.x + ixyz.y * ixyz.x) > xyz)
                            {
                                tempA = objsdfA[idx];
                            }
                            else
                            {
                                Vector4 U = new Vector4(objsdfA[idx], objsdfA[idx + 1], objsdfA[idx + ixyz.x], objsdfA[idx + ixyz.x + 1]);
                                idx += ixyz.y * ixyz.x;
                                Vector4 V = new Vector4(objsdfA[idx], objsdfA[idx + 1], objsdfA[idx + ixyz.x], objsdfA[idx + ixyz.x + 1]);

                                Vector2 u = new Vector2(Mathf.Lerp(U.x, U.y, tDecimal.x), Mathf.Lerp(U.z, U.w, tDecimal.x));
                                float f1 = Mathf.Lerp(u.x, u.y, tDecimal.y);
                                Vector2 v = new Vector2(Mathf.Lerp(V.x, V.y, tDecimal.x), Mathf.Lerp(V.z, V.w, tDecimal.x));
                                float f2 = Mathf.Lerp(v.x, v.y, tDecimal.y);

                                tempA = Mathf.Lerp(f1, f2, tDecimal.z);
                            }
                        }
                    }

                    float tempB = float.MaxValue;
                    {
                        if (boundsB.Contains(tB))
                        {
                            Vector3 tStep = (tB - boundsB.min) / Constants.Step;
                            Vector3Int tInt = new Vector3Int(Mathf.FloorToInt(tStep.x), Mathf.FloorToInt(tStep.y), Mathf.FloorToInt(tStep.z));
                            Vector3 tDecimal = tStep - tInt;

                            int idx = tInt.x + tInt.y * ixyz.x + tInt.z * ixyz.y * ixyz.x;
                            if ((idx + 1 + ixyz.x + ixyz.y * ixyz.x) > xyz)
                            {
                                tempB = objsdfB[idx];
                            }
                            else
                            {///trilinear inter
                                Vector4 U = new Vector4(objsdfB[idx], objsdfB[idx + 1], objsdfB[idx + ixyz.x], objsdfB[idx + ixyz.x + 1]);
                                idx += ixyz.y * ixyz.x;
                                Vector4 V = new Vector4(objsdfB[idx], objsdfB[idx + 1], objsdfB[idx + ixyz.x], objsdfB[idx + ixyz.x + 1]);

                                Vector2 u = new Vector2(Mathf.Lerp(U.x, U.y, tDecimal.x), Mathf.Lerp(U.z, U.w, tDecimal.x));
                                float f1 = Mathf.Lerp(u.x, u.y, tDecimal.y);
                                Vector2 v = new Vector2(Mathf.Lerp(V.x, V.y, tDecimal.x), Mathf.Lerp(V.z, V.w, tDecimal.x));
                                float f2 = Mathf.Lerp(v.x, v.y, tDecimal.y);

                                tempB = Mathf.Lerp(f1, f2, tDecimal.z);
                            }
                        }
                    }

                    boxMatrix[i, j, k] = booleanCompute(tempA, tempB);
                }
            }
        }
    }


}
