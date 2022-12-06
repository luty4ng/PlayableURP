using UnityEngine;

using UnityEngine.Rendering;

using UnityEngine.Rendering.Universal;

public class DualKawaseBlurFeature : ScriptableRendererFeature

{

    [System.Serializable]
    public class MySetting
    {
        public string passProfilerTag = "Dual Kawase Blur";
        public RenderPassEvent passEvent = RenderPassEvent.AfterRenderingTransparents;
        public Material material;
        [Range(1, 8)] public int downSample = 2;
        [Range(2, 8)] public int iteration = 2;
        [Range(0.5f, 5f)] public float blur = 0.5f;

    }

    public MySetting setting = new MySetting();

    class CustomRenderPass : ScriptableRenderPass
    {
        public Material passMat = null;
        public int passDownSample = 2;
        public int passIteration = 2;
        public float passBlur = 4;
        private RenderTargetIdentifier passSource { get; set; }
        private RenderTargetIdentifier m_TmpTarget1;
        private RenderTargetIdentifier m_TmpTarget2;
        private string passProfilerTag;
        struct SampleMode
        {
            public int down;
            public int up;
        };

        SampleMode[] sampleModePerLevel;
        int MaxLevel = 16;
        public CustomRenderPass(string name)
        {
            passProfilerTag = name;
        }

        public void Setup(RenderTargetIdentifier sour)
        {
            this.passSource = sour;
            sampleModePerLevel = new SampleMode[MaxLevel];
            for (int t = 0; t < MaxLevel; t++) // apply for 32 property id, used as RT
            {
                sampleModePerLevel[t] = new SampleMode
                {
                    down = Shader.PropertyToID("_BlurMipDown" + t),
                    up = Shader.PropertyToID("_BlurMipUp" + t)
                };
            }

        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)//执行
        {
            CommandBuffer cmd = CommandBufferPool.Get(passProfilerTag);
            // passMat.SetFloat("_Blur", passBlur);
            cmd.SetGlobalFloat("_Blur", passBlur);
            RenderTextureDescriptor opaqueDesc = renderingData.cameraData.cameraTargetDescriptor;
            int width = opaqueDesc.width / passDownSample;
            int height = opaqueDesc.height / passDownSample;
            opaqueDesc.depthBufferBits = 0;

            // Down Sampling
            RenderTargetIdentifier lastDown = passSource;
            for (int t = 0; t < passIteration; t++)
            {
                int currentDown = sampleModePerLevel[t].down;
                int currentUp = sampleModePerLevel[t].up;
                cmd.GetTemporaryRT(currentDown, width, height, 0, FilterMode.Bilinear, RenderTextureFormat.ARGB32);
                cmd.GetTemporaryRT(currentUp, width, height, 0, FilterMode.Bilinear, RenderTextureFormat.ARGB32);
                cmd.Blit(lastDown, currentDown, passMat, 0);
                lastDown = currentDown;
                width = Mathf.Max(width / 2, 1);
                height = Mathf.Max(height / 2, 1);
            }

            // Up Sampling
            int lastUp = sampleModePerLevel[passIteration - 1].down;
            for (int j = passIteration - 2; j >= 0; j--) // reason for minus 2: line 74 and line 95
            {
                int currentUp = sampleModePerLevel[j].up;
                cmd.Blit(lastUp, currentUp, passMat, 1);
                lastUp = currentUp;
            }
            cmd.Blit(lastUp, passSource, passMat, 1);

            // Release
            for (int k = 0; k < passIteration; k++)
            {
                cmd.ReleaseTemporaryRT(sampleModePerLevel[k].up);
                cmd.ReleaseTemporaryRT(sampleModePerLevel[k].down);
            }
            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }

    }

    CustomRenderPass mypass;
    public override void Create()
    {
        mypass = new CustomRenderPass(setting.passProfilerTag);
        mypass.renderPassEvent = setting.passEvent;
        mypass.passBlur = setting.blur;
        mypass.passIteration = setting.iteration;
        mypass.passMat = setting.material;
        mypass.passDownSample = setting.downSample;
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)//传值到pass里
    {
        mypass.Setup(renderer.cameraColorTarget);
        renderer.EnqueuePass(mypass);
    }

}