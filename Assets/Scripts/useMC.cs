using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class UseMc 
{
    public Mesh mesh = new Mesh();

    Vector3Int Npoint;//Npoint = ncell + Vector3Int.one;
    Vector4[] Voxels;
    Vector3 McMin;
    List<Vector3> verts = new List<Vector3>();
    List<int> indices = new List<int>();


    /// <summary>
    /// MC 局部更新
    /// </summary>
    /// <param name="SB">SceneBox</param>
    public UseMc(SceneBox SB)
    {
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
                    //Voxels[idx] = new Vector4(coord.x, coord.y, coord.z, SB.boxMatrix[x, y, z]); 
                    Voxels[idx] = new Vector4(coord.x, coord.y, coord.z, tmp[idx]);
                }
            }
        }
    }

    public void ComputeMC()
    {
        Vector3Int Ncell = Npoint - Vector3Int.one;
        Marching marching = new MarchingCubes(Ncell, 0.0f);
        marching.Generate(Voxels, verts, indices);

        //A mesh in unity can only be made up of 65000 verts.
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        mesh.SetVertices(verts);
        mesh.SetTriangles(indices, 0);
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
    }
}
