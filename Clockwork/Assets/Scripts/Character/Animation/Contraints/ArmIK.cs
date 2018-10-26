using UnityEngine;

public class ArmIK : TwoBoneIK
{
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

    private HandAnchor m_target = null;

    public HandAnchor Target
    {
        get { return m_target; }
        set
        {
            if (m_target != value)
            {
                if (m_target != null)
                {
                    m_target.occupied = false;
                }

                m_target = value;
                m_targetChanged = true;

                if (m_target != null)
                {
                    m_target.occupied = true;
                }
            }
        }
    }

    private bool m_targetChanged = false;
    private float m_oldTargetBlend = 0;
    private float m_hasTargetBlend = 0;

    public Vector3 ShoulderPosition => m_upperArm.Position;
    public override float MaxReach => GetMaxDistance(m_upperArm, m_forearm, m_hand);

    public override void Initialize()
    {
        m_upperArm.StoreBlendTransform();
        m_forearm.StoreBlendTransform();
        m_hand.StoreBlendTransform();
    }

    public override void UpdateConstraint()
    {
        if (m_targetChanged)
        {
            // Remember the transform pointing to the previous target
            m_upperArm.StoreBlendTransform();
            m_forearm.StoreBlendTransform();
            m_hand.StoreBlendTransform();

            m_oldTargetBlend = 0;

            m_targetChanged = false;
        }

        m_hasTargetBlend = Mathf.MoveTowards(m_hasTargetBlend, Target != null ? 1f : 0f, Time.deltaTime / m_blendDuration);
        float hasTargetBlend = Mathf.SmoothStep(0f, 1f, m_hasTargetBlend);
        
        m_oldTargetBlend = Mathf.MoveTowards(m_oldTargetBlend, 1f, Time.deltaTime / m_blendDuration);
        float oldTargetBlend = Mathf.SmoothStep(0f, 1f, m_oldTargetBlend);

        Vector3 pos = Target != null ? Target.GetHandPosition() : Vector3.zero;
        Quaternion rot = Target != null ? Target.GetHandRotation() : Quaternion.identity;

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
