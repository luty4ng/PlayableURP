using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[ExecuteInEditMode]
public class BokehBlurFeature : ScriptableRendererFeature
{

    [System.Serializable]
    public class MySetting
    {
        public string name = "Bokeh Blur";
        public RenderPassEvent renderPassEvent = RenderPassEvent.AfterRenderingOpaques;
        public Material material;
        [Tooltip("降采样，越大性能越好但是质量越低"), Range(1, 7)] public int downSamping = 2;
        [Tooltip("迭代次数，越小性能越好但是质量越低"), Range(3, 500)] public int iteration = 50;
        [Tooltip("采样半径，越大圆斑越大但是采样点越分散"), Range(0.1f, 10)] public float BlurRadius = 1;
        [Tooltip("模糊过渡的平滑度"), Range(0, 0.5f)] public float BlurSmoothness = 0.1f;
        [Tooltip("近处模糊结束距离")] public float NearDis = 5;
        [Tooltip("远处模糊开始距离")] public float FarDis = 9;
    }

    public MySetting mysetting = new MySetting();
    class CustomRenderPass : ScriptableRenderPass
    {
        public string name;
        public Material material;
        public int iteration;
        public float BlurSmoothness;
        public int downSamping;
        public float BlurRadius;
        public float NearDis;
        public float FarDis;
        RenderTargetIdentifier sour;
        int width;
        int height;
        readonly static int BlurID = Shader.PropertyToID("blur");//申请之后就不在变化
        readonly static int SourBakedID = Shader.PropertyToID("_SourTex");
        public void setup(RenderTargetIdentifier Sour)
        {
            this.sour = Sour;
            material.SetFloat("_Iteration", iteration);
            material.SetFloat("_BlurRadius", BlurRadius);
            material.SetFloat("_NearDis", NearDis);
            material.SetFloat("_FarDis", FarDis);
            material.SetFloat("_BlurSmoothness", BlurSmoothness);
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {

            CommandBuffer cmd = CommandBufferPool.Get(name);
            RenderTextureDescriptor desc = renderingData.cameraData.cameraTargetDescriptor;
            width = desc.width / downSamping;
            height = desc.height / downSamping;
            cmd.GetTemporaryRT(BlurID, width, height, 0, FilterMode.Bilinear, RenderTextureFormat.ARGB32);
            cmd.GetTemporaryRT(SourBakedID, desc);
            cmd.CopyTexture(sour, SourBakedID); //会自动送到shader里
            cmd.Blit(sour, BlurID, material, 0); //降采样模糊的pass
            cmd.Blit(BlurID, sour, material, 1); //降采样pass和源图混合输出
            context.ExecuteCommandBuffer(cmd);
            cmd.ReleaseTemporaryRT(BlurID);
            cmd.ReleaseTemporaryRT(SourBakedID);
            CommandBufferPool.Release(cmd);
        }
    }

    CustomRenderPass m_ScriptablePass = new CustomRenderPass();

    public override void Create()
    {
        m_ScriptablePass.material = mysetting.material;
        m_ScriptablePass.iteration = mysetting.iteration;
        m_ScriptablePass.BlurSmoothness = mysetting.BlurSmoothness;
        m_ScriptablePass.BlurRadius = mysetting.BlurRadius;
        m_ScriptablePass.renderPassEvent = mysetting.renderPassEvent;
        m_ScriptablePass.name = mysetting.name;
        m_ScriptablePass.downSamping = mysetting.downSamping;
        m_ScriptablePass.NearDis = Mathf.Max(mysetting.NearDis, 0);
        m_ScriptablePass.FarDis = Mathf.Max(mysetting.NearDis, mysetting.FarDis);
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        m_ScriptablePass.setup(renderer.cameraColorTarget);
        renderer.EnqueuePass(m_ScriptablePass);
    }

}