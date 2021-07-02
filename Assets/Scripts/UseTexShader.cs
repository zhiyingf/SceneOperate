using UnityEngine;

//public class UseTexShader
//{
//    const int threadGroupSize = 8;

//    RenderTexture sdfA;
//    RenderTexture sdfB;

//    //RenderTexture sdfRes;
//    ComputeBuffer sdfRes;

//    Matrix4x4 transMatrixA;
//    Matrix4x4 transMatrixB;

//    Vector3 boundsMinA;
//    Vector3 boundsMaxA;
//    Vector3 boundsMinB;
//    Vector3 boundsMaxB;

//    Vector3Int sizeA;
//    Vector3Int sizeB;

//    Vector3 localBoxMin;

//    Vector3Int Npoint;

//    public UseTexShader(in Transform transformA, in Transform transformB, in Vector3Int ncell, in Vector3 localBoxMins)
//    {
//        ManagerScriptableObject attachScr = transformA.GetComponent<AttachScriptable>().Scriptable;
//        sizeA = attachScr.Size;
//        setRenderTexture(ref sdfA, sizeA, attachScr.SDFTexture);
//        Bounds boundsA = attachScr.Bounds;
//        boundsMinA = boundsA.min;
//        boundsMaxA = boundsA.max;
//        transMatrixA = transformA.worldToLocalMatrix;


//        attachScr = transformB.GetComponent<AttachScriptable>().Scriptable;
//        sizeB = attachScr.Size;
//        setRenderTexture(ref sdfB, sizeB, attachScr.SDFTexture);
//        Bounds boundsB = attachScr.Bounds;
//        boundsMinB = boundsB.min;
//        boundsMaxB = boundsB.max;
//        transMatrixB = transformB.worldToLocalMatrix;

//        localBoxMin = localBoxMins;

//        Npoint = ncell + Vector3Int.one;
//    }

//    public void ComputeSDF(in ComputeShader SdfShader, in BooleanType type, ref float[,,] boxMatrix)
//    {
//        Vector3Int numThreadsPerAxis = new Vector3Int(Mathf.CeilToInt(Npoint.x / (float)threadGroupSize), Mathf.CeilToInt(Npoint.y / (float)threadGroupSize), Mathf.CeilToInt(Npoint.z / (float)threadGroupSize));
//        int[] xyzAxis = { Npoint.x, Npoint.y, Npoint.z };
//        SdfShader.SetInts("numPointsXyzAxis", xyzAxis);


//        SdfShader.SetTexture(0, "sdfA", sdfA);
//        SdfShader.SetTexture(0, "sdfB", sdfB);

//        //Texture3D TexRes = new Texture3D(Npoint.x, Npoint.y, Npoint.z, TextureFormat.RFloat, false);
//        //setRenderTexture(ref sdfRes, Npoint);
//        //SdfShader.SetTexture(0, "sdfRes", sdfRes);

//        int numPoints = Npoint.x * Npoint.y * Npoint.z;
//        sdfRes = new ComputeBuffer(numPoints, sizeof(float));
//        SdfShader.SetBuffer(0, "sdfRes", sdfRes);

//        SdfShader.SetMatrix("transMatrixA", transMatrixA);
//        SdfShader.SetMatrix("transMatrixB", transMatrixB);

//        float[] BoundsMinA = { boundsMinA.x, boundsMinA.y, boundsMinA.z };
//        SdfShader.SetFloats("boundsMinA", BoundsMinA);
//        float[] BoundsMaxA = { boundsMaxA.x, boundsMaxA.y, boundsMaxA.z };
//        SdfShader.SetFloats("boundsMaxA", BoundsMaxA);
//        float[] BoundsMinB = { boundsMinB.x, boundsMinB.y, boundsMinB.z };
//        SdfShader.SetFloats("boundsMinB", BoundsMinB);
//        float[] BoundsMaxB = { boundsMaxB.x, boundsMaxB.y, boundsMaxB.z };
//        SdfShader.SetFloats("boundsMaxB", BoundsMaxB);

//        int[] SizeA = { sizeA.x, sizeA.y, sizeA.z, sizeA.x * sizeA.y * sizeA.z };
//        SdfShader.SetInts("SizeA", SizeA);
//        int[] SizeB = { sizeB.x, sizeB.y, sizeB.z, sizeB.x * sizeB.y * sizeB.z };
//        SdfShader.SetInts("SizeB", SizeB);

//        float[] LocalBoxMin = { localBoxMin.x, localBoxMin.y, localBoxMin.z };
//        SdfShader.SetFloats("localBoxMin", LocalBoxMin);

//        SdfShader.SetFloat("step", Constants.Step);
//        SdfShader.SetInt("type", (int)type);
//        SdfShader.SetFloat("MaxValue", float.MaxValue);

