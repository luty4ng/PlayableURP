using System;
using System.Dynamic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class BokehBlurFeature : ScriptableRendererFeature
{
    [System.Serializable]
    public class MySetting
    {
        public string profilerTag = "Bokeh Blur";
        public RenderPassEvent renderPassEvent = RenderPassEvent.AfterRenderingTransparents;
        public Material material;
        [Range(1, 10)] public int downSample = 2;
        [Range(3, 500)] public int iteration = 50;
        [Range(0.1f, 10)] public int radius = 1;
        [Range(0, 0.5f)] public float BlurSmoothness = 0.1f;
        public float NearCullDis = 5;
        public float FarCullDis = 9;
    }

    public MySetting setting = new MySetting();
    class CustomRenderPass : ScriptableRenderPass
    {
        public Material passMat = null;
        public int passDownSample;
        public int passIteration;
        public int passRadius;
        public float passBlurSmoothness;
        public float passNearCullDis;
        public float passFarCullDis;
        private int width;
        private int height;
        public FilterMode passFilterMode { get; set; }
        private RenderTargetIdentifier passSource { get; set; }
        private string passProfilerTag;
        private readonly static int BlurId = Shader.PropertyToID("Blur");
        private readonly static int SourceId = Shader.PropertyToID("_SourceTex");
        public CustomRenderPass(string tag)
        {
            this.passProfilerTag = tag;
        }

        public void Setup(RenderTargetIdentifier sour)
        {
            this.passSource = sour;
            passMat.SetFloat("_Iteration", passIteration);
            passMat.SetFloat("_Radius", passRadius);
            passMat.SetFloat("_NearDis", passNearCullDis);
            passMat.SetFloat("_FarDis", passFarCullDis);
            passMat.SetFloat("_BlurSmoothness", passBlurSmoothness);
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            CommandBuffer cmd = CommandBufferPool.Get(passProfilerTag);
            RenderTextureDescriptor opaqueDesc = renderingData.cameraData.cameraTargetDescriptor;
            width = opaqueDesc.width / passDownSample;
            height = opaqueDesc.height / passDownSample;
            opaqueDesc.depthBufferBits = 0;
            cmd.GetTemporaryRT(BlurId, width, height, 0, FilterMode.Bilinear, RenderTextureFormat.ARGB32);
            cmd.GetTemporaryRT(SourceId, opaqueDesc);
            cmd.CopyTexture(passSource, SourceId);
            cmd.Blit(passSource, BlurId, passMat, 0);   // BACKUP
            cmd.Blit(BlurId, passSource, passMat, 1);   // BLEND
            cmd.ReleaseTemporaryRT(BlurId);
            cmd.ReleaseTemporaryRT(SourceId);
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
        myPass.passIteration = setting.iteration;
        myPass.passDownSample = setting.downSample;
        myPass.passRadius = setting.radius;
        myPass.passBlurSmoothness = setting.BlurSmoothness;
        myPass.passNearCullDis = Mathf.Max(setting.NearCullDis, 0);
        myPass.passFarCullDis = Mathf.Max(setting.NearCullDis, setting.FarCullDis);
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        myPass.Setup(renderer.cameraColorTarget);
        renderer.EnqueuePass(myPass);
    }
}