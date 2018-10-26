using UnityEngine;

public abstract class TwoBoneIK : MonoBehaviour, IRigConstraint
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
    /// The update order for the constraint. Lower values are evaluated first.
    /// </summary>
    public int UpdateOrder => -1000;

    /// <summary>
    /// The maximum distance the IK chain can reach.
    /// </summary>
    public abstract float MaxReach { get; }

    public virtual void Initialize()
    {
    }

    /// <summary>
    /// Apply the IK constraint to the bones.
    /// </summary>
    public abstract void UpdateConstraint();

    protected float GetMaxDistance(Bone bone1, Bone bone2, Bone bone3)
    {
        // get bone transforms
        Vector3 p1 = bone1.Position;
        Vector3 p2 = bone2.Position;
        Vector3 p3 = bone3.Position;
        
        // get bone lengths
        Vector3 a = p2 - p1;
        Vector3 b = p3 - p2;

        float a2 = a.sqrMagnitude;
        float b2 = b.sqrMagnitude;

        float aLen = Mathf.Sqrt(a2);
        float bLen = Mathf.Sqrt(b2);

        // find the maximum distance the IK chain can reach (Cosine Law)
        float chainLength = Mathf.Sqrt(a2 + b2 - (2f * aLen * bLen * Mathf.Cos(m_maxAngle * Mathf.Deg2Rad)));
        return chainLength + m_freeDistance;
    }

    /// <summary>
    /// Conducts a two bone IK pass. If the goal is to far to be reached, the root bone may 
    /// be allowed to move some distance to help the chain reach the goal.
    /// </summary>
    /// <param name="bone1">The root bone in the IK chain.</param>
    /// <param name="bone2">The second bone in the IK chain.</param>
    /// <param name="bone3">The bone to place at the target transform.</param>
    /// <param name="targetPos">The goal position.</param>
    /// <param name="targetRot">The goal rotation.</param>
    /// <param name="weight">The strength of the IK constraint.</param>
    protected void DoIK(Bone bone1, Bone bone2, Bone bone3, Vector3 targetPos, Quaternion targetRot, float weight)
    {
        // get bone transforms
        Vector3 p1 = bone1.Position;
        Vector3 p2 = bone2.Position;
        Vector3 p3 = bone3.Position;

        Quaternion r1 = bone1.Rotation;
        Quaternion r2 = bone2.Rotation;
        Quaternion r3 = bone3.Rotation;

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
        float chainLength = Mathf.Sqrt(a2 + b2 - (2f * aLen * bLen * Mathf.Cos(m_maxAngle * Mathf.Deg2Rad)));

        // move target close enough to be reachable if it is too far
        Vector3 offset = Vector3.zero;
        float offsetDistance = Mathf.Clamp(cLen - chainLength, 0f, m_freeDistance);
        if (cLen > chainLength)
        {
            offset = offsetDistance * cNorm;

            cLen = chainLength;
            c2 = cLen * cLen;
            c = cLen * cNorm;
        }

        targetPos = p1 + c + (offsetDistance * cNorm);

#if UNITY_EDITOR
        if (m_debug)
        {
            Debug.DrawLine(p1, p2, Color.red);
            Debug.DrawLine(p2, p3, Color.red);
            Debug.DrawRay(p3, r3 * (0.1f * Vector3.left), Color.red);
            Debug.DrawLine(p1, targetPos, Color.green);
        }
#endif

        // compute the angle from the target direction and middle joint direction
        float ang = Mathf.Acos((a2 + c2 - b2) / (2f * aLen * cLen)) * Mathf.Rad2Deg;

        // compute the direction for the middle joint
        Vector3 cross = Vector3.Cross(b, a);
        float angle = Vector3.SignedAngle(b, a, cross);

        Vector3 v = Quaternion.AngleAxis(ang, cross) * (aLen * cNorm);

        // compute new transforms
        Vector3 ik_p1 = p1 + offset;
        Vector3 ik_p2 = ik_p1 + v;
        
        Quaternion iK_r1 = bone1.LookAt(v, cross);
        Quaternion ik_r2 = bone2.LookAt(targetPos - ik_p2, cross);

        // blend IK by weight
        p1 = Vector3.Lerp(p1, ik_p1, weight);
        p2 = Vector3.Lerp(p2, ik_p2, weight);
        p3 = Vector3.Lerp(p3, targetPos, weight);

        r1 = Quaternion.Slerp(r1, iK_r1, weight);
        r2 = Quaternion.Slerp(r2, ik_r2, weight);
        r3 = Quaternion.Slerp(r3, targetRot, weight);

        // update bones
        bone1.SetGoalTransform(p1, r1);
        bone2.SetGoalTransform(p2, r2);
        bone3.SetGoalTransform(p3, r3);

#if UNITY_EDITOR
        if (m_debug)
        {
            Debug.DrawLine(p1, p2, Color.magenta);
            Debug.DrawLine(p2, p3, Color.magenta);
            Debug.DrawRay(p3, r3 * (0.1f * Vector3.left), Color.magenta);
        }
#endif
    }
}
