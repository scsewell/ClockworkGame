using UnityEngine;

public class MotorSound : MonoBehaviour
{
    [SerializeField]
    [Range(0, 10)]
    private float m_volumeScale = 1.0f;

    [SerializeField]
    [Range(0.01f, 1.0f)]
    private float m_volumeSmoothing = 0.15f;

    private AudioSource m_audio;
    private Quaternion m_lastRot;

    private void Start()
    {
        m_audio = GetComponent<AudioSource>();
        m_lastRot = transform.localRotation;
    }

    private void LateUpdate()
    {
        Quaternion rot = transform.localRotation;
        float targetVol =  0.005f * m_volumeScale * Mathf.Abs(Quaternion.Angle(m_lastRot, rot)) / Time.deltaTime;
        m_audio.volume = Mathf.Lerp(m_audio.volume, targetVol, Time.deltaTime / m_volumeSmoothing);
        m_lastRot = rot;
    }
}
