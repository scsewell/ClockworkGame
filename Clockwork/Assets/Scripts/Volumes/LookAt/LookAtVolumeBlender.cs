using UnityEngine;
using Framework.Volumes;

public class LookAtVolumeBlender : VolumeBlender
{
    private RigLookAtConstraint m_lookAt;

    protected override void Awake()
    {
        base.Awake();
        m_lookAt = GetComponent<RigLookAtConstraint>();
    }

    protected override void UpdateBlending(Transform target, VolumeLayer layer)
    {
        if (m_lookAt != null)
        {
            var profiles = LootAtVolumeManager.Instance.GetProfiles(target, layer);

            if (profiles.Count > 0)
            {
                m_lookAt.Target = profiles[0].volume.target;
                m_lookAt.Weight = profiles[0].weight;
            }
            else
            {
                m_lookAt.Target = null;
                m_lookAt.Weight = 0f;
            }
        }
    }
}
