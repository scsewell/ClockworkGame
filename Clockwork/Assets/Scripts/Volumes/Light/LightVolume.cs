using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering.HDPipeline;
using Framework.Volumes;

public class LightVolume : Volume<LightVolume, LightVolumeManager>
{
    [Header("Profile")]

    private HDAdditionalLightData[] m_lights;

    private struct LightConfig
    {
        private readonly Light m_light;
        private readonly HDAdditionalLightData m_lightData;
        private readonly float m_dimmer;
        private readonly float m_volumeDimmer;

        public LightConfig(HDAdditionalLightData light)
        {
            m_light = light.GetComponent<Light>();
            m_lightData = light;
            m_dimmer = light.lightDimmer;
            m_volumeDimmer = light.volumetricDimmer;
        }

        public void Apply(float weight)
        {
            m_light.enabled = weight > 0;

            if (m_light.enabled)
            {
                m_lightData.lightDimmer = m_dimmer * weight;
                m_lightData.volumetricDimmer = m_volumeDimmer * weight;
            }
        }
    }

    private readonly List<LightConfig> m_lightConfigs = new List<LightConfig>();
    
    protected override void Awake()
    {
        base.Awake();

        m_lights = GetComponentsInChildren<HDAdditionalLightData>(true);
        
        foreach (var light in m_lights)
        {
            m_lightConfigs.Add(new LightConfig(light));
        }

        SetLightWeights(0);
    }

    public void SetLightWeights(float weight)
    {
        foreach (var light in m_lightConfigs)
        {
            light.Apply(weight);
        }
    }

#if UNITY_EDITOR
    protected override Color Color => Color.HSVToRGB(0.32f, 0.75f, 1.0f);

    private void OnDrawGizmos()
    {
        DrawGizmos();
    }
#endif
}

public class LightVolumeManager : VolumeManager<LightVolume, LightVolumeManager>
{
}
