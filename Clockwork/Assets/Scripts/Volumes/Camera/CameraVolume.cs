using UnityEngine;
using Framework.Volumes;
using Cinemachine;

public class CameraVolume : Volume<CameraVolume, CameraVolumeManager>
{
    public CinemachineVirtualCamera target = null;

#if UNITY_EDITOR
    protected override Color Color => Color.HSVToRGB(0.15f, 0.75f, 1.0f);

    private void OnDrawGizmos()
    {
        DrawGizmos();
    }
#endif
}

public class CameraVolumeManager : VolumeManager<CameraVolume, CameraVolumeManager>
{
}
