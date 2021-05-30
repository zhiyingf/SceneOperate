using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

//main scene
[AddComponentMenu("Scene operation")]
[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[ExecuteInEditMode]
public class SceneSDF : MonoBehaviour
{
    public SceneBox SB;
    ObjSdfTable objsdfA;
    ObjSdfTable objsdfB;
    public Transform operationA;///???
    public Transform operationB;

    public bool isEditor;
    public BooleanType operationType;

    //A lock that controls the execution of a program
    public bool living = false;

    //late state
    private MeshFilter filterA;
    private Vector3 positionA;
    private Quaternion rotationA;
    private Vector3 scaleA;

    private MeshFilter filterB;
    private Vector3 positionB;
    private Quaternion rotationB;
    private Vector3 scaleB;

    private List<Material> mats = new List<Material>();

    public ComputeShader McShader;
    public ComputeShader SdfShader;

    //BooleanCompute class
    //private BooleanCompute sdfCompute;

    // Start is called before the first frame update
    void Start()
    {
        living = false;
        Init();
    }

    /// <summary>
    /// 1.初始化体素场，加载两个物体对应的SDF值
    /// 2.记录两个物体的位置、角度、大致范围
    /// </summary>
    public void Init()
    {
        SB = new SceneBox();

        //McShader = 

        //记录物体最初的位置和状态
        if (operationA != null && operationB != null)
        {
            ///物体包围盒中心在原点
            ///小数处理
            ///max<0.5----0.5----size:1.0 (max<0.5 则 0.0 < size < 1.0)
            ///max>0.5----1.0----size:2.0 (max>0.5 则 1.0 < size < 2.0)

            Vector3 sizeA = operationA.GetComponent<Renderer>().bounds.size;
            Vector3 sizeB = operationB.GetComponent<Renderer>().bounds.size;
            //print(new Vector3(Mathf.Ceil(sizeA.x), Mathf.Ceil(sizeA.y), Mathf.Ceil(sizeA.z)));
            //print(new Vector3(Mathf.Ceil(sizeB.x), Mathf.Ceil(sizeB.y), Mathf.Ceil(sizeB.z)));

            //bunny  sphere
            objsdfA = new ObjSdfTable(new Vector3(Mathf.Ceil(sizeA.x), Mathf.Ceil(sizeA.y), Mathf.Ceil(sizeA.z)));
            objsdfB = new ObjSdfTable(new Vector3(Mathf.Ceil(sizeB.x), Mathf.Ceil(sizeB.y), Mathf.Ceil(sizeB.z)), true);//sphere

            ReadSDF(operationA.name, objsdfA.Objsdf);

            //ReadNormalSDF(operationA.name, objsdfA.NormalSDF);


            //sphere sphere
            //objsdfA = new ObjSdfTable(new Vector3(Mathf.Ceil(sizeA.x), Mathf.Ceil(sizeA.y), Mathf.Ceil(sizeA.z)), true);
            //objsdfB = new ObjSdfTable(new Vector3(Mathf.Ceil(sizeB.x), Mathf.Ceil(sizeB.y), Mathf.Ceil(sizeB.z)), true);//sphere



            filterA = operationA.GetComponent<MeshFilter>();
            positionA = filterA.transform.position;
            rotationA = filterA.transform.rotation;
            scaleA = filterA.transform.lossyScale;

            filterB = operationB.GetComponent<MeshFilter>();
            positionB = filterB.transform.position;
            rotationB = filterB.transform.rotation;
            scaleB = filterB.transform.lossyScale;

            mats.Clear();
            mats.AddRange(operationA.GetComponent<Renderer>().sharedMaterials);
            //mats.AddRange(operationB.GetComponent<Renderer>().sharedMaterials);

            //&& Application.isEditor 
            if (isEditor && Application.isPlaying)
            {
                StartCoroutine(Execute());
            }

        }
    }
    // Update is called once per frame
    public void Update()
    {
        ///作用不大？？？ && Application.isEditor 
        if (!living && isEditor&& Application.isPlaying)
        {
            StartCoroutine(Execute());
        }
    }

    public void ExecuteOnClick()
    {
        UpdateMesh();
    }


    //judge operationObj if changed
    public bool Changed()
    {
        bool changeA = operationA.transform.position != positionA || operationA.transform.rotation != rotationA || operationA.transform.lossyScale != scaleA;
        bool changeB = operationB.transform.position != positionB || operationB.transform.rotation != rotationB || operationB.transform.lossyScale != scaleB;

        //保存两个物体上一次位置、方向、范围的变化
        positionA = filterA.transform.position;
        rotationA = filterA.transform.rotation;
        scaleA = filterA.transform.lossyScale;

        positionB = filterB.transform.position;
        rotationB = filterB.transform.rotation;
        scaleB = filterB.transform.lossyScale;

        if (changeA||changeB)
        {
            return true;
        }
        return false;
    }

    //Execute update SDF
    private IEnumerator Execute()
    {
        living = true;
        //Bounds operationBound = operationA.GetComponent<Renderer>().bounds;
        while (operationA != null && operationB != null && SB.sceneBox.Intersects(operationA.GetComponent<Renderer>().bounds) && SB.sceneBox.Intersects(operationB.GetComponent<Renderer>().bounds))
        {
            if (Changed())         
            {   
                UpdateMesh();
            }
            else
            {
                yield return null;
            }
        }
        yield return null;
        living = false;
        yield break;
    }


    //marching cube
    public void UpdateMesh()
    {
        //计时 注意单位是毫秒
        System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
        stopwatch.Start();

        //SB.UpdateSDF(operationA, objsdfA);
        //NumToString(objsdf.Objsdf, "objsdf.txt");

        SB.UpdateSDF(operationA, objsdfA, operationB, objsdfB, operationType, SdfShader);
        print(operationType.ToString());
        ///
        //Vector3Int Npoint = SB.ncells + Vector3Int.one;
        //print("Npoint " + Npoint);
        //print("Npoint " + Npoint.x * Npoint.y * Npoint.z);
        //print("localBoxMin " + SB.localBoxMin);
        ///


        //MC 局部更新
        //UseMC mc = new UseMC(SB);
        //mc.ComputeMC();
        //GetComponent<MeshFilter>().mesh = mc.mesh;
        //GetComponent<Renderer>().sharedMaterials = mats.ToArray();


        /////////////使用mcshader//////////
        ///

        if (McShader)
        {
            UseMcShader mc = new UseMcShader(SB, McShader);
            mc.ComputeMC();
            GetComponent<MeshFilter>().mesh = mc.mesh;
            GetComponent<Renderer>().sharedMaterials = mats.ToArray();
        }
        else
        {
            print("need compute shader");
        }

        ///




        stopwatch.Stop();
        print("update timer: " + stopwatch.ElapsedMilliseconds);//ElapsedMilliseconds  ElapsedTicks时间刻度

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


    /// <summary>
    /// 从本地读取二进制文件至sdf float[]
    /// </summary>
    /// <param name="name"></param>
    /// <param name="sdf"></param>
    public void ReadSDF(string name, float[] sdf)
    {
        name = "Assets\\SDF\\" + name + "100.txt";
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

            for(int i = 0, j = 0; i < size; i += 4,j++)
            {
                sdf[j] = BitConverter.ToSingle(bb, i);
            }
        }
    }

    public void ReadNormalSDF(string name, Vector3[] normalSDF)
    {
        name = "Assets\\SDF\\" + name + "NormalSDF50.txt";
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
            int size = 3 * sizeof(float) * normalSDF.Length;
            byte[] bb = new byte[size];
            br.Read(bb, 0, size);

            for (int i = 0, j = 0; i < size; i += 12, j++)
            {
                Vector3 v3;
                v3.x = BitConverter.ToSingle(bb, i);
                v3.y = BitConverter.ToSingle(bb, i+4);
                v3.z = BitConverter.ToSingle(bb, i+8);
                normalSDF[j] = v3;
            }
            
        }
    }






}
