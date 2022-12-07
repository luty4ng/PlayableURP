using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class MinimalFeature : ScriptableRendererFeature
{
    [System.Serializable]
    public class MySetting
    {
        public string profilerTag = "Minimal Feature";
        public RenderPassEvent renderPassEvent = RenderPassEvent.AfterRenderingTransparents;
        public Material material;
        public int passMax = -1;
    }

    public MySetting setting = new MySetting();
    class CustomRenderPass : ScriptableRenderPass
    {
        public Material passMat = null;
        public int passMatCount = 0;
        public FilterMode passFilterMode { get; set; }
        private RenderTargetIdentifier passSource { get; set; }
        private RenderTargetHandle m_PassDefaultTempTex;
        private string passProfilerTag;

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
            CommandBuffer cmd = CommandBufferPool.Get(passProfilerTag);
            RenderTextureDescriptor opaquedesc = renderingData.cameraData.cameraTargetDescriptor;
            opaquedesc.depthBufferBits = 0;
            cmd.GetTemporaryRT(m_PassDefaultTempTex.id, opaquedesc, passFilterMode);
            Blit(cmd, passSource, m_PassDefaultTempTex.Identifier(), passMat, passMatCount);
            Blit(cmd, m_PassDefaultTempTex.Identifier(), passSource);
            cmd.ReleaseTemporaryRT(m_PassDefaultTempTex.id);
            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }
    }

    CustomRenderPass myPass;
    public override void Create()
    {
        int passint = setting.material == null ? 1 : setting.material.passCount - 1;
        setting.passMax = Mathf.Clamp(setting.passMax, -1, passint);
        myPass = new CustomRenderPass(setting.profilerTag);
        myPass.renderPassEvent = setting.renderPassEvent;
        myPass.passMat = setting.material;
        myPass.passMatCount = setting.passMax;
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        myPass.Setup(renderer.cameraColorTarget);
        renderer.EnqueuePass(myPass);
    }
}