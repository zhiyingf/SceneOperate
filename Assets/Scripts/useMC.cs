using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UseMC 
{
    //List<GameObject> Meshes = new List<GameObject>();
    public Mesh mesh = new Mesh();
    public Vector3Int Ncell;
    public Vector4[] Voxels;
    public Vector3 McMin;
    public Vector3 McMax;
    public float Step = 0.1f;
    public List<Vector3> verts = new List<Vector3>();
    public List<int> indices = new List<int>();

    public UseMC(Vector3Int ncell, Vector3 mcMax, Vector3 mcMin,float[,,] voxels)
    {
        Ncell = ncell;
        McMin = mcMin;
        McMax = mcMax;
        Voxels = new Vector4[(Ncell.x+1) * (Ncell.y+1) * (Ncell.z+1)];
        //
        for(int x = 0; x <= Ncell.x; x++)
        {
            for(int y = 0; y <= Ncell.y; y++)
            {
                for(int z = 0; z <= Ncell.z; z++)
                {
                    Vector3 coord = new Vector3(McMin.x + x * Step, McMin.y + y * Step, McMin.z + z * Step);
                    int idx = x + y * (Ncell.y + 1) + z * (Ncell.x + 1) * (Ncell.y + 1);
                    Voxels[idx] = new Vector4(coord.x, coord.y, coord.z, voxels[x, y, z]);
                }
            }
        }

        
    }

    public void ComputeMC()
    {
        Marching marching = new MarchingCubes(Ncell, McMax, McMin, 0.0f);
        marching.Generate(Voxels, verts, indices);
        //mesh.vertices = verts.ToArray();
        //mesh.triangles = indices.ToArray();
        mesh.SetVertices(verts);
        mesh.SetTriangles(indices, 0);
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
    }




}
