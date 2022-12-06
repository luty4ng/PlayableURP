using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[ExecuteInEditMode]
public class RadialBlurFeature : ScriptableRendererFeature
{
    [System.Serializable]
    public class MySetting
    {
        public string name = "Radial Blur";
        public Material RadialBlurMat = null;
        [Range(0, 1)] public float x = 0.5f;
        [Range(0, 1)] public float y = 0.5f;
        [Range(1, 8)] public int iteration = 5;
        [Range(1, 8)] public float blur = 3;
        [Range(1, 5)] public int downsample = 2;
        [Range(0, 1)] public float instensity = 0.5f;
        public RenderPassEvent renderPassEvent = RenderPassEvent.AfterRenderingTransparents;
    }

    public MySetting setting = new MySetting();
    class CustomRenderPass : ScriptableRenderPass
    {
        public Material material;
        public string name;
        public float x;
        public float y;
        public int iteration;
        public float instensity;
        public float blur;
        public int downsample;
        public RenderTargetIdentifier Source { get; set; }
        public RenderTargetIdentifier BlurTex;
        public RenderTargetIdentifier DSSourceTex;
        public RenderTargetIdentifier SourceTex;

        int ssW;
        int ssH;

        public void Setup(RenderTargetIdentifier source)
        {
            this.Source = source;
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            int BlurTexID = Shader.PropertyToID("_BlurTex");
            int DSSourceTexId = Shader.PropertyToID("_DSSourceTex");
            int SourceTexId = Shader.PropertyToID("_SourceTex");
            int iterID = Shader.PropertyToID("_Iteration");
            int Xid = Shader.PropertyToID("_X");
            int Yid = Shader.PropertyToID("_Y");
            int BlurID = Shader.PropertyToID("_Blur");
            int instenID = Shader.PropertyToID("_Instensity");
            
            // DownSampling
            RenderTextureDescriptor SSdesc = renderingData.cameraData.cameraTargetDescriptor;
            ssH = SSdesc.height / downsample;
            ssW = SSdesc.width / downsample;
            CommandBuffer cmd = CommandBufferPool.Get(name);
            cmd.GetTemporaryRT(DSSourceTexId, ssW, ssH, 0, FilterMode.Bilinear, RenderTextureFormat.ARGB32);//用来存降采样的
            cmd.GetTemporaryRT(BlurTexID, SSdesc);
            cmd.GetTemporaryRT(SourceTexId, SSdesc);
            BlurTex = new RenderTargetIdentifier(BlurTexID);
            DSSourceTex = new RenderTargetIdentifier(DSSourceTexId);
            SourceTex = new RenderTargetIdentifier(SourceTexId);

            cmd.SetGlobalFloat(iterID, iteration);
            cmd.SetGlobalFloat(Xid, x);
            cmd.SetGlobalFloat(Yid, y);
            cmd.SetGlobalFloat(BlurID, blur);
            cmd.SetGlobalFloat(instenID, instensity);

            cmd.Blit(Source, DSSourceTex); //down sampled source, feed to pass0 to blur
            cmd.Blit(Source, SourceTex); //source, feed to pass1 to lerp
            cmd.Blit(DSSourceTex, BlurTex, material, 0);
            cmd.Blit(BlurTex, Source, material, 1);
            context.ExecuteCommandBuffer(cmd);
            cmd.ReleaseTemporaryRT(BlurTexID);
            cmd.ReleaseTemporaryRT(DSSourceTexId);
            cmd.ReleaseTemporaryRT(SourceTexId);
            CommandBufferPool.Release(cmd);
        }
    }

    CustomRenderPass m_ScriptablePass;

    public override void Create()
    {
        m_ScriptablePass = new CustomRenderPass();
        m_ScriptablePass.renderPassEvent = setting.renderPassEvent;
        m_ScriptablePass.blur = setting.blur;
        m_ScriptablePass.x = setting.x;
        m_ScriptablePass.y = setting.y;
        m_ScriptablePass.instensity = setting.instensity;
        m_ScriptablePass.iteration = setting.iteration;
        m_ScriptablePass.material = setting.RadialBlurMat;
        m_ScriptablePass.name = setting.name;
        m_ScriptablePass.downsample = setting.downsample;
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (setting.RadialBlurMat != null)
        {
            m_ScriptablePass.Setup(renderer.cameraColorTarget);
            renderer.EnqueuePass(m_ScriptablePass);
        }
        else
        {
            Debug.LogError("Missing Material in Radial Blur render feature.");
        }
    }

}
