using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class CustomDrawFeature : ScriptableRendererFeature
{
    [System.Serializable]
    public class Setting
    {
        public string profilerTag;
        public ShaderTagId shaderTagId;
        public RenderPassEvent renderPassEvent = RenderPassEvent.AfterRenderingTransparents;
    }
    public Setting setting = new Setting();

    class CustomDrawPass : ScriptableRenderPass
    {
        private RenderTargetIdentifier source { get; set; }
        private RenderTargetHandle destination { get; set; }
        // private RenderTargetHandle m_TemporaryColorTexture;
        private FilteringSettings m_FilteringSettings;
        public string m_ProfilerTag = "DrawAfterTransparents";
        public ShaderTagId m_ShaderTagId = new ShaderTagId("AfterTransparents");
        public CustomDrawPass(RenderPassEvent passEvent)
        {
            renderPassEvent = passEvent;
            m_FilteringSettings = new FilteringSettings(RenderQueueRange.opaque, -1);
            // m_TemporaryColorTexture.Init("_CameraColorTexture");
        }

        public void Setup(RenderTargetIdentifier sourceId)
        {
            source = sourceId;
        }

        // This method is called before executing the render pass.
        // It can be used to configure render targets and their clear state. Also to create temporary render target textures.
        // When empty this render pass will render to the active camera render target.
        // You should never call CommandBuffer.SetRenderTarget. Instead call <c>ConfigureTarget</c> and <c>ConfigureClear</c>.
        // The render pipeline will ensure target setup and clearing happens in a performant manner.
        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {

        }

        // Here you can implement the rendering logic.

        // Use <c>ScriptableRenderContext</c> to issue drawing commands or execute command buffers
        // https://docs.unity3d.com/ScriptReference/Rendering.ScriptableRenderContext.html
        // You don't have to call ScriptableRenderContext.submit, the render pipeline will call it at specific points in the pipeline.
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
            // }

            context.ExecuteCommandBuffer(buffer);
            CommandBufferPool.Release(buffer);
        }

        // Cleanup any allocated resources that were created during the execution of this render pass.
        public override void OnCameraCleanup(CommandBuffer cmd)
        {

        }
    }

    CustomDrawPass m_ScriptablePass;

    /// <inheritdoc/>
    public override void Create()
    {
        m_ScriptablePass = new CustomDrawPass(setting.renderPassEvent);
        m_ScriptablePass.m_ProfilerTag = setting.profilerTag;
        m_ScriptablePass.m_ShaderTagId = setting.shaderTagId;
    }

    // Here you can inject one or multiple render passes in the renderer.
    // This method is called when setting up the renderer once per-camera.
    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        m_ScriptablePass.Setup(renderer.cameraColorTarget);
        renderer.EnqueuePass(m_ScriptablePass);
    }
}


