using UnityEngine;

public abstract class TwoBoneIK : MonoBehaviour, IConstraint
{
    [SerializeField]
    protected bool m_debug = false;

    [SerializeField]
    [Tooltip("The amount the root bone can move towards the goal to help the chain reach the target if it is too far away.")]
    [Range(0f, 0.5f)]
    protected float m_freeDistance = 0f;

    [SerializeField]
    [Tooltip("The largest angle in degrees allowed between the IK bones. Making this large can help avoid snapping when the goat is near the max distance the IK chain can reach.")]
    [Range(0f, 179f)]
    private float m_maxAngle = 179f;

    [SerializeField]
    [Tooltip("The strength of the IK effect.")]
    [Range(0f, 1f)]
    private float m_weight = 1f;

    /// <summary>
    /// The strength of the IK effect.
    /// </summary>
    public float Weight
    {
        get { return m_weight; }
        set {  m_weight = Mathf.Clamp01(value); }
    }

    /// <summary>
    /// The root bone in the IK chain.
    /// </summary>
    protected abstract Bone Bone1 { get; }
    /// <summary>
    /// The second bone in the IK chain.
    /// </summary>
    protected abstract Bone Bone2 { get; }
    /// <summary>
    /// The bone to place at the IK target at the end of the chain.
    /// </summary>
    protected abstract Bone Bone3 { get; }
    
    /// <summary>
    /// The maximum distance the IK chain can reach.
    /// </summary>
    public float MaxReach { get; private set; } = 0;

    /// <summary>
    /// The update order for the constraint. Lower values are evaluated first.
    /// </summary>
    public int UpdateOrder => -1000;


    public virtual void Initialize()
    {
    }

    /// <summary>
    /// Apply the IK constraint to the bones.
    /// </summary>
    public void UpdateConstraint()
    {
        MaxReach = ComputeChainLength() + m_freeDistance;
        UpdateIK();
    }

    public abstract void UpdateIK();

    /// <summary>
    /// Conducts a two bone IK pass. If the goal is to far to be reached, the root bone may 
    /// be allowed to move some distance to help the chain reach the goal.
    /// </summary>
    /// <param name="bone1">The root bone in the IK chain.</param>
    /// <param name="bone2">The second bone in the IK chain.</param>
    /// <param name="bone3">The bone to place at the target transform.</param>
    /// <param name="targetPos">The goal position.</param>
    /// <param name="targetRot">The goal rotation.</param>
    /// <param name="poleTarget">The point defining the direction of the bent in the chain.</param>
    /// <param name="weight">The strength of the IK constraint.</param>
    protected void DoIK(Vector3 targetPos, Quaternion targetRot, Vector3 poleTarget, float weight)
    {
        // get bone transforms
        Vector3 p1 = Bone1.Position;
        Vector3 p2 = Bone2.Position;
        Vector3 p3 = Bone3.Position;

        Quaternion r1 = Bone1.Rotation;
        Quaternion r2 = Bone2.Rotation;
        Quaternion r3 = Bone3.Rotation;

        // get bone lengths
        Vector3 a = p2 - p1;
        Vector3 b = p3 - p2;
        Vector3 c = targetPos - p1;

        float a2 = a.sqrMagnitude;
        float b2 = b.sqrMagnitude;
        float c2 = c.sqrMagnitude;

        float aLen = Mathf.Sqrt(a2);
        float bLen = Mathf.Sqrt(b2);
        float cLen = Mathf.Sqrt(c2);

        Vector3 cNorm = c / cLen;

        // find the maximum distance the IK chain can reach (Cosine Law)
        float chainLength = ComputeChainLength();

        // move target close enough to be reachable if it is too far
        Vector3 offset = Vector3.zero;
        if (cLen > chainLength)
        {
            offset = Mathf.Clamp(cLen - chainLength, 0f, m_freeDistance) * cNorm;

            cLen = chainLength;
            c2 = cLen * cLen;
            c = cLen * cNorm;
        }

        // compute new transforms for root position and target bone position
        Vector3 ik_p1 = p1 + offset;
        Vector3 ik_p3 = ik_p1 + c;
        Quaternion ik_r3 = targetRot;

#if UNITY_EDITOR
        if (m_debug)
        {
            Debug.DrawLine(p1, p2, Color.red);
            Debug.DrawLine(p2, p3, Color.red);
            Debug.DrawRay(p3, r3 * (0.1f * Vector3.left), Color.red);
        }
#endif

        // compute the angle from the target direction and middle joint direction
        float ang = Mathf.Acos((a2 + c2 - b2) / (2f * aLen * cLen)) * Mathf.Rad2Deg;
        
        // compute the direction for the middle joint
        Vector3 axis = new Plane(ik_p1, ik_p3, poleTarget).normal;
        Vector3 v = Quaternion.AngleAxis(ang, axis) * (aLen * cNorm);

        // compute knee position and bone rotations
        Vector3 ik_p2 = ik_p1 + v;
        Quaternion ik_r1 = Bone1.LookAt(v, axis);
        Quaternion ik_r2 = Bone2.LookAt(ik_p3 - ik_p2, axis);
        
        // blend IK by weight
        p1 = Vector3.Lerp(p1, ik_p1, weight);
        p2 = Vector3.Lerp(p2, ik_p2, weight);
        p3 = Vector3.Lerp(p3, ik_p3, weight);

        r1 = Quaternion.Slerp(r1, ik_r1, weight);
        r2 = Quaternion.Slerp(r2, ik_r2, weight);
        r3 = Quaternion.Slerp(r3, targetRot, weight);

        // update bones
        Bone1.SetTransform(p1, r1);
        Bone2.SetTransform(p2, r2);
        Bone3.SetTransform(p3, r3);

#if UNITY_EDITOR
        if (m_debug)
        {
            Debug.DrawLine(ik_p1, ik_p2, Color.magenta);
            Debug.DrawLine(ik_p2, ik_p3, Color.magenta);
            Debug.DrawRay(ik_p3, ik_r3 * (0.1f * Vector3.left), Color.magenta);

            Debug.DrawLine(ik_p1, ik_p3, Color.green);
            Debug.DrawLine(ik_p2, poleTarget, Color.green);
        }
#endif
    }

    /// <summary>
    /// Computes the maximum distance the IK chain is allowed to reach.
    /// </summary>
    private float ComputeChainLength()
    {
        // get bone transforms
        Vector3 p1 = Bone1.Position;
        Vector3 p2 = Bone2.Position;
        Vector3 p3 = Bone3.Position;

        // get bone lengths
        Vector3 a = p2 - p1;
        Vector3 b = p3 - p2;

        float a2 = a.sqrMagnitude;
        float b2 = b.sqrMagnitude;

        float aLen = Mathf.Sqrt(a2);
        float bLen = Mathf.Sqrt(b2);

        // find the maximum distance the IK chain can reach (Cosine Law)
        return Mathf.Sqrt(a2 + b2 - (2f * aLen * bLen * Mathf.Cos(m_maxAngle * Mathf.Deg2Rad)));
    }

}
