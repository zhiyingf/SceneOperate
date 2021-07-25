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
    //public float[] boxMatrix;
    public Texture3D TexMatrix;

    //sdf局部更新---MC局部更新
    public Bounds localBox;

    public ComputeShader SdfShader;

    public UseSdfTexShader texShader;

    const int offset = 2;

    public SceneBox(ComputeShader sdfShader)
    {
        //initSceneBox
        center = new Vector3(0, 0, 0);
        size = new Vector3(Constants.Width, Constants.Height, Constants.Lenght);
        sceneBox = new Bounds(center, size);
        localBox = new Bounds();
        SdfShader = sdfShader;
    }

    public void UpdateSDF(MeshFilter objA, MeshFilter objB,BooleanType type)
    {
        Bounds boundsA = objA.GetComponent<Renderer>().bounds;
        Bounds boundsB = objB.GetComponent<Renderer>().bounds;
        Vector3 objAmin = boundsA.min;
        Vector3 objAmax = boundsA.max;
        Vector3 objBmin = boundsB.min;
        Vector3 objBmax = boundsB.max;

        //local box min max
        //包围盒总是非常紧凑的，但是对于SDF，我们不希望他这么紧凑，所以要加一个偏值量
        localBox.min = new Vector3(Mathf.Min(objAmin.x, objBmin.x) - offset * Constants.Step, Mathf.Min(objAmin.y, objBmin.y) - offset * Constants.Step, Mathf.Min(objAmin.z, objBmin.z) - offset * Constants.Step);
        localBox.max = new Vector3(Mathf.Max(objAmax.x, objBmax.x) + offset * Constants.Step, Mathf.Max(objAmax.y, objBmax.y) + offset * Constants.Step, Mathf.Max(objAmax.z, objBmax.z) + offset * Constants.Step);

        Vector3 boxSizef = localBox.size / Constants.Step;

        ncells = new Vector3Int((int)boxSizef.x, (int)boxSizef.y, (int)boxSizef.z);

        //boxA
        //boxMatrix = new float[ncells.x + 1, ncells.y + 1, ncells.z + 1];

        /////use computeshader
        ///
        texShader = new UseSdfTexShader(objA.transform, objB.transform, ncells, localBox.min, SdfShader);
        //texShader.ComputeSDF(sdfShader, type, ref boxMatrix);
        texShader.ComputeSDF(type, ref TexMatrix);

        //UseSdfBufShader bufShader = new UseSdfBufShader(objA.transform, objB.transform, ncells, localBoxMin);
        //bufShader.ComputeSDF(sdfShader, type, ref boxMatrix);
    }

    public void UpdateSDFLater(MeshFilter objB, BooleanType type)
    {
        Bounds old = localBox;
        Vector3 objAmin = old.min;
        Vector3 objAmax = old.max;

        Bounds boundsB = objB.GetComponent<Renderer>().bounds;
        Vector3 objBmin = boundsB.min;
        Vector3 objBmax = boundsB.max;

        localBox.min = new Vector3(Mathf.Min(objAmin.x, objBmin.x) - offset * Constants.Step, Mathf.Min(objAmin.y, objBmin.y) - offset * Constants.Step, Mathf.Min(objAmin.z, objBmin.z) - offset * Constants.Step);
        localBox.max = new Vector3(Mathf.Max(objAmax.x, objBmax.x) + offset * Constants.Step, Mathf.Max(objAmax.y, objBmax.y) + offset * Constants.Step, Mathf.Max(objAmax.z, objBmax.z) + offset * Constants.Step);
        
        Vector3 boxSizef = localBox.size / Constants.Step;

        ncells = new Vector3Int((int)boxSizef.x, (int)boxSizef.y, (int)boxSizef.z);

        /////use computeshader
        texShader.SetSDFLater(TexMatrix, old, objB.transform, ncells, localBox.min);
        texShader.ComputeSDF(type, ref TexMatrix);
    }

}
