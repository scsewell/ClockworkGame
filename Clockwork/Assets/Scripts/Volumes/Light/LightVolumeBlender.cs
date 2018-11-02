using UnityEngine;
using Framework.Volumes;

public class LightVolumeBlender : VolumeBlender
{
    protected override void UpdateBlending(Transform target, VolumeLayer layer)
    {
        var profiles = LightVolumeManager.Instance.GetProfiles(target, layer);

        foreach (var profile in profiles)
        {
            profile.volume.SetLightWeights(profile.weight);
        }
    }
}
