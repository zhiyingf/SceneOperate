using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/SpawnManagerScriptableObject", order = 1)]
public class SpawnManagerScriptableObject : ScriptableObject
{
    public Mesh SourceMesh;
    public float BoundsPadding;
    public int Size;
    public int Width;
    public int Height;
    public int Depth;
    public float CellSize;
    //[Header("Bounds")]
    //public Vector3 Center;
    //public Vector3 Extent;
    public Bounds Bounds;
    [SerializeField]
    public Texture3D SDFTexture;

    public void SetTexture()
    {
        const int maxGridSideLength = 32;
        Texture3D initTex = new Texture3D(maxGridSideLength, maxGridSideLength, maxGridSideLength, TextureFormat.RFloat, false); //width height depth


        float[] initState = new float[maxGridSideLength * maxGridSideLength * maxGridSideLength];

        //set the pixels of computeTex
        for (int i = 0; i < maxGridSideLength; i++)
        {
            for (int j = 0; j < maxGridSideLength; j++)
            {
                for (int k = 0; k < maxGridSideLength; k++)
                {
                    var index = k + j * maxGridSideLength + i * maxGridSideLength * maxGridSideLength;
                    initState[index] = 0.0f;
                }
            }
        }
        //public void SetPixelData(T[] data, int mipLevel, int sourceDataStartIndex); This is useful if you want to load compressed or other non-color texture format data into a texture.
        initTex.SetPixelData(initState, 0); //mipLevel 对吗？
        initTex.filterMode = FilterMode.Bilinear;
        initTex.Apply();
        AssetDatabase.CreateAsset(initTex, "Assets/tex3dForDebug.asset");
    }
}