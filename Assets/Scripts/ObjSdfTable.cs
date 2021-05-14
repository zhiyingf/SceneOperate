using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjSdfTable
{
    //public float Width;
    //public float Height;
    //public float Lenght;
    public Vector3 Whl;
    public float Step = 0.1f;
    public Vector3Int Ncells;
    public float[] Objsdf;


    //General SDF for preloaded objects
    public ObjSdfTable(Vector3 whl)
    {
        Whl = whl;
        Ncells = new Vector3Int((int)Mathf.Round(Whl.x / Step), (int)Mathf.Round(Whl.y / Step), (int)Mathf.Round(Whl.z / Step));
        Objsdf = new float[(Ncells.x + 1) * (Ncells.y + 1) * (Ncells.z + 1)];
        //Objsdf = objsdf;
    }

    //Calculate the SDF of the circle using implicit functions
    public ObjSdfTable(Vector3 whl, bool flag)
    {
        Whl = whl;
        Ncells = new Vector3Int((int)Mathf.Round(Whl.x / Step), (int)Mathf.Round(Whl.y / Step), (int)Mathf.Round(Whl.z / Step));
        Objsdf = new float[(Ncells.x + 1) * (Ncells.y + 1) * (Ncells.z + 1)];
        if(flag) ComputeSphereSdf(1);
        else ComputeBoxSdf(new Vector3(1,1,1));
    }

    //origin point of sphere is (0,0,0)
    public void ComputeSphereSdf(float r)
    {
        Vector3 origin = new Vector3(-r/2, -r/2, -r/2);
        for (int i = 0; i <= Ncells.x; i++)
        {
            float vx = Step * i;
            for (int j = 0; j <= Ncells.y; j++)
            {
                float vy = Step * j;
                for (int k = 0; k <= Ncells.z; k++)
                {
                    float vz = Step * k;
                    Vector3 v3 = new Vector3(vx, vy, vz);
                    v3 += origin;
                    int idx = i * (Ncells.y + 1) * (Ncells.z + 1) + j * (Ncells.z + 1) + k;
                    Objsdf[idx] = v3.magnitude - r/2;
                }
            }
        }
    }

    public void ComputeBoxSdf(Vector3 b)
    {
        Vector3 origin = -b/2;
        for (int i = 0; i <= Ncells.x; i++)
        {
            float vx = Step * i;
            for (int j = 0; j <= Ncells.y; j++)
            {
                float vy = Step * j;
                for (int k = 0; k <= Ncells.z; k++)
                {
                    float vz = Step * k;
                    Vector3 p = new Vector3(vx, vy, vz);
                    p += origin;
                    ///
                    Vector3 q = new Vector3(Mathf.Abs(p.x), Mathf.Abs(p.y), Mathf.Abs(p.z)) - b/2;
                    Vector3 qmax = new Vector3(Mathf.Max(q.x, 0.0f), Mathf.Max(q.y, 0.0f), Mathf.Max(q.z, 0.0f));
                    ///
                    int idx = i * (Ncells.y + 1) * (Ncells.z + 1) + j * (Ncells.z + 1) + k;
                    Objsdf[idx] = qmax.magnitude - Mathf.Min(Mathf.Max(q.x, Mathf.Max(q.y, q.z)), 0.0f);
                }
            }
        }
    }
}
