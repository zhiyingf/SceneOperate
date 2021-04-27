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
    ObjSdfTable objsdfA;//= new ObjSdfTable(1, 1, 1, 1);
    ObjSdfTable objsdfB;
    public Transform operationA;///???
    public Transform operationB;

    public BooleanType operationType;

    //A lock that controls the execution of a program
    private bool living = false;

    //late state
    private Vector3 positionA;
    private Quaternion rotationA;
    private Vector3 scaleA;

    //BooleanCompute class
    //private BooleanCompute sdfCompute;

    // Start is called before the first frame update
    void Start()
    {
        living = false;

        SB = new SceneBox();
        objsdfA = new ObjSdfTable(1, 1, 1, true);
        objsdfB = new ObjSdfTable(1, 1, 1, true);


        //记录物体最初的位置和状态
        if (operationA != null)
        {
            MeshFilter filterA = operationA.GetComponent<MeshFilter>();
            positionA = filterA.transform.position;
            rotationA = filterA.transform.rotation;
            scaleA = filterA.transform.lossyScale;
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
        if (operationA.transform.position != positionA || operationA.transform.rotation != rotationA || operationA.transform.lossyScale != scaleA)
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
                SB.UpdateSDF(operationA, objsdfA);

            }
        }
        yield return null;
        living = false;
        yield break;
    }


    //marching cube
    public void UpdateMesh()
    {
        SB.InitBoxMatrix(SB.boxMatrix, SB.ncells);
        //SB.UpdateSDF(operationA, objsdfA);
        SB.UpdateSDF(operationA, objsdfA, operationB, objsdfB, operationType);

        ///
        //Vector3 pos = operationA.position;
        //Vector3 sizeHalf = new Vector3(objsdfA.Width / 2.0f, objsdfA.Height / 2.0f, objsdfA.Lenght / 2.0f);
        //Vector3 objAmin = pos - sizeHalf;
        //Vector3 objAmax = pos + sizeHalf;

        //pos = operationB.position;
        //sizeHalf = new Vector3(objsdfB.Width / 2.0f, objsdfB.Height / 2.0f, objsdfB.Lenght / 2.0f);
        //Vector3 objBmin = pos - sizeHalf;
        //Vector3 objBmax = pos + sizeHalf;

        //Vector3 boxMin = new Vector3(Mathf.Min(objAmin.x, objBmin.x), Mathf.Min(objAmin.y, objBmin.y), Mathf.Min(objAmin.z, objBmin.z));
        //Vector3 boxMax = new Vector3(Mathf.Max(objAmax.x, objBmax.x), Mathf.Max(objAmax.y, objBmax.y), Mathf.Max(objAmax.z, objBmax.z));

        //Vector3 boxSizef = (boxMax - boxMin) / objsdfB.Step;
        //Vector3Int boxSize = new Vector3Int((int)boxSizef.x+5, (int)boxSizef.y+5, (int)boxSizef.z+5);
        //print("boxSize: ");
        //print(boxSize);
        

        //float[,,] box = new float[boxSize.x + 1, boxSize.y + 1, boxSize.z + 1];
        //SB.InitBoxMatrix(box, boxSize);

        ////objB in box begin position
        //pos = (objBmin - boxMin) / objsdfB.Step;
        //Vector3Int posBegin = new Vector3Int((int)Mathf.Round(pos.x), (int)Mathf.Round(pos.y), (int)Mathf.Round(pos.z));
        //Vector3Int posEnd = objsdfB.Ncells + posBegin;
        //print("box objB: ");
        //print(posBegin);
        //print(posEnd);

        ///



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
