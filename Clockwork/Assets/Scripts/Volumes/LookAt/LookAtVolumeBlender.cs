using UnityEngine;
using Framework.Volumes;

public class LookAtVolumeBlender : VolumeBlender
{
    private CharacterAnimation m_anim;

    protected override void Awake()
    {
        base.Awake();
        m_anim = GetComponent<CharacterAnimation>();
    }

    protected override void UpdateBlending(Transform target, VolumeLayer layer)
    {
        if (m_anim != null)
        {
            var profiles = LootAtVolumeManager.Instance.GetProfiles(target, layer);

            if (profiles.Count > 0)
            {
                m_anim.LookAtTarget = profiles[0].volume.target;
                m_anim.LookAtWeight = profiles[0].weight;
            }
            else
            {
                m_anim.LookAtTarget = null;
                m_anim.LookAtWeight = 0f;
            }
        }
    }
}
