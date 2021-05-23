using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UseMCshader
{
    const int threadGroupSize = 8;

    ComputeShader McShader;
    ComputeBuffer voxelsBuffer;
    ComputeBuffer triangleBuffer;
    ComputeBuffer triCountBuffer;

    public Mesh mesh = new Mesh();
    Vector3Int Npoint;//Npoint = ncell + Vector3Int.one;
    Vector4[] Voxels;
    Vector3 McMin;
    Vector3 McMax;

    /// <summary>
    /// MC 局部更新
    /// </summary>
    /// <param name="SB"></param>
    public UseMCshader(SceneBox SB, ComputeShader mcShader)
    {
        McShader = mcShader;
        Npoint = SB.posEnd - SB.posBegin + Vector3Int.one;
        McMin = SB.localBoxMin;
        McMax = SB.localBoxMax;
        Voxels = new Vector4[Npoint.x * Npoint.y * Npoint.z];

        for (int x = 0, xx = SB.posBegin.x; x < Npoint.x; x++, xx++)
        {
            for (int y = 0, yy = SB.posBegin.y; y < Npoint.y; y++, yy++)
            {
                for (int z = 0, zz = SB.posBegin.z; z < Npoint.z; z++, zz++)
                {
                    Vector3 coord = new Vector3(McMin.x + x * Constants.Step, McMin.y + y * Constants.Step, McMin.z + z * Constants.Step);
                    int idx = x + y * Npoint.x + z * Npoint.y * Npoint.x;
                    Voxels[idx] = new Vector4(coord.x, coord.y, coord.z, SB.boxMatrix[xx, yy, zz]);
                }
            }
        }
    }

    public void ComputeMC()
    {
        InitBuffers();

        Vector3Int ncell = Npoint - Vector3Int.one;
        Vector3Int numThreadsPerAxis = new Vector3Int(Mathf.CeilToInt(ncell.x / threadGroupSize), Mathf.CeilToInt(ncell.y / threadGroupSize), Mathf.CeilToInt(ncell.z / threadGroupSize));

        int[] xyzAxis = { Npoint.x, Npoint.y, Npoint.z };
        McShader.SetInts("numPointsXyzAxis", xyzAxis);
        triangleBuffer.SetCounterValue(0);
        McShader.SetBuffer(0, "triangles", triangleBuffer);
        McShader.SetBuffer(0, "points", voxelsBuffer);
        McShader.SetFloat("isoLevel", 0.0f);

        McShader.Dispatch(0, numThreadsPerAxis.x, numThreadsPerAxis.y, numThreadsPerAxis.z);

        // Get number of triangles in the triangle buffer
        ComputeBuffer.CopyCount(triangleBuffer, triCountBuffer, 0);
        int[] triCountArray = { 0 };
        triCountBuffer.GetData(triCountArray);
        int numTris = triCountArray[0];

        // Get triangle data from shader
        Triangle[] tris = new Triangle[numTris];
        triangleBuffer.GetData(tris, 0, 0, numTris);

        ReleaseBuffers();

        //数据处理：将三角面片转化成 mesh
        var vertices = new Vector3[numTris * 3];
        var meshTriangles = new int[numTris * 3];

        for (int i = 0; i < numTris; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                meshTriangles[i * 3 + j] = i * 3 + j;
                vertices[i * 3 + j] = tris[i][j];
            }
        }
        mesh.vertices = vertices;
        mesh.triangles = meshTriangles;

        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
    }

    private void InitBuffers()
    {
        //int numPoints = Npoint.x * Npoint.y * Npoint.z;
        int numVoxels = (Npoint.x - 1) * (Npoint.y - 1) * (Npoint.z - 1);
        int maxTriangleCount = numVoxels * 5;

        voxelsBuffer = new ComputeBuffer(Voxels.Length, sizeof(float) * 4);
        voxelsBuffer.SetData(Voxels);

        triangleBuffer = new ComputeBuffer(maxTriangleCount, sizeof(float) * 3 * 3, ComputeBufferType.Append);

        triCountBuffer = new ComputeBuffer(1, sizeof(int), ComputeBufferType.Raw);
    }

    private void ReleaseBuffers()
    {
        if (triangleBuffer != null)
        {
            triangleBuffer.Release();
            voxelsBuffer.Release();
            triCountBuffer.Release();
        }
    }

    struct Triangle
    {
#pragma warning disable 649 // disable unassigned variable warning
        public Vector3 a;
        public Vector3 b;
        public Vector3 c;

        public Vector3 this[int i]
        {
            get
            {
                switch (i)
                {
                    case 0:
                        return a;
                    case 1:
                        return b;
                    default:
                        return c;
                }
            }
        }
    }

}


