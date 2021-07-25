using UnityEditor;
using UnityEngine;

public class UseSdfTexShader
{
    const int threadGroupSize = 8;

    ComputeShader SdfShader;

    Texture3D texA;
    Texture3D texB;

    RenderTexture sdfA;
    //RenderTexture sdfB;

    //RenderTexture sdfRes;
    ComputeBuffer sdfRes;

    Matrix4x4 transMatrixA;
    Matrix4x4 transMatrixB;

    Vector3 boundsMinA;
    Vector3 boundsMaxA;
    Vector3 boundsSizeA;

    Vector3 boundsMinB;
    Vector3 boundsMaxB;
    Vector3 boundsSizeB;

    Vector3Int sizeA;
    Vector3Int sizeB;

    Vector3 localBoxMin;

    Vector3Int Npoint;

    public UseSdfTexShader(in Transform transformA, in Transform transformB, in Vector3Int ncell, in Vector3 localBoxMins, in ComputeShader sdfShader)
    {
        SdfShader = sdfShader;

        ManagerScriptableObject attachScr = transformA.GetComponent<AttachScriptable>().Scriptable;
        sizeA = attachScr.Size;
        texA = attachScr.SDFTexture;
        //setRenderTexture(ref sdfA, sizeA, texA);
        //AssetDatabase.CreateAsset(sdfA, "Assets/sdfA.asset");
        Bounds boundsA = attachScr.Bounds;
        boundsMinA = boundsA.min;
        boundsMaxA = boundsA.max;
        boundsSizeA = boundsA.size;
        transMatrixA = transformA.worldToLocalMatrix;


        attachScr = transformB.GetComponent<AttachScriptable>().Scriptable;
        sizeB = attachScr.Size;
        texB = attachScr.SDFTexture;
        //setRenderTexture(ref sdfB, sizeB, texB);
        //AssetDatabase.CreateAsset(sdfB, "Assets/sdfB.asset");
        Bounds boundsB = attachScr.Bounds;
        boundsMinB = boundsB.min;
        boundsMaxB = boundsB.max;
        boundsSizeB = boundsB.size;
        transMatrixB = transformB.worldToLocalMatrix;

        localBoxMin = localBoxMins;

        Npoint = ncell + Vector3Int.one;
    }

    public void SetSDFLater(in Texture3D TexMatrix, in Bounds boundsA, in Transform transformB, in Vector3Int ncell, in Vector3 localBoxMins)
    {
        sizeA = Npoint;
        texA = TexMatrix;
        boundsMinA = boundsA.min;
        boundsMaxA = boundsA.max;
        boundsSizeA = boundsA.size;
        transMatrixA = Matrix4x4.identity;

        ManagerScriptableObject attachScr = transformB.GetComponent<AttachScriptable>().Scriptable;
        sizeB = attachScr.Size;
        texB = attachScr.SDFTexture;
        Bounds boundsB = attachScr.Bounds;
        boundsMinB = boundsB.min;
        boundsMaxB = boundsB.max;
        boundsSizeB = boundsB.size;
        transMatrixB = transformB.worldToLocalMatrix;

        localBoxMin = localBoxMins;

        Npoint = ncell + Vector3Int.one;
    }

