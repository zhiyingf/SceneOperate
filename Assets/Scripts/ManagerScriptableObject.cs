using UnityEngine;

[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/SpawnManagerScriptableObject", order = 1)]
public class ManagerScriptableObject : ScriptableObject
{
    //public Mesh SourceMesh;
    //public float BoundsPadding;
    public Vector3Int Size;
    public Bounds Bounds;
    public Texture3D SDFTexture;
    public void SetValue(Vector3Int size, Bounds bounds, Texture3D texture)
    {
        Size = size;
        Bounds = bounds;
        SDFTexture = texture;
    }
}