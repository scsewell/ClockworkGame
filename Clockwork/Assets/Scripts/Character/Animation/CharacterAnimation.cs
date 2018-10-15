using UnityEngine;

public class CharacterAnimation : MonoBehaviour
{
    private IRigConstraint[] m_contraints;

    private void Awake()
    {
        m_contraints = GetComponentsInChildren<IRigConstraint>(true);
    }
    
    public void VisualUpdate()
    {
        for (int i = 0; i < m_contraints.Length; i++)
        {
            m_contraints[i].UpdateConstraint();
        }
    }
}
