using System.Text;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Profiling;

public class SystemHUD : MonoBehaviour
{
    [SerializeField]
    [Range(1, 200)]
    private int m_sampleCount = 50;
    
    [SerializeField]
    private Gradient m_gradient;

    [SerializeField]
    [Range(0, 120)]
    private int m_gradientMinFps = 30;

    [SerializeField]
    [Range(0, 120)]
    private int m_gradientMaxFps = 60;

    private readonly Queue<float> m_samples = new Queue<float>();
    private readonly StringBuilder m_sb  = new StringBuilder();
    private Canvas m_canvas;
    private Text m_text;
    private float m_lastTime;
    private float m_avgTime;
    private float m_minTime;
    private float m_maxTime;

    private bool m_active = false;
    public bool Active
    {
        get { return m_active; }
        set
        {
            if (m_active != value)
            {
                m_active = value;
                m_canvas.enabled = m_active;
                m_samples.Clear();
            }
        }
    }

    private void Awake()
    {
        m_canvas = GetComponentInChildren<Canvas>(true);
        m_text = GetComponentInChildren<Text>(true);

        m_canvas.enabled = false;
    }
    
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F1))
        {
            Active = !Active;
        }

        if (Active)
        {
            m_lastTime = Time.deltaTime;

            // add the current time sample
            m_samples.Enqueue(m_lastTime);
            if (m_samples.Count > m_sampleCount)
            {
                m_samples.Dequeue();
            }

            // Find the frame times
            m_minTime = m_lastTime;
            m_maxTime = m_lastTime;
            m_avgTime = 0;
            foreach (float sample in m_samples)
            {
                if (m_minTime > sample)
                {
                    m_minTime = sample;
                }
                if (m_maxTime < sample)
                {
                    m_maxTime = sample;
                }
                m_avgTime += sample;
            }
            m_avgTime /= m_sampleCount;
        }
    }

    private void LateUpdate()
    {
        if (Active)
        {
            m_sb.Clear();

            AppendFps("FPS", m_lastTime);
            AppendFps("AVG", m_avgTime);
            AppendFps("MIN", m_maxTime, false, false);
            AppendFps("MAX", m_minTime, false);

            m_sb.Append("MEM USED: ");
            m_sb.Append(Mathf.CeilToInt(Profiler.GetMonoUsedSizeLong() / (1024 * 1024)));
            m_sb.Append(" MB\n");

            m_sb.Append("MEM ALLOCATED: ");
            m_sb.Append(Mathf.CeilToInt(Profiler.GetMonoHeapSizeLong() / (1024 * 1024)));
            m_sb.Append(" MB");

            m_text.text = m_sb.ToString();
        }
    }

    private void AppendFps(string text, float time, bool showFrameTime = true, bool newLine = true)
    {
        Color color = m_gradient.Evaluate(Mathf.Clamp01(Mathf.InverseLerp(m_gradientMinFps, m_gradientMaxFps, 1f / time)));
        string fpsStr = Mathf.RoundToInt(1f / time).ToString();

        if (showFrameTime)
        {
            AppendText(color, text, ": ", fpsStr, " [", (time * 1000).ToString("0.0"), " ms]");
        }
        else
        {
            AppendText(color, text, ": ", fpsStr);
        }

        m_sb.Append("  ");
        if (newLine)
        {
            m_sb.Append("\n");
        }
    }

    private void AppendText(Color color, params string[] texts)
    {
        m_sb.Append("<color=#");
        m_sb.Append(ColorUtility.ToHtmlStringRGBA(color));
        m_sb.Append(">");
        foreach (string text in texts)
        {
            m_sb.Append(text);
        }
        m_sb.Append("</color>");
    }
}
