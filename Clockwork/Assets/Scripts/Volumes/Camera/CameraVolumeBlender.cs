using UnityEngine;
using Framework.Volumes;

public class CameraVolumeBlender : VolumeBlender
{
    private Camera m_cam;

    protected override void Awake()
    {
        base.Awake();
        m_cam = GetComponent<Camera>();
    }

    protected override void UpdateBlending(Transform target, VolumeLayer layer)
    {
        if (m_cam != null && m_cam.enabled)
        {
            var profiles = CameraVolumeManager.Instance.GetProfiles(target, layer);

            for (int i = 0; i < profiles.Count; i++)
            {
                var profileBlend = profiles[i];
                var volume = profileBlend.volume;
                var virCam = volume.target;
                
                if (i == 0)
                {
                    transform.position = virCam.transform.position;
                    transform.rotation = virCam.transform.rotation;
                    m_cam.fieldOfView = virCam.m_Lens.FieldOfView;
                    m_cam.nearClipPlane = virCam.m_Lens.NearClipPlane;
                    m_cam.farClipPlane = virCam.m_Lens.FarClipPlane;
                }
                else
                {
                    transform.position = Vector3.Lerp(transform.position, virCam.transform.position, profileBlend.weight);
                    transform.rotation = Quaternion.Slerp(transform.rotation, virCam.transform.rotation, profileBlend.weight);
                    m_cam.fieldOfView = Mathf.Lerp(m_cam.fieldOfView, virCam.m_Lens.FieldOfView, profileBlend.weight);
                    m_cam.nearClipPlane = Mathf.Lerp(m_cam.nearClipPlane, virCam.m_Lens.NearClipPlane, profileBlend.weight);
                    m_cam.farClipPlane = Mathf.Lerp(m_cam.farClipPlane, virCam.m_Lens.FarClipPlane, profileBlend.weight);
                }
            }
        }
    }
}
