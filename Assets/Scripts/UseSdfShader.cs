using UnityEngine;

public class UseSdfShader
{
    const int threadGroupSize = 8;

    ComputeShader SdfShader;

    ComputeBuffer sdfA;
    ComputeBuffer sdfB;

    Vector3Int Npoint;


    public UseSdfShader(Vector3Int ncell,ComputeShader sdfShader,float[,,] sdfa,float[,,] sdfb)
    {
        Npoint = ncell + Vector3Int.one;
        SdfShader = sdfShader;
        InitBuffers(sdfa, sdfb);
    }

    public void ComputeSDF(float[,,] sdfa, BooleanType type)
    {
        Vector3Int numThreadsPerAxis = new Vector3Int(Mathf.CeilToInt(Npoint.x / (float)threadGroupSize), Mathf.CeilToInt(Npoint.y / (float)threadGroupSize), Mathf.CeilToInt(Npoint.z / (float)threadGroupSize));
        int[] xyzAxis = { Npoint.x, Npoint.y, Npoint.z };

        int kernel = SdfShader.FindKernel(type.ToString());
        SdfShader.SetInts("numPointsXyzAxis", xyzAxis);
        SdfShader.SetBuffer(kernel, "sdfA", sdfA);
        SdfShader.SetBuffer(kernel, "sdfB", sdfB);
        SdfShader.Dispatch(kernel, numThreadsPerAxis.x, numThreadsPerAxis.y, numThreadsPerAxis.z);

        int numPoints = Npoint.x * Npoint.y * Npoint.z;
        float[] tmp = new float[numPoints];
        sdfA.GetData(tmp, 0, 0, numPoints);
        ReleaseBuffers();

        for (int i = 0; i < Npoint.x; i++)
        {
            for (int j = 0; j < Npoint.y; j++)
            {
                for (int k = 0; k < Npoint.z; k++)
                {
                    int idx = i + j * Npoint.x + k * Npoint.y * Npoint.x;
                    sdfa[i, j, k] = tmp[idx];
                }
            }
        }

    }

    private void InitBuffers(float[,,] sdfa, float[,,] sdfb)
    {
        int numPoints = Npoint.x * Npoint.y * Npoint.z;
        float[] tmp1 = new float[numPoints];
        float[] tmp2 = new float[numPoints];
        for (int i = 0; i < Npoint.x; i++)
        {
            for (int j = 0; j < Npoint.y; j++)
            {
                for (int k = 0; k < Npoint.z; k++)
                {
                    int idx = i + j * Npoint.x + k * Npoint.y * Npoint.x;
                    tmp1[idx] = sdfa[i, j, k];
                    tmp2[idx] = sdfb[i, j, k];
                }
            }
        }
        
        sdfA = new ComputeBuffer(numPoints, sizeof(float));
        sdfA.SetData(tmp1);

        sdfB = new ComputeBuffer(numPoints, sizeof(float));
        sdfB.SetData(tmp2);
    }

    private void ReleaseBuffers()
    {
        if (sdfA != null)
        {
            sdfA.Release();
            sdfB.Release();
        }
    }

}
