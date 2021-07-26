using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[AddComponentMenu("SceneOperate/Scene Operation Area")]
[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[ExecuteInEditMode]

//public class SceneSDFArea : MonoBehaviour
//{
//    public SceneBox SB;
//    public ComputeShader McShader;
//    public ComputeShader SdfShader;

//    private List<OpAndType> operations;

//    //A lock that controls the execution of a program
//    public bool living = false;
//    public bool isEditor;

//    private List<Material> mats = new List<Material>();

//    // Start is called before the first frame update
//    void Start()
//    {
//        living = false;
//        Init();
//    }

//    public void Init()
//    {
//        SB = new SceneBox(SdfShader);

//    }

//    public bool changed()
//    {
//        bool flag = false;
//        int size = operations.Count;
//        for (int i = 0; i < size; i++)
//        {
//            Transform tran = operations[i].mesh.transform;
//            OpState state = operations[i].state;
//            if (state.changed(tran))
//            {
//                flag = true;

//                OpAndType tmp = operations[i];
//                tmp.state = new OpState(tran);
//                operations[i] = tmp;
//            }
//        }
//        return flag;
//    }

//    // Update is called once per frame
//    void Update()
//    {

//        if (!living && isEditor && Application.isPlaying)
//        {
//            StartCoroutine(Execute());
//        }
//    }

//    public void ExecuteOnClick()
//    {
//        UpdateSDF();
//        UpdateMesh(McShader);
//    }

//    private IEnumerator Execute()
//    {
//        living = true;
//        while (operations.Count > 0)
//        {
//            if (changed())
//            {
//                UpdateSDF();
//                UpdateMesh(McShader);
//            }
//            else
//            {
//                yield return null;
//            }
//        }
//        yield return null;
//        living = false;
//        yield break;
//    }

//    public void UpdateSDF()
//    {
//        int size = operations.Count;
//        if (size == 2)
//        {
//            SB.UpdateSDF(operations[0].mesh, operations[1].mesh, operations[1].type);
//        }
//        if (size > 2)
//        {
//            for (int i = 2; i < size; i++)
//            {
//                SB.UpdateSDFLater(operations[i].mesh, operations[i].type);
//            }
//        }


//    }

//    public void UpdateMesh(ComputeShader mcShader)
//    {
//        if (SB.ncells != Vector3Int.zero)
//        {
//            UseMcShader mc = new UseMcShader(SB, mcShader);
//            mc.ComputeMC();
//            GetComponent<MeshFilter>().mesh = mc.mesh;
//            GetComponent<Renderer>().sharedMaterials = mats.ToArray();
//        }
//        else
//        {
//            print("need compute shader");
//        }
//    }

//    public struct OpAndType
//    {
//        public MeshFilter mesh;
//        public BooleanType type;
//        public OpState state;
//        //int orderId;
//        public OpAndType(MeshFilter mesh1, BooleanType type1, OpState state1)
//        {
//            mesh = mesh1;
//            type = type1;
//            state = state1;
//        }

//        public OpAndType(MeshFilter mesh1, BooleanType type1)
//        {
//            mesh = mesh1;
//            type = type1;
//            state = new OpState(mesh.transform);
//        }
//    }

//    public struct OpState
//    {
//        public Vector3 postion;
//        public Quaternion rotation;
//        public Vector3 scale;

//        public OpState(Vector3 pos, Quaternion rot, Vector3 sc)
//        {
//            postion = pos;
//            rotation = rot;
//            scale = sc;
//        }

//        public OpState(in Transform tran)
//        {
//            postion = tran.position;
//            rotation = tran.rotation;
//            scale = tran.localScale;
//        }

//        public bool changed(in Transform tran)
//        {
//            if (tran.position != postion || tran.rotation != rotation || tran.lossyScale != scale)
//            {
//                return true;
//            }
//            return false;
//        }
//    }


//}

public class SceneSDFArea : MonoBehaviour
{
    public SceneBox SB;
    public ComputeShader McShader;
    public ComputeShader SdfShader;

    private List<OpAndType> Info = new List<OpAndType>();
    public List<MeshFilter> Operations = new List<MeshFilter>();

    //A lock that controls the execution of a program
    public bool living = false;
    public bool isEditor;

    private List<Material> mats = new List<Material>();

    // Start is called before the first frame update
    void Start()
    {
        living = false;
        Init();
    }

    public void Init()
    {
        SB = new SceneBox(SdfShader);
        //Info = new List<OpAndType>();
        //Operations = new List<MeshFilter>();

        int size = Operations.Count;
        for(int i = 0; i < size; i++)
        {
            Info.Add(new OpAndType(Operations[i], BooleanType.Union));
        }
    }

    public bool changed()
    {
        bool flag = false;
        int size = Operations.Count;

        if (isNull(Operations))
        {
            for (int i = 0; i < size; i++)
            {
                Transform tran = Operations[i].transform;
                OpState state = Info[i].state;
                if (state.changed(tran))
                {
                    flag = true;

                    OpAndType tmp = Info[i];
                    tmp.state = new OpState(tran);
                    Info[i] = tmp;
                }
            }
        }
        
        return flag;
    }

    // Update is called once per frame
    void Update()
    {
        int size = Operations.Count;
        if (size > Info.Count && isNull(Operations))
        {
            for (int i = 0; i < size; i++)
            {
                Info.Add(new OpAndType(Operations[i], BooleanType.Union));
            }
        }

        if (!living && isEditor && Application.isPlaying)
        {
            StartCoroutine(Execute());
        }
    }

    public void ExecuteOnClick()
    {
        if (UpdateSDF())
        {
            UpdateMesh();
        }
    }

    private IEnumerator Execute()
    {
        living = true;
        while (Operations.Count > 0)
        {
            if (changed()&&UpdateSDF())
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

    public bool UpdateSDF()
    {
        int size = Operations.Count;
        
        if (size >= 2 && isNull(Operations))
        {
            SB.UpdateSDF(Operations[0], Operations[1], Info[1].type);

            //string srcName = "Assets/source/res/resBegin.asset";
            //AssetDatabase.CreateAsset(SB.TexMatrix, srcName);

            for (int i = 2; i < size; i++)
            {
                SB.UpdateSDFLater(Operations[i], Info[i].type);
            }
            return true;
        }
        return false;
    }

    public void UpdateMesh()
    {
        if (McShader)
        {
            mats.Clear();
            mats.AddRange(Operations[0].GetComponent<Renderer>().sharedMaterials);

            UseMcShader mc = new UseMcShader(SB, McShader);
            mc.ComputeMC();
            GetComponent<MeshFilter>().mesh = mc.mesh;
            GetComponent<Renderer>().sharedMaterials = mats.ToArray();
        }
        else
        {
            print("need compute shader");
        }
    }

    public bool isNull(List<MeshFilter> op)
    {
        int size = op.Count;
        for(int i = 0; i < size; i++)
        {
            if (op[i] == null)
            {
                return false;
            }
        }
        return true;
    }

    public struct OpAndType
    {
        //public MeshFilter mesh;
        public BooleanType type;
        public OpState state;
        //int orderId;
        //public OpAndType(MeshFilter mesh1, BooleanType type1, OpState state1)
        //{
        //    //mesh = mesh1;
        //    type = type1;
        //    state = state1;
        //}

        public OpAndType(MeshFilter mesh1, BooleanType type1)
        {
            //mesh = mesh1;
            type = type1;
            state = new OpState(mesh1.transform);
        }
    }

    public struct OpState
    {
        public Vector3 postion;
        public Quaternion rotation;
        public Vector3 scale;

        public OpState(Vector3 pos, Quaternion rot, Vector3 sc)
        {
            postion = pos;
            rotation = rot;
            scale = sc;
        }

        public OpState(in Transform tran)
        {
            postion = tran.position;
            rotation = tran.rotation;
            scale = tran.localScale;
        }

        public bool changed(in Transform tran)
        {
            if (tran.position != postion || tran.rotation != rotation || tran.lossyScale != scale)
            {
                return true;
            }
            return false;
        }
    }


}
