using UnityEngine;
using Framework.Volumes;

public class DustVolume : Volume<DustVolume, DustVolumeManager>
{
    [Header("Profile")]
    
    public Color color = Color.black;
    public Vector3 windMin = Vector3.zero;
    public Vector3 windMax = Vector3.zero;

#if UNITY_EDITOR
    protected override Color Color => Color.HSVToRGB(0.95f, 0.75f, 1.0f);

    private void OnDrawGizmos()
    {
        DrawGizmos();
    }
#endif
}

public class DustVolumeManager : VolumeManager<DustVolume, DustVolumeManager>
{
}
