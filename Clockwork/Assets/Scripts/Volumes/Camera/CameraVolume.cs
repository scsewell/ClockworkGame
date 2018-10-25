using UnityEngine;
using Framework.Volumes;
using Cinemachine;

public class CameraVolume : Volume<CameraVolume, CameraVolumeManager>
{
    [Header ("Profile")]

    public CinemachineVirtualCamera cam = null;
    public bool followPlayerCenter = true;
    public Transform target = null;

    protected override void Awake()
    {
        base.Awake();

        if (cam == null)
        {
            cam = GetComponentInChildren<CinemachineVirtualCamera>();
            if (cam == null)
            {
                Debug.LogWarning($"Camera volume \"{name}\" does not have a camera assigned and will have no effect.");
            }
        }
    }

    private void Update()
    {
        if (cam != null)
        {
            cam.Follow = followPlayerCenter ? Main.Instance.playerCenterOfMass : target;
        }
    }

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
