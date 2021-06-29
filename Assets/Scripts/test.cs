// Calculate the world origin relative to this transform.
using UnityEngine;
using System.Collections;

public class test : MonoBehaviour
{
    public void Update()
    {
        Vector3 relativePoint = transform.InverseTransformPoint(0, 0, 0);
        print(relativePoint);
        if (relativePoint.z > 0)
            print("The world origin is in front of this object");
        else
            print("The world origin is behind of this object");
    }
}