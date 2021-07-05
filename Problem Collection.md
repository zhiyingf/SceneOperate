# Problem Collection

+ Unity3D & ComputeShader & Texture3D & RenderTexture & ScriptableObject
  + 由于每个model都对应一个SDF，如何存储SDF以及SDF相关数据（比如SDF size，Bounds），然后与模型关联
    + 使用一个ScriptableObject存储一个model所有所需数据，其中将SDF存储到Texture3D
  + 将SDF存储到Texture3D，将Texture3D传入ComputeShader进行SDF交并差运算
    + Texture3D传入ComputeShader不可随机读取，属于**SRV（Shader resource view）**，而RenderTexture设置enableRandomWrite为True可以实现随机读取，属于UAV（Unordered Access view），由此只能先将Texture3D转化成RenderTexture，然后传入ComputeShader。
      + 如何将Texture3D转化成RenderTexture？
        + [How do I get the raw pixels of a RFloat RenderTexture on the CPU?](https://answers.unity.com/questions/1398173/how-do-i-get-the-raw-pixels-of-a-rfloat-rendertext.html?childToView=1846405#answer-1846405)
        + [how to blit texture3D to Volume RenderTexture](https://answers.unity.com/questions/1686993/how-to-blit-texture3d-to-volume-rendertexture.html)
    + 在ComputeShader中如何使用Texture3D进行采样？
      + SamplerState名字设置问题？[Compute Shader SamplerState confusion](https://forum.unity.com/threads/compute-shader-samplerstate-confusion.163591/)
      + Texture3D采样函数选择问题？以及使用问题？[Compute Shader compile error from Texture3D.Sample()](https://answers.unity.com/questions/1311444/compute-shader-compile-error-from-texture3dsample.html)
  + 
+ 

