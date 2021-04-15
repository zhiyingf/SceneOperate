using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IMarching
{
    float Surface { get; set; }

    Vector3Int Ncells { get; set; }

    Vector3 McMin { get; set; }

    Vector3 McMax { get; set; }

    void Generate(IList<float> voxels, IList<Vector3> verts, IList<int> indices);
}
