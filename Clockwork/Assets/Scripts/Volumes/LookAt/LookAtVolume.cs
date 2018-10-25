using UnityEngine;
using Framework.Volumes;

public class LookAtVolume : Volume<LookAtVolume, LootAtVolumeManager>
{
    [Header("Profile")]

    public Transform target = null;

#if UNITY_EDITOR
    protected override Color Color => Color.HSVToRGB(0.75f, 0.75f, 1.0f);

    private void OnDrawGizmos()
    {
        DrawGizmos();
    }
#endif
}

public class LootAtVolumeManager : VolumeManager<LookAtVolume, LootAtVolumeManager>
{
}
