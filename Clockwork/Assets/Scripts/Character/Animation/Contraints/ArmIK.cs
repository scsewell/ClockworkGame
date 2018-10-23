using UnityEngine;

public class ArmIK : TwoBoneIK
{
    [SerializeField]
    private Transform m_target = null;

    [Header("Bones")]

    [SerializeField]
    private Bone m_upperArm;
    [SerializeField]
    private Bone m_forearm;
    [SerializeField]
    private Bone m_hand;

    /// <summary>
    /// The goal of the IK contraint.
    /// </summary>
    public Transform Target
    {
        get { return m_target; }
        set { m_target = value; }
    }

    public override void UpdateConstraint()
    {
        if (m_target != null && Weight > 0)
        {
            DoIK(m_upperArm, m_forearm, m_hand, m_target.position, m_target.rotation);
        }
    }
}
