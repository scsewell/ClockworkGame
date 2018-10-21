using UnityEngine;
using Framework.Volumes;

public class LookAtVolumeBlender : VolumeBlender
{
    protected override void UpdateBlending(Transform target, VolumeLayer layer)
    {
        CharacterAnimation anim = target.GetComponent<CharacterAnimation>();
        
        if (anim != null)
        {
            var profiles = LootAtVolumeManager.Instance.GetProfiles(target, layer);

            if (profiles.Count > 0)
            {
                anim.LookAtTarget = profiles[0].volume.target;
                anim.LookAtWeight = profiles[0].weight;
            }
            else
            {
                anim.LookAtTarget = null;
                anim.LookAtWeight = 0f;
            }
        }
    }
}
