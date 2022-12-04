using System.Net.NetworkInformation;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class ScannerRenderFeature : ScriptableRendererFeature
{
    [System.Serializable]
    public class Setting
    {
        public Material material;
        public RenderPassEvent renderPassEvent = RenderPassEvent.AfterRenderingTransparents;
    }
    public Setting setting = new Setting();

    public class ScannerPass : ScriptableRenderPass
    {
        public Material material;
        private RenderTargetIdentifier sourceId;
        public void SetSourceId(RenderTargetIdentifier sourceId)
        {
            this.sourceId = sourceId;
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            int dest = Shader.PropertyToID("destination");
            int matrixId = Shader.PropertyToID("_CornerMatrix");
            CommandBuffer buffer = CommandBufferPool.Get("Reconstruct World Space Position");
            RenderTextureDescriptor desc = renderingData.cameraData.cameraTargetDescriptor;
            Camera camera = renderingData.cameraData.camera;

            // using the nearest plane
            float nearScale = camera.nearClipPlane * Mathf.Tan(Mathf.Deg2Rad * camera.fieldOfView * 0.5f);
            Vector3 toTop = camera.transform.up * nearScale;
            Vector3 toRight = camera.transform.right * nearScale * camera.aspect;
            Vector3 toForward = camera.transform.forward * camera.nearClipPlane;

            Vector3 BottomLeft = toForward - toRight - toTop;
            // the value equals to Secant(正割) of FOV/2
            float scale = BottomLeft.magnitude / camera.nearClipPlane;
            BottomLeft.Normalize();
            BottomLeft *= scale;
            Vector3 BottomRight = toForward + toRight - toTop;
            BottomRight.Normalize();
            BottomRight *= scale;
            Vector3 TopRight = toForward + toRight + toTop;
            TopRight.Normalize();
            TopRight *= scale;
            Vector3 TopLeft = toForward - toRight + toTop;
            TopLeft.Normalize();
            TopLeft *= scale;

            // pass as matrix to sha der
            Matrix4x4 QuadMatrix = new Matrix4x4();
            QuadMatrix.SetRow(0, BottomLeft);
            QuadMatrix.SetRow(1, BottomRight);
            QuadMatrix.SetRow(2, TopRight);
            QuadMatrix.SetRow(3, TopLeft);
            buffer.SetGlobalMatrix(matrixId, QuadMatrix);
            context.ExecuteCommandBuffer(buffer);
            // material.SetMatrix("_CornerMatrix", QuadMatrix);

            // blit
            buffer.GetTemporaryRT(dest, desc);
            buffer.Blit(sourceId, dest, material);
            buffer.Blit(dest, sourceId);
            context.ExecuteCommandBuffer(buffer);
            buffer.ReleaseTemporaryRT(dest);
            CommandBufferPool.Release(buffer);
        }
    }

    private ScannerPass m_ScannerPass;
    public ScannerPass GetPass()
    {
        return m_ScannerPass;
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        m_ScannerPass.SetSourceId(renderer.cameraColorTarget);
        renderer.EnqueuePass(m_ScannerPass);
    }

    public override void Create()
    {
        m_ScannerPass = new ScannerPass();
        m_ScannerPass.material = setting.material;
        m_ScannerPass.renderPassEvent = setting.renderPassEvent;
    }
}