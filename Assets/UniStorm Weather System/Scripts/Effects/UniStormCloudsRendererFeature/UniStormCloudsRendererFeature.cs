using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace UniStorm.Utility
{
    public class UniStormCloudsRendererFeature : ScriptableRendererFeature
    {
        class UniStormCloudsRenderPass : ScriptableRenderPass
        {
            private UniStormClouds m_UniStormClouds;
            private ProfilingSampler m_ProfilingSampler = new ProfilingSampler("UniStorm Clouds");

            public UniStormCloudsRenderPass(UniStormClouds uniStormClouds)
            {
                this.m_UniStormClouds = uniStormClouds;
                renderPassEvent = RenderPassEvent.AfterRenderingSkybox;
            }

            [System.Obsolete]
            public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
            {
#if UNITY_EDITOR
                if (UnityEditor.EditorApplication.isPaused) return;
#endif

                if (m_UniStormClouds == null || !m_UniStormClouds.enabled) return;

                CameraData cameraData = renderingData.cameraData;
                Camera camera = cameraData.camera;

                //Skip execution for the Scene View camera or if not in runtime
                if (cameraData.cameraType == CameraType.SceneView || !Application.isPlaying) return;

                if (!UniStormSystem.Instance.UniStormInitialized) return;

                CommandBuffer cmd = CommandBufferPool.Get("UniStorm Clouds");
                using (new ProfilingScope(cmd, m_ProfilingSampler))
                {
                    // 1. Render the first clouds buffer - lower resolution
                    cmd.Blit(null, m_UniStormClouds.lowResCloudsBuffer, m_UniStormClouds.skyMaterial, 0);

                    // 2. Blend between low and hi-res
                    cmd.SetGlobalTexture("_uLowresCloudTex", m_UniStormClouds.lowResCloudsBuffer);
                    cmd.SetGlobalTexture("_uPreviousCloudTex", m_UniStormClouds.fullCloudsBuffer[m_UniStormClouds.fullBufferIndex]);
                    cmd.Blit(m_UniStormClouds.fullCloudsBuffer[m_UniStormClouds.fullBufferIndex], m_UniStormClouds.fullCloudsBuffer[m_UniStormClouds.fullBufferIndex ^ 1], m_UniStormClouds.skyMaterial, 1);

                    switch (m_UniStormClouds.CloudShadowsTypeRef)
                    {
                        case UniStormClouds.CloudShadowsType.Off:
                            break;
                        case UniStormClouds.CloudShadowsType.Simulated:

                            // Low performance light cookie noise
                            m_UniStormClouds.shadowsBuildingMaterial.SetFloat("_uCloudsCoverage", m_UniStormClouds.skyMaterial.GetFloat("_uCloudsCoverage"));
                            m_UniStormClouds.shadowsBuildingMaterial.SetFloat("_uCloudsCoverageBias", m_UniStormClouds.skyMaterial.GetFloat("_uCloudsCoverageBias"));
                            m_UniStormClouds.shadowsBuildingMaterial.SetFloat("_uCloudsDensity", m_UniStormClouds.skyMaterial.GetFloat("_uCloudsDensity"));
                            m_UniStormClouds.shadowsBuildingMaterial.SetFloat("_uCloudsDetailStrength", m_UniStormClouds.skyMaterial.GetFloat("_uCloudsDetailStrength"));
                            m_UniStormClouds.shadowsBuildingMaterial.SetFloat("_uCloudsBaseEdgeSoftness", m_UniStormClouds.skyMaterial.GetFloat("_uCloudsBaseEdgeSoftness"));
                            m_UniStormClouds.shadowsBuildingMaterial.SetFloat("_uCloudsBottomSoftness", m_UniStormClouds.skyMaterial.GetFloat("_uCloudsBottomSoftness"));
                            m_UniStormClouds.shadowsBuildingMaterial.SetFloat("_uSimulatedCloudAlpha", m_UniStormClouds.cloudTransparency);

                            cmd.Blit(GenerateNoise.baseNoiseTexture, m_UniStormClouds.cloudShadowsBuffer[0], m_UniStormClouds.shadowsBuildingMaterial, 3);
                            m_UniStormClouds.PublicCloudShadowTexture = m_UniStormClouds.cloudShadowsBuffer[0];

                            break;
                        case UniStormClouds.CloudShadowsType.RealTime:

                            cmd.Blit(m_UniStormClouds.fullCloudsBuffer[m_UniStormClouds.fullBufferIndex ^ 1], m_UniStormClouds.cloudShadowsBuffer[0]);
                            for (int i = 0; i < m_UniStormClouds.shadowBlurIterations; i++)
                            {
                                cmd.Blit(m_UniStormClouds.cloudShadowsBuffer[0], m_UniStormClouds.cloudShadowsBuffer[1], m_UniStormClouds.shadowsBuildingMaterial, 1);
                                cmd.Blit(m_UniStormClouds.cloudShadowsBuffer[1], m_UniStormClouds.cloudShadowsBuffer[0], m_UniStormClouds.shadowsBuildingMaterial, 2);
                            }

                            break;
                        default:
                            break;
                    }

                    cmd.SetGlobalFloat("_uLightning", 0.0f);

                    // 3. Set to material for the sky (not in the command buffer)
                    m_UniStormClouds.cloudsMaterial.SetTexture("_MainTex", m_UniStormClouds.fullCloudsBuffer[m_UniStormClouds.fullBufferIndex ^ 1]);
                }
                context.ExecuteCommandBuffer(cmd);
                CommandBufferPool.Release(cmd);
            }
        }

        UniStormCloudsRenderPass m_ScriptablePass;

        public override void Create()
        {
            // Initialization deferred to AddRenderPasses
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            UniStormClouds uniStormClouds = FindAnyObjectByType<UniStormClouds>();
            if (uniStormClouds == null)
                return;

            if (m_ScriptablePass == null)
            {
                m_ScriptablePass = new UniStormCloudsRenderPass(uniStormClouds);
            }

            renderer.EnqueuePass(m_ScriptablePass);
        }
    }
}
