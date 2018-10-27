using UnityEngine;

public class ArmIK : TwoBoneIK
{
    [SerializeField]
    [Tooltip("The time in seconds over which the bones smoothly switch to a new target.")]
    [Range(0f, 2f)]
    private float m_blendDuration = 0.2f;

    [Header("Bones")]

    [SerializeField]
    private Bone m_upperArm;
    [SerializeField]
    private Bone m_forearm;
    [SerializeField]
    private Bone m_hand;

    private Bone[] m_bones = null;
    private HandAnchor m_target = null;
    private bool m_targetChanged = false;
    private float m_oldTargetBlend = 0;
    private float m_hasTargetBlend = 0;

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

    public Vector3 ShoulderPosition => m_upperArm.Position;
    public override float MaxReach => GetMaxDistance(m_upperArm, m_forearm, m_hand);

    public override void Initialize()
    {
        m_bones = new Bone[] { m_upperArm, m_forearm, m_hand };

        foreach (Bone bone in m_bones)
        {
            bone.StoreBlendTransform();
        }
    }

    public override void UpdateConstraint()
    {
        if (m_targetChanged)
        {
            // Remember the transform looking at the previous target
            foreach (Bone bone in m_bones)
            {
                bone.StoreBlendTransform();
            }
            m_oldTargetBlend = 1f;
            m_targetChanged = false;
        }

        m_hasTargetBlend = Mathf.MoveTowards(m_hasTargetBlend, Target != null ? 1f : 0f, Time.deltaTime / m_blendDuration);
        float hasTargetBlend = Mathf.SmoothStep(0f, 1f, m_hasTargetBlend);
        
        Vector3 pos = Target != null ? Target.GetHandPosition() : Vector3.zero;
        Quaternion rot = Target != null ? Target.GetHandRotation() : Quaternion.identity;

        DoIK(m_upperArm, m_forearm, m_hand, pos, rot, Weight * hasTargetBlend);

        // blend from the previous transforms for smooth target swtiches
        m_oldTargetBlend = Mathf.MoveTowards(m_oldTargetBlend, 0f, Time.deltaTime / m_blendDuration);
        float oldTargetBlend = Mathf.SmoothStep(0f, 1f, m_oldTargetBlend);

        foreach (Bone bone in m_bones)
        {
            bone.ApplyBlendTransform(oldTargetBlend);
            bone.StoreLastTransform();
        }
    }
}
