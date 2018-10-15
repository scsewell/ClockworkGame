using UnityEngine;

public class PlayMusic : MonoBehaviour
{
    [SerializeField]
    private MusicParams m_music;

    private void Start()
    {
        AudioManager.Instance.PlayMusic(m_music);
    }
}
