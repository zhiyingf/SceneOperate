using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjSdfTable
{
    public float Lenght;
    public float width;
    public float height;
    public float[,,] objsdf;
    public float step;

    //general
    public ObjSdfTable(float Lenght, float width, float height, float[,,] objsdf)
    {
        this.Lenght = Lenght;
        this.width = width;
        this.height = height;
        this.objsdf = objsdf;
    }

    //spere
    public ObjSdfTable(float Lenght, float width, float height, float r)
    {
        this.Lenght = Lenght;
        this.width = width;
        this.height = height;
        ComputeSphereSdf(r);
    }

    public void ComputeSphereSdf(float r)
    {
        float tmp = 1 / step;
        for (int i = 0; i <= (int)tmp * width; i++)
        {
            float vx = step * i;
            for (int j = 0; j <= (int)tmp * height; j++)
            {
                float vy = step * j;
                for (int k = 0; k <= (int)tmp * Lenght; k++)
                {
                    float vz = step * k;
                    Vector3 v3 = new Vector3(vx, vy, vz);
                    objsdf[i, j, k] = v3.magnitude - r;
                }
            }
        }
    }
}
