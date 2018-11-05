using UnityEngine;

public class Cart : MonoBehaviour
{
    [SerializeField]
    private Rigidbody[] m_wheels;

    [SerializeField]
    private AudioSource m_wheelSound;

    [SerializeField]
    [Range(0f, 1f)]
    private float m_wheelVolumeScale = 0.1f;

    [SerializeField]
    [Range(0.01f, 1f)]
    private float m_wheelPitchScale = 0.2f;

    [SerializeField]
    [Range(0.01f, 1f)]
    private float m_wheelVolumeSmoothing = 0.2f;


    private Rigidbody m_body;

    private void Awake()
    {
        m_body = GetComponent<Rigidbody>();
    }
    
    private void Update()
    {
        // determine roughly how much the wheels are rotating relative to the cart body
        float angVel = 0;
        foreach (Rigidbody body in m_wheels)
        {
            angVel += (m_body.angularVelocity - body.angularVelocity).magnitude;
        }

        // blend to the a new volume and pitch
        float targetVolume = (m_wheelVolumeScale * angVel) / m_wheels.Length;
        float targetPitch = (m_wheelPitchScale * angVel) / m_wheels.Length;

        float smoothing = Time.deltaTime / m_wheelVolumeSmoothing;
        m_wheelSound.volume = Mathf.Lerp(m_wheelSound.volume, targetVolume, smoothing);
        m_wheelSound.pitch = Mathf.Lerp(m_wheelSound.pitch, targetPitch, smoothing);

        // don't play the sound while it wouldn't be audible
        if (m_wheelSound.isPlaying && m_wheelSound.volume < 0.001f)
        {
            m_wheelSound.Pause();
        }
        else if (!m_wheelSound.isPlaying)
        {
            m_wheelSound.Play();
        }
    }
}
