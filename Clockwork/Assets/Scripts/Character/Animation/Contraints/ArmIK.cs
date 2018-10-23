using UnityEngine;

public class ArmIK : TwoBoneIK
{
    [SerializeField]
    private Transform m_upperArm;
    [SerializeField]
    private Transform m_forearm;
    [SerializeField]
    private Transform m_hand;

    public Transform target = null;

    private Vector3 m_targetPosition;
    private Quaternion m_targetRotation;

    public override void UpdateConstraint()
    {
        if (Weight > 0)
        {
            if (target != null)
            {
                m_targetPosition = target.position;
                m_targetRotation = target.rotation;
            }

            DoIK(m_upperArm, m_forearm, m_hand, m_targetPosition, m_targetRotation);
        }
    }

    public void SetTarget(Transform transform)
    {
        target = transform;
    }

    public void SetTarget(Vector3 position, Quaternion rotation)
    {
        m_targetPosition = position;
        m_targetRotation = rotation;
        target = null;
    }
}
