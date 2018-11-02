using UnityEngine;
using Framework.Volumes;

public class DustVolumeBlender : VolumeBlender
{
    private ParticleSystem m_dust;
    private Material m_dustMat;

    protected override void Awake()
    {
        base.Awake();
        m_dust = GetComponent<ParticleSystem>();

        ParticleSystemRenderer renderer = GetComponent<ParticleSystemRenderer>();
        m_dustMat = new Material(renderer.material);
        renderer.material = m_dustMat;
    }

    protected override void UpdateBlending(Transform target, VolumeLayer layer)
    {
        if (m_dust != null)
        {
            var profiles = DustVolumeManager.Instance.GetProfiles(target, layer);
            
            Color color = new Color(0f, 0f, 0f, 0f);
            Vector3 windMin = Vector3.zero;
            Vector3 windMax = Vector3.zero;

            for (int i = 0; i < profiles.Count; i++)
            {
                var profileBlend = profiles[i];
                var volume = profileBlend.volume;
                var weight = profileBlend.weight;

                color = Color.Lerp(color, volume.color, weight);
                windMin = Vector3.Lerp(windMin, volume.windMin, weight);
                windMax = Vector3.Lerp(windMax, volume.windMax, weight);
            }

            m_dustMat.SetColor("_Color", color);

            var velocity = m_dust.velocityOverLifetime;
            velocity.x = SetVelocity(velocity.x, windMin.x, windMax.x);
            velocity.y = SetVelocity(velocity.y, windMin.y, windMax.y);
            velocity.z = SetVelocity(velocity.z, windMin.z, windMax.z);

            var emission = m_dust.emission;
            emission.rateOverTimeMultiplier = color.a <= float.Epsilon ? 0f : 1000f;
        }
    }

    private ParticleSystem.MinMaxCurve SetVelocity(ParticleSystem.MinMaxCurve axis, float minSpeed, float maxSpeed)
    {
        axis.constantMin = minSpeed;
        axis.constantMax = maxSpeed;
        return axis;
    }
}

