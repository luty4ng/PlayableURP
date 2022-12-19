using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class GlitchSplitFeature : ScriptableRendererFeature
{
    [System.Serializable]
    public class MySetting
    {
        public string profilerTag = "Minimal Feature";
        public RenderPassEvent renderPassEvent = RenderPassEvent.AfterRenderingTransparents;
        public Material material;
        [Range(0, 1)] public float Intensity = 0.5f;
    }

    public MySetting setting = new MySetting();
    class CustomRenderPass : ScriptableRenderPass
    {
        public Material passMat = null;
        public FilterMode passFilterMode { get; set; }
        private RenderTargetIdentifier passSource { get; set; }
        private RenderTargetHandle m_PassDefaultTempTex;
        private string passProfilerTag;
        public float passIntensity = 0.5f;

        public CustomRenderPass(string tag)
        {
            this.passProfilerTag = tag;
        }

        public void Setup(RenderTargetIdentifier sour)
        {
            this.passSource = sour;
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            passMat.SetFloat("_Intensity", passIntensity);
            CommandBuffer cmd = CommandBufferPool.Get(passProfilerTag);
            RenderTextureDescriptor opaquedesc = renderingData.cameraData.cameraTargetDescriptor;
            opaquedesc.depthBufferBits = 0;
            cmd.GetTemporaryRT(m_PassDefaultTempTex.id, opaquedesc, passFilterMode);
            cmd.CopyTexture(passSource, m_PassDefaultTempTex.id);
            // Blit(cmd, passSource, m_PassDefaultTempTex.Identifier(), passMat);
            Blit(cmd, m_PassDefaultTempTex.Identifier(), passSource, passMat);
            cmd.ReleaseTemporaryRT(m_PassDefaultTempTex.id);
            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }
    }

    CustomRenderPass myPass;
    public override void Create()
    {
        myPass = new CustomRenderPass(setting.profilerTag);
        myPass.renderPassEvent = setting.renderPassEvent;
        myPass.passMat = setting.material;
        myPass.passIntensity = setting.Intensity;
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        myPass.Setup(renderer.cameraColorTarget);
        renderer.EnqueuePass(myPass);
    }
}