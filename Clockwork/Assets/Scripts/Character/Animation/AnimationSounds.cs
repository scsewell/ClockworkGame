using UnityEngine;
using Framework;

public class AnimationSounds : MonoBehaviour
{
    [SerializeField]
    private AudioSource m_foot;

    [SerializeField]
    private AudioClip[] m_footSounds;

    public void Step()
    {
        if (m_foot != null && m_footSounds.Length > 0)
        {
            m_foot.PlayOneShot(m_footSounds.PickRandom());
        }
    }
}
