using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Marching : IMarching
{
    public float Surface { get; set; }

    public Vector3Int Ncells { get; set; }

    private Vector4[] Cube { get; set; }

    /// <summary>
    /// Winding order of triangles use 2,1,0 or 0,1,2
    /// </summary>
    protected int[] WindingOrder { get; private set; }

    //protected Vector3 StepSize { get; private set; }

    public Marching(Vector3Int ncells , float surface = 0.5f)
    {
        Ncells = ncells;
        //McMax = mcMax;
        //McMin = mcMin;
        Surface = surface;
        Cube = new Vector4[8];
        WindingOrder = new int[] { 0, 1, 2 };
        //StepSize = McMax - McMin;
        //StepSize = new Vector3(StepSize.x / Ncells.x, StepSize.y / Ncells.y, StepSize.z / Ncells.z);
    }

    public virtual void Generate(IList<Vector4> voxels, IList<Vector3> verts, IList<int> indices)
    {
        if (Surface > 0.0f)
        {
            WindingOrder[0] = 0;
            WindingOrder[1] = 1;
            WindingOrder[2] = 2;
        }
        else
        {
            WindingOrder[0] = 2;
            WindingOrder[1] = 1;
            WindingOrder[2] = 0;
        }

        
        int x, y, z, i;
        int ix, iy, iz;

        
        for (x = 0; x < Ncells.x ; x++)
        {
            for (y = 0; y < Ncells.y ; y++)
            {
                for (z = 0; z < Ncells.z ; z++)
                {
                    //Get the values in the 8 neighbours which make up a cube
                    for (i = 0; i < 8; i++)
                    {
                        ix = x + VertexOffset[i, 0];
                        iy = y + VertexOffset[i, 1];
                        iz = z + VertexOffset[i, 2];

                        Cube[i] = voxels[ix + iy * (Ncells.x + 1) + iz * (Ncells.x + 1) * (Ncells.y + 1)];
                        //Cube[i] = voxels[iz + iy * (Ncells.z + 1) + ix * (Ncells.y + 1) * (Ncells.z + 1)];
                    }

                    //Perform algorithm
                    March(Cube, verts, indices);
                }
            }
        }
    }

    /// <summary>
    /// MarchCube performs the Marching algorithm on a single cube
    /// mcPoint is cube the first point:vertex0
    /// </summary>
    protected abstract void March(Vector4[] cube, IList<Vector3> vertList, IList<int> indexList);

    /// <summary>
    /// GetOffset finds the approximate point of intersection of the surface
    /// between two points with the values v1 and v2
    /// </summary>
    //protected virtual float GetOffset(float v1, float v2)
    //{
    //    float delta = v2 - v1;
    //    return (delta == 0.0f) ? Surface : (Surface - v1) / delta;
    //}

    protected virtual Vector3 LinearInterp(Vector4 p1, Vector4 p2, float surface)
    {
        Vector3 p;
        Vector3 p13 = new Vector3(p1.x, p1.y, p1.z);
        Vector3 p23 = new Vector3(p2.x, p2.y, p2.z);

        if (p1.w != p2.w)
        {
            p = p13 + (p23 - p13) / (p2.w - p1.w) * (surface - p1.w);
        }
        else
        {
            p = p13;
        }
        return p;

    }

    /// <summary>
    /// VertexOffset lists the positions, relative to vertex0, 
    /// of each of the 8 vertices of a cube.
    /// vertexOffset[8][3]
    /// </summary>
    protected static readonly int[,] VertexOffset = new int[,]
    {
            {0, 0, 0},{1, 0, 0},{1, 1, 0},{0, 1, 0},
            {0, 0, 1},{1, 0, 1},{1, 1, 1},{0, 1, 1}
            //{0, 0, 0},{1, 0, 0},{1, 0, 1},{0, 0, 1},
            //{0, 1, 0},{1, 1, 0},{1, 1, 1},{0, 1, 1}
    };



}
