using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

//main scene
[AddComponentMenu("Scene operation")]
[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[ExecuteInEditMode]
public class SceneSDF : MonoBehaviour
{
    public SceneBox SB;//= new SceneBox();
    ObjSdfTable objsdf;//= new ObjSdfTable(1, 1, 1, 1);
    public Transform operationA;///???
    //public Transform operationB;
    public BooleanType operationType;

    //A lock that controls the execution of a program
    private bool living = false;

    //late state
    private Vector3 position;
    private Quaternion rotation;
    private Vector3 scale;

    //BooleanCompute class
    //private BooleanCompute sdfCompute;

    // Start is called before the first frame update
    void Start()
    {
        living = false;

        SB = new SceneBox();
        objsdf = new ObjSdfTable(1, 1, 1, 1);


        //记录物体最初的位置和状态
        if (operationA != null)
        {
            MeshFilter filterA = operationA.GetComponent<MeshFilter>();
            position = filterA.transform.position;
            rotation = filterA.transform.rotation;
            scale = filterA.transform.lossyScale;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Application.isEditor && !Application.isPlaying)
        {
            if (!living)
            {
                //StartCoroutine(Execute());
            }
        }
    }

    public void ExecuteOnClick()
    {
        UpdateMesh();
    }


    //judge operationObj if changed
    public bool Changed()
    {
        if (operationA.transform.position != position || operationA.transform.rotation != rotation || operationA.transform.lossyScale != scale)
        {
            return true;
        }
        return false;
    }

    //Execute update SDF
    public IEnumerator Execute()
    {
        living = true;
        //Bounds operationBound = operationA.GetComponent<Renderer>().bounds;
        while (operationA != null && SB.sceneBox.Intersects(operationA.GetComponent<Renderer>().bounds))
        {
            if (Changed())
            {   //update SDF???
                SB.UpdateSDF(operationA, objsdf);

            }
        }
        yield return null;
        living = false;
        yield break;
    }


    //marching cube
    public void UpdateMesh()
    {
        GetComponent<MeshFilter>().mesh.Clear();
        SB.UpdateSDF(operationA, objsdf);

        
        //NumToString(objsdf.Objsdf, "objsdf.txt");

        UseMC mc = new UseMC(SB.ncells, SB.sceneBox.max, SB.sceneBox.min, SB.boxMatrix);

        mc.ComputeMC();
        GetComponent<MeshFilter>().mesh = mc.mesh;

        //print(NumToString(SB.boxMatrix));
        //NumToString(SB.boxMatrix, "boxMatrix.txt");
    }

    //把每一个数取出来转化为字符串
    public void NumToString(float[,,] list, string fileName)
    {
        string str = "";
        //foreach (float n in list)
        //    str += n.ToString() + " ";
        for(int i = 0; i <= 10; i++)
        {
            for(int j = 0;j <= 10; j++)
            {
                for(int k = 0;k <= 10; k++)
                {
                    str += list[i,j,k].ToString() + " ";
                }
            }
        }

        string pathout = "E:\\Users\\zhiyi\\SceneOperate\\"+fileName;

        StreamWriter sw = new StreamWriter(pathout, true);
        sw.WriteLine(str);
        sw.Close();
        sw.Dispose();

        //return str;
    }

}
