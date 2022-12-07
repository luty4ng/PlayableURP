using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class RenderMaskFeature : ScriptableRendererFeature
{
    [System.Serializable]
    public class MySetting
    {
        public string profilerTag = "Render Mask";
        public RenderPassEvent renderRassEvent = RenderPassEvent.AfterRenderingOpaques;
        public LayerMask LayerMask;
        public Material Material;
    }

    private class RenderMaskRenderPass : ScriptableRenderPass
    {
        private readonly static int m_MaskColorId = Shader.PropertyToID("_MaskSoildColor");
        private ShaderTagId m_ShaderTag = new ShaderTagId("UniversalForward");
        private MySetting m_Setting;
        private FilteringSettings m_FilteringSettings;
        private string passProfilerTag;

        public RenderMaskRenderPass(MySetting setting)
        {
            this.m_Setting = setting;
            m_FilteringSettings = new FilteringSettings(RenderQueueRange.all, this.m_Setting.LayerMask);
            passProfilerTag = setting.profilerTag;
        }

        public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
        {
            RenderTextureDescriptor opaqueDesc = new RenderTextureDescriptor(128, 128);
            cmd.GetTemporaryRT(m_MaskColorId, opaqueDesc);
            ConfigureTarget(m_MaskColorId);
            ConfigureClear(ClearFlag.All, Color.black);
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            CommandBuffer cmd = CommandBufferPool.Get(passProfilerTag);
            using (new ProfilingScope(cmd, new ProfilingSampler(passProfilerTag)))
            {
                var drawSettings = CreateDrawingSettings(m_ShaderTag, ref renderingData,
                    renderingData.cameraData.defaultOpaqueSortFlags);
                drawSettings.overrideMaterial = m_Setting.Material;
                drawSettings.overrideMaterialPassIndex = 0;
                context.DrawRenderers(renderingData.cullResults, ref drawSettings, ref m_FilteringSettings);
            }
            CommandBufferPool.Release(cmd);
        }

        public override void FrameCleanup(CommandBuffer cmd)
        {

        }
    }

    public MySetting Setting;
    private RenderMaskRenderPass m_ScriptableMaskRenderPass;

    public override void Create()
    {
        m_ScriptableMaskRenderPass = new RenderMaskRenderPass(this.Setting);
        m_ScriptableMaskRenderPass.renderPassEvent = this.Setting.renderRassEvent;
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        renderer.EnqueuePass(m_ScriptableMaskRenderPass);
    }
}