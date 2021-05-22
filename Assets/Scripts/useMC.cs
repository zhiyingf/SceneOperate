using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UseMC 
{
    public Mesh mesh = new Mesh();
    public Vector3Int Npoint;//Npoint = ncell + Vector3Int.one;
    public Vector4[] Voxels;
    public Vector3 McMin;
    public Vector3 McMax;
    public List<Vector3> verts = new List<Vector3>();
    public List<int> indices = new List<int>();

    /// <summary>
    /// MC 全局更新
    /// </summary>
    /// <param name="ncell"></param>
    /// <param name="mcMax"></param>
    /// <param name="mcMin"></param>
    /// <param name="voxels"></param>
    public UseMC(Vector3Int ncell, Vector3 mcMax, Vector3 mcMin, float[,,] voxels)
    {
        Npoint = ncell + Vector3Int.one;
        McMin = mcMin;
        McMax = mcMax;
        Voxels = new Vector4[Npoint.x * Npoint.y * Npoint.z];
        //
        for(int x = 0; x < Npoint.x; x++)
        {
            for(int y = 0; y < Npoint.y; y++)
            {
                for(int z = 0; z < Npoint.z; z++)
                {
                    Vector3 coord = new Vector3(McMin.x + x * Constants.Step, McMin.y + y * Constants.Step, McMin.z + z * Constants.Step);
                    int idx = x + y * Npoint.x + z * Npoint.y * Npoint.x;
                    Voxels[idx] = new Vector4(coord.x, coord.y, coord.z, voxels[x, y, z]);
                }
            }
        }
    }

    /// <summary>
    /// MC 局部更新
    /// </summary>
    /// <param name="SB">SceneBox</param>
    public UseMC(SceneBox SB)
    {
        Npoint = SB.posEnd - SB.posBegin + Vector3Int.one;
        McMin = SB.localBoxMin;
        McMax = SB.localBoxMax;
        Voxels = new Vector4[Npoint.x * Npoint.y * Npoint.z];

        for (int x = 0, xx = SB.posBegin.x; x < Npoint.x; x++,xx++)
        {
            for (int y = 0, yy = SB.posBegin.y; y < Npoint.y; y++,yy++)
            {
                for (int z = 0, zz = SB.posBegin.z; z < Npoint.z; z++,zz++)
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
        Vector3Int Ncell = Npoint - Vector3Int.one;
        Marching marching = new MarchingCubes(Ncell, McMax, McMin, 0.0f);
        marching.Generate(Voxels, verts, indices);
        mesh.SetVertices(verts);
        mesh.SetTriangles(indices, 0);
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
    }


}
