using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class KawaseBlurFeature : ScriptableRendererFeature
{
    [System.Serializable]
    public class MySetting
    {
        public string profilerTag = "Kawase Blur";
        public RenderPassEvent renderPassEvent = RenderPassEvent.AfterRenderingTransparents;
        public Material material;
        [Range(2, 10)] public int downSample = 2;
        [Range(2, 10)] public int iteration = 2;
        [Range(0.5f, 5f)] public float blur = 0.5f;
        public int passMax = -1;
    }

    public MySetting setting = new MySetting();
    class CustomRenderPass : ScriptableRenderPass
    {
        public Material passMat = null;
        public int passMatCount = 0;
        public int passDownSample = 2;
        public int passIteration = 2;
        public float passBlur = 4;
        public FilterMode passFilterMode { get; set; }
        private RenderTargetIdentifier passSource { get; set; }
        private RenderTargetIdentifier m_tmpTargetId1;
        private RenderTargetIdentifier m_tmpTargetId2;
        private string passProfilerTag;

        public CustomRenderPass(string profilerTag)
        {
            passProfilerTag = profilerTag;
        }

        public void Setup(RenderTargetIdentifier sour)
        {
            this.passSource = sour;
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            int tmpPropertyId1 = Shader.PropertyToID("bufferBlur1");
            int tmpPropertyId2 = Shader.PropertyToID("bufferBlur2");
            CommandBuffer cmd = CommandBufferPool.Get(passProfilerTag);
            RenderTextureDescriptor opaqueDesc = renderingData.cameraData.cameraTargetDescriptor;
            int width = opaqueDesc.width / passDownSample;
            int height = opaqueDesc.height / passDownSample;
            opaqueDesc.depthBufferBits = 0;

            cmd.GetTemporaryRT(tmpPropertyId1, width, height, 0, FilterMode.Bilinear, RenderTextureFormat.ARGB32);
            cmd.GetTemporaryRT(tmpPropertyId2, width, height, 0, FilterMode.Bilinear, RenderTextureFormat.ARGB32);
            m_tmpTargetId1 = new RenderTargetIdentifier(tmpPropertyId1);
            m_tmpTargetId2 = new RenderTargetIdentifier(tmpPropertyId2);
            cmd.SetGlobalFloat("_Blur", 1f);
            cmd.Blit(passSource, m_tmpTargetId1, passMat, 0);

            for (int iter = 0; iter < passIteration; iter++)
            {
                cmd.SetGlobalFloat("_Blur", iter * passBlur + 1);
                cmd.Blit(m_tmpTargetId1, m_tmpTargetId2, passMat);
                var tmpTarget = m_tmpTargetId1;
                m_tmpTargetId1 = m_tmpTargetId2;
                m_tmpTargetId2 = tmpTarget;
            }

            cmd.SetGlobalFloat("_Blur", passIteration * passBlur + 1);
            cmd.Blit(m_tmpTargetId1, passSource, passMat, 0);

            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }
    }

    CustomRenderPass myPass;
    public override void Create()
    {
        myPass = new CustomRenderPass(setting.profilerTag);
        myPass.renderPassEvent = setting.renderPassEvent;
        myPass.passBlur = setting.blur;
        myPass.passIteration = setting.iteration;
        myPass.passMat = setting.material;
        myPass.passDownSample = setting.downSample;
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        myPass.Setup(renderer.cameraColorTarget);
        renderer.EnqueuePass(myPass);
    }
}