//        SdfShader.Dispatch(0, numThreadsPerAxis.x, numThreadsPerAxis.y, numThreadsPerAxis.z);

//        float[] tmp = new float[numPoints];
//        sdfRes.GetData(tmp, 0, 0, numPoints);
//        ReleaseBuffers(ref sdfRes);

//        for (int i = 0; i < Npoint.x; i++)
//        {
//            for (int j = 0; j < Npoint.y; j++)
//            {
//                for (int k = 0; k < Npoint.z; k++)
//                {
//                    int idx = i + j * Npoint.x + k * Npoint.y * Npoint.x;
//                    boxMatrix[i, j, k] = tmp[idx];
//                }
//            }
//        }
//    }

//    private void setRenderTexture(ref RenderTexture renderTex, in Vector3Int size, in Texture3D tex3D)
//    {
//        renderTex = new RenderTexture(size.x, size.y, 0, RenderTextureFormat.RFloat);//nt width, int height, int depth(深度缓冲区bit数，不赋值给相机。不需要控制深度，那么深度缓冲区设置为0),
//        renderTex.volumeDepth = size.z; //3D的体积范围渲染纹理或数组纹理的切片数 Use volumeDepth to set 3D depth
//        renderTex.enableRandomWrite = true; //设置了enableRandomWrite标记,这使你的compute shader 有权写入贴图
//        renderTex.dimension = UnityEngine.Rendering.TextureDimension.Tex3D; //3d类型的renderTexture只用于compute shader capable platforms
//        renderTex.filterMode = FilterMode.Bilinear; //单像素采样
//        renderTex.wrapMode = TextureWrapMode.Repeat;
//        renderTex.useMipMap = false;
//        renderTex.Create(); //如果不执行create(),Shader执行结束像素也不会被修改

//        for (int i = 0; i < renderTex.volumeDepth; i++)
//        {
//            Graphics.Blit(tex3D, renderTex, i, i); //rendertexture 赋值的方法是bilt 但是3d render texture似乎只在第一层作用？其实是只bilt 1 层
//        }
//    }

//    private void setRenderTexture(ref RenderTexture renderTex, in Vector3Int size)
//    {
//        renderTex = new RenderTexture(size.x, size.y, 0, RenderTextureFormat.RFloat);//nt width, int height, int depth(深度缓冲区bit数，不赋值给相机。不需要控制深度，那么深度缓冲区设置为0),
//        renderTex.volumeDepth = size.z; //3D的体积范围渲染纹理或数组纹理的切片数 Use volumeDepth to set 3D depth
//        renderTex.enableRandomWrite = true; //设置了enableRandomWrite标记,这使你的compute shader 有权写入贴图
//        renderTex.dimension = UnityEngine.Rendering.TextureDimension.Tex3D; //3d类型的renderTexture只用于compute shader capable platforms
//        renderTex.filterMode = FilterMode.Bilinear; //单像素采样
//        renderTex.wrapMode = TextureWrapMode.Repeat;
//        renderTex.useMipMap = false;
//        renderTex.Create(); //如果不执行create(),Shader执行结束像素也不会被修改
//    }

//    private void InitBuffers(in float[,,] temp, ref ComputeBuffer buf, ref Vector3Int size)
//    {
//        int numPoints = size.x * size.y * size.z;
//        float[] tmp1 = new float[numPoints];
//        for (int i = 0; i < size.x; i++)
//        {
//            for (int j = 0; j < size.y; j++)
//            {
//                for (int k = 0; k < size.z; k++)
//                {
//                    int idx = i + j * size.x + k * size.y * size.x;
//                    tmp1[idx] = temp[i, j, k];
//                }
//            }
//        }

//        buf = new ComputeBuffer(numPoints, sizeof(float));
//        buf.SetData(tmp1);
//    }

//    private void ReleaseBuffers(ref ComputeBuffer buf)
//    {
//        if (buf != null)
//        {
//            buf.Release();
//        }
//    }

//}

public class UseTexShader
{
    const int threadGroupSize = 8;

    Texture3D texA;
    Texture3D texB;

    ComputeBuffer sdfA;
    ComputeBuffer sdfB;

    //RenderTexture sdfA;
    //RenderTexture sdfB;

    //RenderTexture sdfRes;
    ComputeBuffer sdfRes;

    Matrix4x4 transMatrixA;
    Matrix4x4 transMatrixB;

    Vector3 boundsMinA;
    Vector3 boundsMaxA;
    Vector3 boundsMinB;
    Vector3 boundsMaxB;

    Vector3Int sizeA;
    Vector3Int sizeB;

    Vector3 localBoxMin;

    Vector3Int Npoint;

