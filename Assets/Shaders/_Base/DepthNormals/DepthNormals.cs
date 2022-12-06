using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class DepthNormals : ScriptableRendererFeature
{
    class DepthNormalsPass : ScriptableRenderPass
    {

        int kDepthBufferBits = 32;
        private RenderTargetHandle depthAttachmentHandle { get; set; }
        private RenderTextureDescriptor descriptor { get; set; }

        private Material m_DepthNormalsMaterial;
        private FilteringSettings m_FilteringSettings;
        private string m_ProfilerTag = "DepthNormals Prepass";
        private ShaderTagId m_ShaderTagId = new ShaderTagId("DepthOnly");

        public DepthNormalsPass(RenderQueueRange renderQueueRange, LayerMask layerMask, Material material)
        {
            m_FilteringSettings = new FilteringSettings(renderQueueRange, layerMask);
            m_DepthNormalsMaterial = material;
        }

        public void Setup(RenderTextureDescriptor baseDescriptor, RenderTargetHandle depthAttachmentHandle)
        {
            this.depthAttachmentHandle = depthAttachmentHandle;
            baseDescriptor.colorFormat = RenderTextureFormat.ARGB32;
            baseDescriptor.depthBufferBits = kDepthBufferBits;
            this.descriptor = baseDescriptor;

        }

        public override void OnCameraSetup(CommandBuffer buffer, ref RenderingData renderingData)
        {
            buffer.GetTemporaryRT(depthAttachmentHandle.id, descriptor, FilterMode.Point);
            ConfigureTarget(depthAttachmentHandle.Identifier());
            ConfigureClear(ClearFlag.All, Color.black);
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            CommandBuffer buffer = CommandBufferPool.Get(m_ProfilerTag);
            // using (new ProfilingScope(buffer, new ProfilingSampler(m_ProfilerTag)))
            // {
                context.ExecuteCommandBuffer(buffer);
                buffer.Clear();

                var sortFlags = renderingData.cameraData.defaultOpaqueSortFlags;
                var drawSettings = CreateDrawingSettings(m_ShaderTagId, ref renderingData, sortFlags);
                drawSettings.perObjectData = PerObjectData.None;

                ref CameraData cameraData = ref renderingData.cameraData;
                Camera cameara = cameraData.camera;
                // if(cameraData.isStereoEnabled)
                //     context.StartMultiEye(cameara);

                drawSettings.overrideMaterial = m_DepthNormalsMaterial;
                context.DrawRenderers(renderingData.cullResults, ref drawSettings, ref m_FilteringSettings);
                buffer.SetGlobalTexture("_CameraDepthNormalsTexture", depthAttachmentHandle.id);
            // }

            context.ExecuteCommandBuffer(buffer);
            CommandBufferPool.Release(buffer);
        }
        
        public override void OnCameraCleanup(CommandBuffer buffer)
        {
            if (depthAttachmentHandle != RenderTargetHandle.CameraTarget)
            {
                buffer.ReleaseTemporaryRT(depthAttachmentHandle.id);
                depthAttachmentHandle = RenderTargetHandle.CameraTarget;
            }
        }
    }

    private DepthNormalsPass m_DepthNormalsPass;
    private RenderTargetHandle m_DepthNormalsTexture;
    private Material m_DepthNormalsMaterials;

    /// <inheritdoc/>
    public override void Create()
    {
        m_DepthNormalsMaterials = CoreUtils.CreateEngineMaterial("Practical-URP/Base/DepthNormalsTexture");
        m_DepthNormalsPass = new DepthNormalsPass(RenderQueueRange.opaque, -1, m_DepthNormalsMaterials);
        m_DepthNormalsPass.renderPassEvent = RenderPassEvent.AfterRenderingPrePasses;
        m_DepthNormalsTexture.Init("_CameraDepthNormalsTexture");
    }

    // Here you can inject one or multiple render passes in the renderer.
    // This method is called when setting up the renderer once per-camera.
    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        m_DepthNormalsPass.Setup(renderingData.cameraData.cameraTargetDescriptor, m_DepthNormalsTexture);
        renderer.EnqueuePass(m_DepthNormalsPass);
    }
}


