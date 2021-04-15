using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UseMC 
{
    //List<GameObject> Meshes = new List<GameObject>();
    public Mesh mesh = new Mesh();
    public Vector3Int Ncell;
    public float[] Voxels;
    public Vector3 McMin;
    public Vector3 McMax;
    public List<Vector3> verts = new List<Vector3>();
    public List<int> indices = new List<int>();

    public UseMC(Vector3Int ncell,Vector3 mcMin,Vector3 mcMax,float[,,] voxels)
    {
        Ncell = ncell;
        McMin = mcMin;
        McMax = mcMax;
        Voxels = new float[Ncell.x * Ncell.y * Ncell.z];
        for(int x = 0; x < Ncell.x; x++)
        {
            for(int y = 0; y < Ncell.y; y++)
            {
                for(int z = 0; z < Ncell.z; z++)
                {
                    int idx = x + y * Ncell.y + z * Ncell.x * Ncell.y;
                    Voxels[idx] = voxels[x, y, z];
                }
            }
        }

        
    }

    public void ComputeMC()
    {
        Marching marching = new MarchingCubes(Ncell, McMax, McMin, 0.0f);
        marching.Generate(Voxels, verts, indices);
        mesh.vertices = verts.ToArray();
        mesh.triangles = indices.ToArray();
    }




}