    public UseTexShader(in Transform transformA, in Transform transformB, in Vector3Int ncell, in Vector3 LocalBoxMins)
    {
        ManagerScriptableObject attachScr = transformA.GetComponent<AttachScriptable>().Scriptable;
        sizeA = attachScr.Size;
        texA = attachScr.SDFTexture;
        //setRenderTexture(ref sdfA, sizeA, attachScr.SDFTexture);
        Bounds boundsA = attachScr.Bounds;
        boundsMinA = boundsA.min;
        boundsMaxA = boundsA.max;
        transMatrixA = transformA.worldToLocalMatrix;


        attachScr = transformB.GetComponent<AttachScriptable>().Scriptable;
        sizeB = attachScr.Size;
        texB = attachScr.SDFTexture;
        //setRenderTexture(ref sdfB, sizeB, attachScr.SDFTexture);
        Bounds boundsB = attachScr.Bounds;
        boundsMinB = boundsB.min;
        boundsMaxB = boundsB.max;
        transMatrixB = transformB.worldToLocalMatrix;

        localBoxMin = LocalBoxMins;

        Npoint = ncell + Vector3Int.one;
    }

    public void ComputeSDF(in ComputeShader SdfShader, in BooleanType type, ref float[,,] boxMatrix)
    {
        Vector3Int numThreadsPerAxis = new Vector3Int(Mathf.CeilToInt(Npoint.x / (float)threadGroupSize), Mathf.CeilToInt(Npoint.y / (float)threadGroupSize), Mathf.CeilToInt(Npoint.z / (float)threadGroupSize));
        int[] xyzAxis = { Npoint.x, Npoint.y, Npoint.z };
        SdfShader.SetInts("numPointsXyzAxis", xyzAxis);


        int n1 = sizeA.x * sizeA.y * sizeA.z;
        sdfA = new ComputeBuffer(n1, sizeof(float));
        var tmp1 = texA.GetPixelData<float>(0);
        sdfA.SetData(tmp1);
        SdfShader.SetBuffer(0, "sdfA", sdfA);

        int n2 = sizeB.x * sizeB.y * sizeB.z;
        sdfB = new ComputeBuffer(n2, sizeof(float));
        var tmp2 = texB.GetPixelData<float>(0);
        sdfB.SetData(tmp2);
        SdfShader.SetBuffer(0, "sdfB", sdfB);

        int numPoints = Npoint.x * Npoint.y * Npoint.z;
        sdfRes = new ComputeBuffer(numPoints, sizeof(float));
        SdfShader.SetBuffer(0, "sdfRes", sdfRes);

        SdfShader.SetMatrix("transMatrixA", transMatrixA);
        SdfShader.SetMatrix("transMatrixB", transMatrixB);

        float[] BoundsMinA = { boundsMinA.x, boundsMinA.y, boundsMinA.z };
        SdfShader.SetFloats("boundsMinA", BoundsMinA);
        float[] BoundsMaxA = { boundsMaxA.x, boundsMaxA.y, boundsMaxA.z };
        SdfShader.SetFloats("boundsMaxA", BoundsMaxA);
        float[] BoundsMinB = { boundsMinB.x, boundsMinB.y, boundsMinB.z };
        SdfShader.SetFloats("boundsMinB", BoundsMinB);
        float[] BoundsMaxB = { boundsMaxB.x, boundsMaxB.y, boundsMaxB.z };
        SdfShader.SetFloats("boundsMaxB", BoundsMaxB);

        int[] SizeA = { sizeA.x, sizeA.y, sizeA.z, sizeA.x * sizeA.y * sizeA.z };
        SdfShader.SetInts("SizeA", SizeA);
        int[] SizeB = { sizeB.x, sizeB.y, sizeB.z, sizeB.x * sizeB.y * sizeB.z };
        SdfShader.SetInts("SizeB", SizeB);

        float[] LocalBoxMin = { localBoxMin.x, localBoxMin.y, localBoxMin.z };
        SdfShader.SetFloats("localBoxMin", LocalBoxMin);

        SdfShader.SetFloat("step", Constants.Step);
        SdfShader.SetInt("type", (int)type);
        SdfShader.SetFloat("MaxValue", float.MaxValue);

        SdfShader.Dispatch(0, numThreadsPerAxis.x, numThreadsPerAxis.y, numThreadsPerAxis.z);

        float[] tmp = new float[numPoints];
        sdfRes.GetData(tmp, 0, 0, numPoints);
        ReleaseBuffers(ref sdfRes);
        ReleaseBuffers(ref sdfA);
        ReleaseBuffers(ref sdfB);

        for (int i = 0; i < Npoint.x; i++)
        {
            for (int j = 0; j < Npoint.y; j++)
            {
                for (int k = 0; k < Npoint.z; k++)
                {
                    int idx = i + j * Npoint.x + k * Npoint.y * Npoint.x;
                    boxMatrix[i, j, k] = tmp[idx];
                }
            }
        }
    }

    private void ReleaseBuffers(ref ComputeBuffer buf)
    {
        if (buf != null)
        {
            buf.Release();
        }
    }

}