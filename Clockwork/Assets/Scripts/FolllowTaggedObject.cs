using UnityEngine;

public class FolllowTaggedObject : MonoBehaviour
{
    private Transform m_t;

    private void Start()
    {
        GameObject go = GameObject.FindGameObjectWithTag("Player");
        if (go != null)
        {
            m_t = go.transform;
        }
    }
    
    private void Update()
    {
        transform.position = m_t.position;
        transform.rotation = m_t.rotation;
    }
}
