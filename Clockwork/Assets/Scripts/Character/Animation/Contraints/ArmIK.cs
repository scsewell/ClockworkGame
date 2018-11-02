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

    [SerializeField]
    private Transform m_poleTarget = null;

    [SerializeField]
    [Tooltip("Position offset applied to the hand relative to anchor positions.")]
    private Vector3 m_handOffset = Vector3.zero;

    private Bone[] m_bones = null;
    private HandAnchor m_target = null;
    private bool m_targetChanged = false;
    private float m_oldTargetBlend = 0;
    private float m_hasTargetBlend = 0;

    public bool IsGrabbingTarget { get; private set; } = false;
    public Vector3 ShoulderPosition => m_upperArm.Position;

    protected override Bone Bone1 => m_upperArm;
    protected override Bone Bone2 => m_forearm;
    protected override Bone Bone3 => m_hand;

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
    
    public override void Initialize()
    {
        m_bones = new Bone[] { m_upperArm, m_forearm, m_hand };

        foreach (Bone bone in m_bones)
        {
            bone.StoreBlendTransform();
        }
    }

    public override void UpdateIK()
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

        Vector3 pos = Vector3.zero;
        Quaternion rot = Quaternion.identity;

        if (Target != null)
        {
            pos = Target.GetHandPosition();

            Transform chest = m_upperArm.transform.parent;
            float chestDirFac = Vector3.Dot(pos - chest.position, chest.forward);
            Vector3 handDir = pos - (chest.position + (chest.rotation * new Vector3(-0.2f, 0, -0.35f)));
            
            rot = Target.GetHandRotation(m_hand, handDir);
            pos += rot * m_handOffset;
        }

        DoIK(pos, rot, m_poleTarget.position, Weight * hasTargetBlend);

        // check if the hand is at the target
        IsGrabbingTarget = Vector3.Distance(m_hand.Position, pos) < 0.01f;

        // blend from the previous transforms for smooth target swtiches
        m_oldTargetBlend = Mathf.MoveTowards(m_oldTargetBlend, 0f, Time.deltaTime / m_blendDuration);
        float oldTargetBlend = Mathf.SmoothStep(0f, 1f, m_oldTargetBlend);

        foreach (Bone bone in m_bones)
        {
            bone.ApplyBlendTransform(oldTargetBlend);
            bone.StoreLastTransform();
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (m_debug)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(m_poleTarget.position, 0.05f);
        }
    }
#endif
}
