using UnityEngine;

public class ArmIK : TwoBoneIK
{
    [SerializeField]
    [Tooltip("The goal of the IK chain.")]
    public Transform target = null;

    [SerializeField]
    [Tooltip("The time in seconds over which the bones smoothly switch to the new target.")]
    [Range(0f, 2f)]
    private float m_blendDuration = 0.2f;

    [Header("Bones")]

    [SerializeField]
    private Bone m_upperArm;
    [SerializeField]
    private Bone m_forearm;
    [SerializeField]
    private Bone m_hand;

    private Transform m_lastTarget = null;
    private float m_oldTargetBlend = 0;
    private float m_hasTargetBlend = 0;

    public override void Initialize()
    {
        m_upperArm.StoreBlendTransform();
        m_forearm.StoreBlendTransform();
        m_hand.StoreBlendTransform();
    }

    public override void UpdateConstraint()
    {
        if (m_lastTarget != target)
        {
            // Remember the transform pointing to the previous target
            m_upperArm.StoreBlendTransform();
            m_forearm.StoreBlendTransform();
            m_hand.StoreBlendTransform();

            m_oldTargetBlend = 0;

            m_lastTarget = target;
        }

        m_hasTargetBlend = Mathf.MoveTowards(m_hasTargetBlend, target != null ? 1f : 0f, Time.deltaTime / m_blendDuration);
        float hasTargetBlend = Mathf.SmoothStep(0f, 1f, m_hasTargetBlend);
        
        m_oldTargetBlend = Mathf.MoveTowards(m_oldTargetBlend, 1f, Time.deltaTime / m_blendDuration);
        float oldTargetBlend = Mathf.SmoothStep(0f, 1f, m_oldTargetBlend);

        Vector3 pos = target != null ? target.position : Vector3.zero;
        Quaternion rot = target != null ? target.rotation : Quaternion.identity;

        DoIK(m_upperArm, m_forearm, m_hand, pos, rot, Weight * hasTargetBlend);
        
        // blend from the previous transforms for smooth target swtiches
        m_upperArm.ApplyGoalTransform(oldTargetBlend);
        m_forearm.ApplyGoalTransform(oldTargetBlend);
        m_hand.ApplyGoalTransform(oldTargetBlend);

        m_upperArm.StoreLastTransform();
        m_forearm.StoreLastTransform();
        m_hand.StoreLastTransform();
    }
}
