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
        McMin = SB.localBoxMin;
        Voxels = new Vector4[Npoint.x * Npoint.y * Npoint.z];

        for (int x = 0; x < Npoint.x; x++)
        {
            for (int y = 0; y < Npoint.y; y++)
            {
                for (int z = 0; z < Npoint.z; z++)
                {
                    Vector3 coord = new Vector3(McMin.x + x * Constants.Step, McMin.y + y * Constants.Step, McMin.z + z * Constants.Step);
                    int idx = x + y * Npoint.x + z * Npoint.y * Npoint.x;
                    //int idx = z + y * Npoint.z + x * Npoint.y * Npoint.z;
                    Voxels[idx] = new Vector4(coord.x, coord.y, coord.z, SB.boxMatrix[x, y, z]);
                }
            }
        }

        WriteSDF("bunnySDF.txt", Voxels);

        //NumToString("bunnySDFstring.txt", Voxels);
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


    public void WriteSDF(string name, Vector4[] Voxels)
    {
        FileStream f = new FileStream(name, FileMode.Create);
        BinaryWriter bw = new BinaryWriter(f);
        int size = Voxels.Length;
        for (int i = 0; i < size; i++)
        {
            //byte[] byArray = BitConverter.GetBytes(sdf[i]);
            //bw.Write(byArray, 0, 4);
            bw.Write(Voxels[i].x);
            bw.Write(Voxels[i].y);
            bw.Write(Voxels[i].z);
            bw.Write(Voxels[i].w);
        }

        bw.Close();

    }

    public void NumToString(string name, Vector4[] Voxels)
    {
        string str = "";

        int size = Voxels.Length;
        for (int i = 0; i < size; i++)
        {
            
            str += Voxels[i].x.ToString() + " ";
            str += Voxels[i].y.ToString() + " ";
            str += Voxels[i].z.ToString() + " ";
            str += Voxels[i].w.ToString() + " ";
        }

        

        //string pathout = "E:\\Users\\zhiyi\\SceneOperate\\" + name;

        StreamWriter sw = new StreamWriter(name, true);
        sw.WriteLine(str);
        sw.Close();
        sw.Dispose();

        //return str;
    }


}
