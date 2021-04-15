using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Marching : IMarching
{
    public float Surface { get; set; }

    public Vector3Int Ncells { get; set; }

    public Vector3 McMin { get; set; }

    public Vector3 McMax { get; set; }

    private float[] Cube { get; set; }

    protected int[] WindingOrder { get; private set; }

    protected Vector3 StepSize { get; private set; }

    public Marching(Vector3Int ncells , Vector3 mcMax , Vector3 mcMin , float surface = 0.5f)
    {
        Ncells = ncells;
        McMax = mcMax;
        McMin = mcMin;
        Surface = surface;
        Cube = new float[8];
        WindingOrder = new int[] { 0, 1, 2 };
        StepSize = McMax - McMin;
        StepSize = new Vector3(StepSize.x / Ncells.x, StepSize.y / Ncells.y, StepSize.z / Ncells.z);
    }

    public virtual void Generate(IList<float> voxels, IList<Vector3> verts, IList<int> indices)
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

        //-1? -0?
        for (x = 0; x < Ncells.x - 1 ; x++)
        {
            for (y = 0; y < Ncells.y - 1 ; y++)
            {
                for (z = 0; z < Ncells.z - 1 ; z++)
                {
                    //Get the values in the 8 neighbours which make up a cube
                    for(i = 0; i < 8; i++)
                    {
                        ix = x + VertexOffset[i, 0];
                        iy = y + VertexOffset[i, 1];
                        iz = z + VertexOffset[i, 2];

                        Cube[i] = voxels[ix + iy * Ncells.x + iz * Ncells.x * Ncells.y];
                    }

                    Vector3 mcFirstPoint = new Vector3(McMin.x + StepSize.x * x, McMin.y + StepSize.y * y, McMin.z + StepSize.z * z);
                    //Perform algorithm
                    March(mcFirstPoint, Cube, verts, indices);
                }
            }
        }
    }


    protected abstract void March(Vector3 mcPoint, float[] cube, IList<Vector3> vertList, IList<int> indexList);


    protected virtual float GetOffset(float v1, float v2)
    {
        float delta = v2 - v1;
        return (delta == 0.0f) ? Surface : (Surface - v1) / delta;
    }

    protected static readonly int[,] VertexOffset = new int[,]
    {
            {0, 0, 0},{1, 0, 0},{1, 1, 0},{0, 1, 0},
            {0, 0, 1},{1, 0, 1},{1, 1, 1},{0, 1, 1}
    };



}
