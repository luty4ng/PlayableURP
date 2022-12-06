using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class CameraColorTextureFeature : ScriptableRendererFeature
{
    private string m_ProfilerTag;
    class CameraColorTexturePass : ScriptableRenderPass
    {
        private RenderTargetIdentifier source { get; set; }
        private RenderTargetHandle destination { get; set; }
        private RenderTargetHandle m_TemporaryColorTexture;
        private string m_ProfilerTag;
        public CameraColorTexturePass(RenderPassEvent passEvent)
        {
            renderPassEvent = passEvent;
            m_ProfilerTag = "CameraColorTexture";
            m_TemporaryColorTexture.Init("_CameraColorTextureAlpha");
        }

        public void Setup(RenderTargetIdentifier sourceId)
        {
            source = sourceId;
        }

        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {

        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            CommandBuffer buffer = CommandBufferPool.Get(m_ProfilerTag);
            RenderTextureDescriptor opaqueDesc = renderingData.cameraData.cameraTargetDescriptor;
            // opaqueDesc.depthBufferBits = 0;
            buffer.GetTemporaryRT(m_TemporaryColorTexture.id, opaqueDesc, FilterMode.Bilinear);
            Blit(buffer, source, m_TemporaryColorTexture.Identifier());
            buffer.SetGlobalTexture("_CameraColorTextureAlpha", m_TemporaryColorTexture.id);
            buffer.ReleaseTemporaryRT(m_TemporaryColorTexture.id);
            context.ExecuteCommandBuffer(buffer);
            CommandBufferPool.Release(buffer);
        }
        
        public override void OnCameraCleanup(CommandBuffer cmd)
        {

        }
    }

    public RenderPassEvent renderPassEvent = RenderPassEvent.AfterRenderingPostProcessing;

    CameraColorTexturePass m_ScriptablePass;

    /// <inheritdoc/>
    public override void Create()
    {
        m_ScriptablePass = new CameraColorTexturePass(renderPassEvent);
    }

    // Here you can inject one or multiple render passes in the renderer.
    // This method is called when setting up the renderer once per-camera.
    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        m_ScriptablePass.Setup(renderer.cameraColorTarget);
        renderer.EnqueuePass(m_ScriptablePass);
    }
}


