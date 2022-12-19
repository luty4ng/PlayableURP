using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[ExecuteInEditMode]
public class OutlineFeature : ScriptableRendererFeature
{
    public enum TYPE
    {
        INcolorON, INcolorOFF
    }

    [System.Serializable]
    public class setting
    {
        public Material mymat;
        public Color color = Color.blue;
        [Range(1000, 5000)] public int QueueMin = 2000;
        [Range(1000, 5000)] public int QueueMax = 2500;
        public LayerMask layer;
        public RenderPassEvent passEvent = RenderPassEvent.AfterRenderingSkybox;
        [Range(0.0f, 3.0f)] public float blur = 1.0f;
        [Range(1, 5)] public int passloop = 3;
        public TYPE ColorType = TYPE.INcolorON;
    }
    public setting mysetting = new setting();
    int solidcolorID;

    class SoildColorPass : ScriptableRenderPass
    {
        setting mysetting = null;
        OutlineFeature OutlineFeature = null;
        ShaderTagId shaderTag = new ShaderTagId("DepthOnly");
        FilteringSettings filter;
        public SoildColorPass(setting setting, OutlineFeature render)
        {
            mysetting = setting;
            OutlineFeature = render;
            // filter setting
            RenderQueueRange queue = new RenderQueueRange();
            queue.lowerBound = Mathf.Min(setting.QueueMax, setting.QueueMin);
            queue.upperBound = Mathf.Max(setting.QueueMax, setting.QueueMin);
            filter = new FilteringSettings(queue, setting.layer);
        }

        public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
        {
            int temp = Shader.PropertyToID("_MyTempColor1");
            RenderTextureDescriptor desc = cameraTextureDescriptor;
            cmd.GetTemporaryRT(temp, desc);
            OutlineFeature.solidcolorID = temp;
            ConfigureTarget(temp);
            ConfigureClear(ClearFlag.All, Color.black);
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            mysetting.mymat.SetColor("_SoildColor", mysetting.color);
            CommandBuffer cmd = CommandBufferPool.Get("Capture Solid Color");
            var drawSetting = CreateDrawingSettings(shaderTag, ref renderingData, renderingData.cameraData.defaultOpaqueSortFlags);
            drawSetting.overrideMaterial = mysetting.mymat;
            drawSetting.overrideMaterialPassIndex = 0;
            context.DrawRenderers(renderingData.cullResults, ref drawSetting, ref filter);
            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }

    }

    class OutlinePass : ScriptableRenderPass
    {
        setting mysetting = null;
        OutlineFeature OutlineFeature = null;
        struct LEVEL
        {
            public int down;
            public int up;
        };

        LEVEL[] my_level;
        int maxLevel = 16;
        RenderTargetIdentifier sour;
        public OutlinePass(setting setting, OutlineFeature render, RenderTargetIdentifier source)
        {
            mysetting = setting;
            OutlineFeature = render;
            sour = source;
            my_level = new LEVEL[maxLevel];
            for (int t = 0; t < maxLevel; t++)
            {
                my_level[t] = new LEVEL
                {
                    down = Shader.PropertyToID("_BlurMipDown" + t),
                    up = Shader.PropertyToID("_BlurMipUp" + t)
                };
            }

            if (mysetting.ColorType == TYPE.INcolorON)
            {
                mysetting.mymat.EnableKeyword("_INCOLORON");
                mysetting.mymat.DisableKeyword("_INCOLOROFF");
            }
            else
            {
                mysetting.mymat.EnableKeyword("_INCOLOROFF");
                mysetting.mymat.DisableKeyword("_INCOLORON");
            }
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            CommandBuffer cmd = CommandBufferPool.Get("Outline");
            RenderTextureDescriptor desc = renderingData.cameraData.cameraTargetDescriptor;
            int SourID = Shader.PropertyToID("_SourTex");
            cmd.GetTemporaryRT(SourID, desc);
            cmd.CopyTexture(sour, SourID);
            int BlurID = Shader.PropertyToID("_BlurTex");
            cmd.GetTemporaryRT(BlurID, desc);
            mysetting.mymat.SetFloat("_Blur", mysetting.blur);

            int width = desc.width / 2;
            int height = desc.height / 2;
            int LastDown = OutlineFeature.solidcolorID;

            for (int t = 0; t < mysetting.passloop; t++)
            {
                int midDown = my_level[t].down;
                int midUp = my_level[t].up;
                cmd.GetTemporaryRT(midDown, width, height, 0, FilterMode.Bilinear, RenderTextureFormat.ARGB32);
                cmd.GetTemporaryRT(midUp, width, height, 0, FilterMode.Bilinear, RenderTextureFormat.ARGB32);
                cmd.Blit(LastDown, midDown, mysetting.mymat, 1);
                LastDown = midDown;
                width = Mathf.Max(width / 2, 1);
                height = Mathf.Max(height / 2, 1);
            }

            int lastUp = my_level[mysetting.passloop - 1].down;
            for (int j = mysetting.passloop - 2; j >= 0; j--)
            {
                int midUp = my_level[j].up;
                cmd.Blit(lastUp, midUp, mysetting.mymat, 2);
                lastUp = midUp;
            }

            cmd.Blit(lastUp, BlurID, mysetting.mymat, 2);
            cmd.Blit(OutlineFeature.solidcolorID, sour, mysetting.mymat, 3);
            context.ExecuteCommandBuffer(cmd);

            for (int k = 0; k < mysetting.passloop; k++)
            {
                cmd.ReleaseTemporaryRT(my_level[k].up);
                cmd.ReleaseTemporaryRT(my_level[k].down);
            }

            cmd.ReleaseTemporaryRT(BlurID);
            cmd.ReleaseTemporaryRT(SourID);
            cmd.ReleaseTemporaryRT(OutlineFeature.solidcolorID);
            CommandBufferPool.Release(cmd);
        }
    }

    SoildColorPass m_DrawSoildColorPass;
    OutlinePass m_OutlinePass;
    public override void Create()
    {
        m_DrawSoildColorPass = new SoildColorPass(mysetting, this);
        m_DrawSoildColorPass.renderPassEvent = RenderPassEvent.AfterRenderingPrePasses;
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (mysetting.mymat != null)
        {
            RenderTargetIdentifier sour = renderer.cameraColorTarget;
            renderer.EnqueuePass(m_DrawSoildColorPass);
            m_OutlinePass = new OutlinePass(mysetting, this, sour);
            m_OutlinePass.renderPassEvent = mysetting.passEvent;
            renderer.EnqueuePass(m_OutlinePass);
        }
        else
        {
            Debug.LogError("材质球丢失！请设置材质球");
        }
    }
}