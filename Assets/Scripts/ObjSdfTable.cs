using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjSdfTable
{
    public float Width;
    public float Height;
    public float Lenght;
    public float Step = 0.1f;
    public Vector3Int Ncells;
    public float[,,] Objsdf;


    //General SDF for preloaded objects
    public ObjSdfTable(float width, float height, float lenght, float[,,] objsdf)
    {
        Lenght = lenght;
        Width = width;
        Height = height;
        Ncells = new Vector3Int((int)Mathf.Round(Width / Step), (int)Mathf.Round(Height / Step), (int)Mathf.Round(Lenght / Step));
        Objsdf = objsdf;
    }

    //Calculate the SDF of the circle using implicit functions
    public ObjSdfTable(float width, float height, float lenght, bool flag)
    {
        Lenght = lenght;
        Width = width;
        Height = height;
        Ncells = new Vector3Int((int)Mathf.Round(Width / Step), (int)Mathf.Round(Height / Step), (int)Mathf.Round(Lenght / Step));
        Objsdf = new float[Ncells.x+1, Ncells.y+1, Ncells.z+1];
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
                    Objsdf[i, j, k] = v3.magnitude - r/2;
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
                    Objsdf[i, j, k] = qmax.magnitude - Mathf.Min(Mathf.Max(q.x, Mathf.Max(q.y, q.z)), 0.0f);
                }
            }
        }
    }
}
