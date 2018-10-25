using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneChanger : MonoBehaviour
{
    [SerializeField]
    [Range(0, 10)]
    private float m_fadeInTime = 5.0f;

    private Image m_fadeBlack;

    private void Awake()
    {
        m_fadeBlack = GetComponentInChildren<Image>(true);
    }

#if !UNITY_EDITOR
    private void Start()
    {
        StartCoroutine(LoadScene(1));
    }
#endif

    private IEnumerator LoadScene(int index)
    {
        AudioListener.pause = true;
        AudioListener.volume = 0f;

        m_fadeBlack.enabled = true;
        m_fadeBlack.color = new Color(0f, 0f, 0f, 1f);

        SceneManager.LoadScene(index, LoadSceneMode.Additive);

        // wait a few frames for everything to load nicely
        int frameCount = 16;
        while (frameCount > 0)
        {
            frameCount--;
            yield return null;
        }

        System.GC.Collect();
        AudioListener.pause = false;

        // fade in
        float duration = m_fadeInTime;
        while (duration > 0)
        {
            duration -= Time.deltaTime;
            float fac = 0.5f - (0.5f * Mathf.Cos(Mathf.Clamp01(duration / m_fadeInTime) * Mathf.PI));

            AudioListener.volume = 1f - fac;
            m_fadeBlack.color = new Color(0f, 0f, 0f, Mathf.Pow(fac, 0.33f));

            yield return null;
        }

        m_fadeBlack.enabled = false;
    }
}
