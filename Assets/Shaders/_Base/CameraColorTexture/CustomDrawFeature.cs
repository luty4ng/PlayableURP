using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class CustomDrawFeature : ScriptableRendererFeature
{
    [System.Serializable]
    public class Setting
    {
        public string profilerTag = "DrawAfterTransparents";
        public string shaderTagId = "AfterTransparents";
        public RenderPassEvent renderPassEvent = RenderPassEvent.AfterRenderingTransparents;
    }

    public Setting setting = new Setting();
    class CustomDrawPass : ScriptableRenderPass
    {
        private RenderTargetIdentifier source { get; set; }
        private RenderTargetHandle destination { get; set; }
        private FilteringSettings m_FilteringSettings;
        public string m_ProfilerTag = "DrawAfterTransparents";
        public ShaderTagId m_ShaderTagId = new ShaderTagId("AfterTransparents");
        public CustomDrawPass(RenderPassEvent passEvent)
        {
            renderPassEvent = passEvent;
            m_FilteringSettings = new FilteringSettings(RenderQueueRange.opaque, -1);
        }

        public void Setup(RenderTargetIdentifier sourceId)
        {
            source = sourceId;
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            CommandBuffer buffer = CommandBufferPool.Get(m_ProfilerTag);
            context.ExecuteCommandBuffer(buffer);
            buffer.Clear();

            var sortedFlags = renderingData.cameraData.defaultOpaqueSortFlags;
            var drawSettings = CreateDrawingSettings(m_ShaderTagId, ref renderingData, sortedFlags);
            drawSettings.perObjectData = PerObjectData.None;

            ref CameraData cameraData = ref renderingData.cameraData;
            Camera camera = cameraData.camera;
            context.DrawRenderers(renderingData.cullResults, ref drawSettings, ref m_FilteringSettings);
            context.ExecuteCommandBuffer(buffer);
            CommandBufferPool.Release(buffer);
        }
    }

    CustomDrawPass m_ScriptablePass;

    public override void Create()
    {
        m_ScriptablePass = new CustomDrawPass(setting.renderPassEvent);
        m_ScriptablePass.m_ProfilerTag = setting.profilerTag;
        m_ScriptablePass.m_ShaderTagId = new ShaderTagId(setting.shaderTagId);
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        m_ScriptablePass.Setup(renderer.cameraColorTarget);
        renderer.EnqueuePass(m_ScriptablePass);
    }
}


