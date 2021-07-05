using System;
using System.IO;
using UnityEditor;
using UnityEngine;

public class SetScriptableObj : MonoBehaviour
{
    private void Start()
    {
        SetTexture();
    }

    // Start is called before the first frame update
    void SetTexture()
    {
        String path = @"E:\Users\zhiyi\SceneOperate\Assets\SDF\100";
        //第二种方法
        DirectoryInfo folder = new DirectoryInfo(path);

        int i = 0;
        foreach (FileInfo file in folder.GetFiles("*.txt"))
        {
            i++;
            string sdfName = file.Name;
            const int maxGridSideLength = 101;//51
            Vector3Int ncell = new Vector3Int(maxGridSideLength, maxGridSideLength, maxGridSideLength);
            float[] sdf = new float[(ncell.x + 1) * (ncell.x + 1) * (ncell.x + 1)];
            ReadSDF(sdfName, sdf);

            string assetName = sdfName.Substring(0, sdfName.Length - 4);

            print("assetName " + i + " " + assetName);
            Texture3D texture = new Texture3D(maxGridSideLength, maxGridSideLength, maxGridSideLength, TextureFormat.RFloat, false);
            texture.filterMode = FilterMode.Bilinear;
            texture.SetPixelData(sdf, 0);
            //texture.Apply();//??
            string texName = "Assets/source/Texture3D/" + assetName + ".asset";
            AssetDatabase.CreateAsset(texture, texName);

            Bounds bounds = new Bounds(new Vector3(0, 0, 0), new Vector3(1, 1, 1));

            ManagerScriptableObject myScr = ScriptableObject.CreateInstance<ManagerScriptableObject>();
            myScr.SetValue(ncell, bounds, texture);
            string srcName = "Assets/source/ScriptableObj/" + assetName + ".asset";
            AssetDatabase.CreateAsset(myScr, srcName);
        }


    }


    public void ReadSDF(string name, float[] sdf)
    {
        name = "Assets\\SDF\\100\\" + name;// + "-50.txt";
        //print(System.IO.Directory.GetCurrentDirectory());
        if (!File.Exists(name))
        {
            print(name + " not exist");
            return;
        }

        FileStream f = new FileStream(name, FileMode.Open,
        FileAccess.Read, FileShare.Read);
        // Create an instance of BinaryReader that can
        // read bytes from the FileStream.
        using (BinaryReader br = new BinaryReader(f))
        {
            int size = sizeof(float) * sdf.Length;
            byte[] bb = new byte[size];
            br.Read(bb, 0, size);

            for (int i = 0, j = 0; i < size; i += 4, j++)
            {
                sdf[j] = BitConverter.ToSingle(bb, i);
            }
        }
    }
}