    public void ComputeSDF(in BooleanType type, ref Texture3D TexMatrix)//ref float[,,] boxMatrix
    {
        Vector3Int numThreadsPerAxis = new Vector3Int(Mathf.CeilToInt(Npoint.x / (float)threadGroupSize), Mathf.CeilToInt(Npoint.y / (float)threadGroupSize), Mathf.CeilToInt(Npoint.z / (float)threadGroupSize));
        int[] xyzAxis = { Npoint.x, Npoint.y, Npoint.z };
        SdfShader.SetInts("numPointsXyzAxis", xyzAxis);


        //SdfShader.SetTexture(0, "sdfA", sdfA, 0, UnityEngine.Rendering.RenderTextureSubElement.Default);
        //SdfShader.SetTexture(0, "sdfB", sdfB, 0, UnityEngine.Rendering.RenderTextureSubElement.Default);
        initRenderTexture(ref sdfA, sizeA);
        Graphics.CopyTexture(texA, sdfA);
        SdfShader.SetTexture(0, "sdfA", sdfA, 0);

        initRenderTexture(ref sdfA, sizeB);
        Graphics.CopyTexture(texB, sdfA);
        SdfShader.SetTexture(0, "sdfB", sdfA, 0);

        int numPoints = Npoint.x * Npoint.y * Npoint.z;
        sdfRes = new ComputeBuffer(numPoints, sizeof(float));
        SdfShader.SetBuffer(0, "sdfRes", sdfRes);
        //initRenderTexture(ref sdfRes, Npoint);
        //SdfShader.SetTexture(0, "sdfRes", sdfRes, 0);

        SdfShader.SetMatrix("transMatrixA", transMatrixA);
        SdfShader.SetMatrix("transMatrixB", transMatrixB);

        float[] BoundsMinA = { boundsMinA.x, boundsMinA.y, boundsMinA.z };
        SdfShader.SetFloats("boundsMinA", BoundsMinA);
        float[] BoundsMaxA = { boundsMaxA.x, boundsMaxA.y, boundsMaxA.z };
        SdfShader.SetFloats("boundsMaxA", BoundsMaxA);
        float[] BoundsSizeA = { boundsSizeA.x, boundsSizeA.y, boundsSizeA.z };
        SdfShader.SetFloats("boundsSizeA", BoundsSizeA);

        float[] BoundsMinB = { boundsMinB.x, boundsMinB.y, boundsMinB.z };
        SdfShader.SetFloats("boundsMinB", BoundsMinB);
        float[] BoundsMaxB = { boundsMaxB.x, boundsMaxB.y, boundsMaxB.z };
        SdfShader.SetFloats("boundsMaxB", BoundsMaxB);
        float[] BoundsSizeB = { boundsSizeB.x, boundsSizeB.y, boundsSizeB.z };
        SdfShader.SetFloats("boundsSizeB", BoundsSizeB);

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

        ///////
        //TexMatrix = new Texture3D(Npoint.x, Npoint.y, Npoint.z, TextureFormat.RFloat, false);
        //TexMatrix.filterMode = FilterMode.Bilinear;
        //TexMatrix.wrapMode = TextureWrapMode.Clamp;
        //Graphics.CopyTexture(sdfRes, TexMatrix);
        ///////

        /////
        float[] tmp = new float[numPoints];
        sdfRes.GetData(tmp, 0, 0, numPoints);
        ReleaseBuffers(ref sdfRes);


        TexMatrix = new Texture3D(Npoint.x, Npoint.y, Npoint.z, TextureFormat.RFloat, false);
        TexMatrix.filterMode = FilterMode.Bilinear;
        TexMatrix.wrapMode = TextureWrapMode.Clamp;
        TexMatrix.SetPixelData(tmp, 0);
        TexMatrix.Apply();
        /////

        ///////
        //for (int i = 0; i < Npoint.x; i++)
        //{
        //    for (int j = 0; j < Npoint.y; j++)
        //    {
        //        for (int k = 0; k < Npoint.z; k++)
        //        {
        //            int idx = i + j * Npoint.x + k * Npoint.y * Npoint.x;
        //            boxMatrix[i, j, k] = tmp[idx];
        //        }
        //    }
        //}
    }

    private void initRenderTexture(ref RenderTexture renderTex, in Vector3Int size)
    {
        renderTex = new RenderTexture(size.x, size.y, 0, RenderTextureFormat.RFloat);//nt width, int height, int depth(深度缓冲区bit数，不赋值给相机。不需要控制深度，那么深度缓冲区设置为0),
        renderTex.enableRandomWrite = true; //设置了enableRandomWrite标记,这使你的compute shader 有权写入贴图
        renderTex.dimension = UnityEngine.Rendering.TextureDimension.Tex3D; //3d类型的renderTexture只用于compute shader capable platforms
        renderTex.volumeDepth = size.z; //3D的体积范围渲染纹理或数组纹理的切片数 Use volumeDepth to set 3D depth
        renderTex.filterMode = FilterMode.Bilinear;
        renderTex.wrapMode = TextureWrapMode.Clamp;
        renderTex.useMipMap = false;
        renderTex.Create(); //如果不执行create(),Shader执行结束像素也不会被修改

        //for (int i = 0; i < renderTex.volumeDepth; i++)
        //{
        //    Graphics.Blit(tex3D, renderTex, i, i); //rendertexture 赋值的方法是blit 但是3d render texture似乎只在第一层作用？其实是只bilt 1 层
        //}
        //Graphics.CopyTexture(tex3D, renderTex);
        //Graphics.ConvertTexture(tex3D, renderTex);
    }

    private void ReleaseBuffers(ref ComputeBuffer buf)
    {
        if (buf != null)
        {
            buf.Release();
        }
    }

}
