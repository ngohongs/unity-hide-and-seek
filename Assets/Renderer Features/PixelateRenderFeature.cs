using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class PixelateRenderFeature : ScriptableRendererFeature
{
    [System.Serializable]
    public class Settings
    {
        [Tooltip("Pixelation material")]
        public Material pixelateMaterial;
        [Tooltip("When should the pixelation material be applied")]
        public RenderPassEvent renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;
        [HideInInspector]
        [Tooltip("DEPRECATED! Screen Pixel Height should be used. How should the pixelation be strong")]
        [Min(1)] public int pixelationScale = 1;
        [Tooltip("Downscaling to render target with height of")]
        public int screenPixelHeight = 480;
        [Tooltip("Outline strength / visibility")]
        [Range(0,1)] public float outlineStrength = 1;
        [Tooltip("Banding effect strength")]
        [Min(1)] public float bandingFactor = 100;
        [Tooltip("Depth outline sensitivity - depth value is first multiplied and then raised to the value of bias")]
        [Min(0)] public float depthOutlineMultiplier = 1;
        [Tooltip("Depth outline sensitivity - depth value is first multiplied and then raised to the value of bias")]
        [Min(0)] public float depthOutlineBias = 1;
        [Tooltip("Normal outline sensitivity  outline value is first multiplied and then raised to the value of bias")]
        [Min(0)] public float normalOutlineMultiplier = 1;
        [Tooltip("Normal outline sensitivity - outline value is first multiplied and then raised to the value of bias")]
        [Min(0)] public float normalOutlineBias = 1;
        public bool showOutlineOnly = false;
        public bool showDepthOutline = true;
        public bool showNormalOutline = true;

    }

    public Settings settings = new Settings();

    class CustomRenderPass : ScriptableRenderPass
    {
        private RenderTargetIdentifier source;

        private RenderTargetHandle pixelateTexture;

        private Material pixelateMaterial;

        private int pixelationScale;
        private int screenPixelHeight;
        private float outlineStrength;
        private float depthOutlineMultiplier;
        private float depthOutlineBias;
        private float normalOutlineMultiplier;
        private float normalOutlineBias;
        private float bandingFactor;
        private bool showOutlineOnly;
        private bool showDepthOutline;
        private bool showNormalOutline;

        public CustomRenderPass(Material pixelate, PixelateRenderFeature.Settings settings)
        {
            this.pixelateMaterial = settings.pixelateMaterial;
            this.pixelationScale = settings.pixelationScale;
            this.screenPixelHeight = settings.screenPixelHeight;
            this.outlineStrength = settings.outlineStrength;
            this.depthOutlineMultiplier = settings.depthOutlineMultiplier;
            this.depthOutlineBias = settings.depthOutlineBias;
            this.normalOutlineMultiplier = settings.normalOutlineMultiplier;
            this.normalOutlineBias = settings.normalOutlineBias;
            this.bandingFactor = settings.bandingFactor;
            this.showOutlineOnly = settings.showOutlineOnly;
            this.showDepthOutline = settings.showDepthOutline;
            this.showNormalOutline = settings.showNormalOutline;
        pixelateTexture.Init("_PixelateTexture");
        }

        public void SetSource(RenderTargetIdentifier source)
        {
            this.source = source;
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
            CommandBuffer cmd = CommandBufferPool.Get("PixelateFeature");

            int scale = Mathf.Clamp(pixelationScale, 1, 100);
            RenderTextureDescriptor pixelateTextureDesc = renderingData.cameraData.cameraTargetDescriptor;
            pixelateTextureDesc.depthBufferBits = 0;
            pixelateTextureDesc.width = (int) Mathf.Round(Camera.main.aspect * screenPixelHeight);
            pixelateTextureDesc.height = screenPixelHeight;
            cmd.GetTemporaryRT(pixelateTexture.id, pixelateTextureDesc, FilterMode.Point);

            cmd.SetGlobalInteger("_PixelationScale", scale);
            cmd.SetGlobalInteger("_SmallScreenWidth", pixelateTextureDesc.width);
            cmd.SetGlobalInteger("_SmallScreenHeight", pixelateTextureDesc.height);
            cmd.SetGlobalFloat("_OutlineStrength", outlineStrength);
            cmd.SetGlobalFloat("_DepthOutlineMultiplier", depthOutlineMultiplier);
            cmd.SetGlobalFloat("_DepthOutlineBias", depthOutlineBias);
            cmd.SetGlobalFloat("_NormalOutlineMultiplier", normalOutlineMultiplier);
            cmd.SetGlobalFloat("_NormalOutlineBias", normalOutlineBias);
            cmd.SetGlobalFloat("_BandingFactor", bandingFactor);
            cmd.SetGlobalInt("_ShowOutlineOnly", showOutlineOnly ? 1 : 0);
            cmd.SetGlobalInt("_ShowDepthOutline", showDepthOutline ? 1 : 0);
            cmd.SetGlobalInt("_ShowNormalOutline", showNormalOutline ? 1 : 0);

            Blit(cmd, source, pixelateTexture.Identifier(), pixelateMaterial);
            Blit(cmd, pixelateTexture.Identifier(), source);

            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);

        }

        // Cleanup any allocated resources that were created during the execution of this render pass.
        public override void FrameCleanup(CommandBuffer cmd)
        {
            cmd.ReleaseTemporaryRT(pixelateTexture.id);
        }
    }

    CustomRenderPass m_ScriptablePass;

    /// <inheritdoc/>
    public override void Create()
    {
        // Configures where the render pass should be injected.
        var pixelateMaterial = new Material(Shader.Find("Pixelate/Pixelate"));
        m_ScriptablePass = new CustomRenderPass(pixelateMaterial, settings);
        m_ScriptablePass.renderPassEvent = settings.renderPassEvent;
    }

    // Here you can inject one or multiple render passes in the renderer.
    // This method is called when setting up the renderer once per-camera.
    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        m_ScriptablePass.SetSource(renderer.cameraColorTarget);
        renderer.EnqueuePass(m_ScriptablePass);
    }
}


