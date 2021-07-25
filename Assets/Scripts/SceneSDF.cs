using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

//main scene
[AddComponentMenu("SceneOperate/Scene Operation")]
[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[ExecuteInEditMode]
public class SceneSDF : MonoBehaviour
{
    public SceneBox SB;

    public MeshFilter operationA;
    public MeshFilter operationB;

    public bool isEditor;
    public BooleanType operationType;

    //A lock that controls the execution of a program
    public bool living = false;

    public ComputeShader McShader;
    public ComputeShader SdfShader;

    //late state
    private string nameA;
    private Vector3 positionA;
    private Quaternion rotationA;
    private Vector3 scaleA;

    private string nameB;
    private Vector3 positionB;
    private Quaternion rotationB;
    private Vector3 scaleB;

    private List<Material> mats = new List<Material>();

    // Start is called before the first frame update
    void Start()
    {
        living = false;
        Init();
    }

    //use ref/out
    public void ObjAssign(in MeshFilter operation,ref string name, ref Vector3 position, ref Quaternion rotation, ref Vector3 scale)
    {
        name = operation.name;

        ///物体包围盒中心在原点
        ///小数处理
        ///max<0.5----0.5----size:1.0 (max<0.5 则 0.0 < size < 1.0)
        ///max>0.5----1.0----size:2.0 (max>0.5 则 1.0 < size < 2.0)
        ///
        //Vector3 size = operation.GetComponent<Renderer>().bounds.size;
        //objsdf = new ObjSdfTable(new Vector3(Mathf.Ceil(size.x), Mathf.Ceil(size.y), Mathf.Ceil(size.z)));
        //print("Bounds" + " name" + new Vector3(Mathf.Ceil(size.x), Mathf.Ceil(size.y), Mathf.Ceil(size.z)));
        //ReadSDF(operation.name, objsdf.Objsdf);

        position = operation.transform.position;
        rotation = operation.transform.rotation;
        scale = operation.transform.lossyScale;

        mats.Clear();
        mats.AddRange(operationA.GetComponent<Renderer>().sharedMaterials);
    }

    /// <summary>
    /// 1.初始化体素场，加载两个物体对应的SDF值
    /// 2.记录两个物体的位置、角度、大致范围
    /// </summary>
    public void Init()
    {
        SB = new SceneBox(SdfShader);
        if (operationA != null)
        {
            ObjAssign(operationA, ref nameA, ref positionA, ref rotationA, ref scaleA);
        }
        if (operationB != null)
        {
            ObjAssign(operationB, ref nameB, ref positionB, ref rotationB, ref scaleB);
        }
    }

    // Update is called once per frame
    public void Update()
    {
        //判空

        //更改operationA or operationB对应的需要数据更新
        if(operationA!=null && operationB != null)
        {
            if (nameA != operationA.name || nameB != operationB.name)
            {
                StopAllCoroutines();
                living = false;//必不可少 把锁打开
                if (nameA != operationA.name)
                {
                    ObjAssign(operationA, ref nameA, ref positionA, ref rotationA, ref scaleA);
                }
                if (nameB != operationB.name)
                {
                    ObjAssign(operationB, ref nameB, ref positionB, ref rotationB, ref scaleB);
                }
            }

            ///作用不大？？？ && Application.isEditor 
            if (!living && isEditor && Application.isPlaying)
            {
                StartCoroutine(Execute());
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
        bool changeA = operationA.transform.position != positionA || operationA.transform.rotation != rotationA || operationA.transform.lossyScale != scaleA;
        bool changeB = operationB.transform.position != positionB || operationB.transform.rotation != rotationB || operationB.transform.lossyScale != scaleB;

        
        if (changeA||changeB)
        {
            //保存两个物体上一次位置、方向、范围的变化
            positionA = operationA.transform.position;
            rotationA = operationA.transform.rotation;
            scaleA = operationA.transform.lossyScale;

            positionB = operationB.transform.position;
            rotationB = operationB.transform.rotation;
            scaleB = operationB.transform.lossyScale;

            return true;
        }
        return false;
    }

    //Execute update SDF
    private IEnumerator Execute()
    {
        living = true;
        //Bounds operationBound = operationA.GetComponent<Renderer>().bounds;
        while (operationA != null && operationB != null)
        {// && SB.sceneBox.Intersects(operationA.GetComponent<Renderer>().bounds) && SB.sceneBox.Intersects(operationB.GetComponent<Renderer>().bounds)
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

        SB.UpdateSDF(operationA, operationB, operationType);

        //MC 局部更新
        //UseMC mc = new UseMC(SB);
        //mc.ComputeMC();
        //GetComponent<MeshFilter>().mesh = mc.mesh;
        //GetComponent<Renderer>().sharedMaterials = mats.ToArray();


        /////////////使用mcshader//////////
        ///

        if (McShader)
        {
            //string srcName = "Assets/source/res/resSDF.asset";
            //AssetDatabase.CreateAsset(SB.TexMatrix, srcName);

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

    }
}
