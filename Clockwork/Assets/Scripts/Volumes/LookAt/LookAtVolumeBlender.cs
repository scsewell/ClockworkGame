using Framework.Volumes;

public class LookAtVolumeBlender : VolumeBlender
{
    private CharacterAnimation m_anim;

    protected override void Awake()
    {
        base.Awake();
        m_anim = m_target.GetComponentInChildren<CharacterAnimation>(true);
    }

    private void Update()
    {
        var profiles = LootAtVolumeManager.Instance.GetProfiles(m_target, m_layer);

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
