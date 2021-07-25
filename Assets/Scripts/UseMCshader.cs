using System.IO;
using UnityEngine;

public class UseMcShader
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

    /// <summary>
    /// MC 局部更新
    /// </summary>
    /// <param name="SB"></param>
    public UseMcShader(SceneBox SB, ComputeShader mcShader)
    {
        McShader = mcShader;
        Npoint = SB.ncells + Vector3Int.one;
        McMin = SB.localBox.min;
        Voxels = new Vector4[Npoint.x * Npoint.y * Npoint.z];

        var tmp = SB.TexMatrix.GetPixelData<float>(0);

        for (int x = 0; x < Npoint.x; x++)
        {
            for (int y = 0; y < Npoint.y; y++)
            {
                for (int z = 0; z < Npoint.z; z++)
                {
                    Vector3 coord = new Vector3(McMin.x + x * Constants.Step, McMin.y + y * Constants.Step, McMin.z + z * Constants.Step);
                    int idx = x + y * Npoint.x + z * Npoint.y * Npoint.x;
                    //int idx = z + y * Npoint.z + x * Npoint.y * Npoint.z;
                    Voxels[idx] = new Vector4(coord.x, coord.y, coord.z, tmp[idx]);
                }
            }
        }
    }

    public void ComputeMC()
    {
        InitBuffers();

        Vector3Int numThreadsPerAxis = new Vector3Int(Mathf.CeilToInt(Npoint.x / (float)threadGroupSize), Mathf.CeilToInt(Npoint.y / (float)threadGroupSize), Mathf.CeilToInt(Npoint.z / (float)threadGroupSize));

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

        //UInt16: A mesh in unity can only be made up of 65536 verts.
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        mesh.vertices = vertices;
        mesh.triangles = meshTriangles;

        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
    }

    private void InitBuffers()
    {
        int numPoints = Npoint.x * Npoint.y * Npoint.z;
        int numVoxels = (Npoint.x - 1) * (Npoint.y - 1) * (Npoint.z - 1);
        int maxTriangleCount = numVoxels * 5;

        voxelsBuffer = new ComputeBuffer(numPoints, sizeof(float) * 4);//Voxels.Length
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


