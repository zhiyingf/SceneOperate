using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IMarching
{
    /// <summary>
    /// Surface is the value of equivalent surface.
    /// </summary>
    float Surface { get; set; }

    /// <summary>
    /// Ncells is the number of cube int the x.y.z axis.
    /// </summary>
    Vector3Int Ncells { get; set; }

    /// <summary>
    /// perform marching cube algorithm
    /// </summary>
    /// <param name="voxels">the cube value</param>
    /// <param name="verts">vertex point of triangle mesh</param>
    /// <param name="indices">indices of the vertex</param>
    void Generate(IList<Vector4> voxels, IList<Vector3> verts, IList<int> indices);
}
