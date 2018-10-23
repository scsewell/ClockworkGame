using UnityEngine;

public class LegIK : TwoBoneIK
{
    [SerializeField]
    private Transform m_thigh;
    [SerializeField]
    private Transform m_shin;
    [SerializeField]
    private Transform m_foot;
    [SerializeField]
    private Transform m_toe;

    [SerializeField]
    private LayerMask m_groundLayers = Physics.DefaultRaycastLayers;

    [SerializeField]
    [Range(0f, 0.5f)]
    private float m_footHeight = 0.1f;
    [SerializeField]
    [Range(0f, 0.5f)]
    private float m_radius = 0.1f;
    [SerializeField]
    [Range(0f, 90f)]
    private float m_maxSlope = 45f;

    [SerializeField]
    private string m_weightCurveName = string.Empty;

    private Animator m_anim;
    private readonly RaycastHit[] m_hits = new RaycastHit[10];
    private RaycastHit m_footHit;
    private bool m_hasFootHit = false;

    private void Awake()
    {
        m_anim = GetComponentInParent<Animator>();
    }

    private void OnDrawGizmos()
    {
        if (m_debug && m_hasFootHit && Weight > 0)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(m_footHit.point + (m_radius * m_footHit.normal), m_radius);
        }
    }

    public override void UpdateConstraint()
    {
        m_hasFootHit = false;
        
        // find all ground under the foot
        Vector3 searchDir = Vector3.down;
        float searchHeight = Vector2.Distance(m_thigh.position, m_shin.position) + Vector2.Distance(m_shin.position, m_foot.position);
        Vector3 searchPos = m_foot.position - ((searchHeight + m_radius) * searchDir);
        float searchDistance = searchHeight + m_footHeight;

#if UNITY_EDITOR
        if (m_debug)
        {
            Debug.DrawRay(searchPos, searchDir * searchDistance, Color.cyan);
        }
#endif

        int hitCount = Physics.SphereCastNonAlloc(searchPos, m_radius, searchDir, m_hits, searchDistance, m_groundLayers, QueryTriggerInteraction.Ignore);

        // TODO --  Smooth ground normal over time

        // find flattest surface
        m_footHit = default;
        m_hasFootHit = false;
        float maxSlope = Mathf.Cos(m_maxSlope * Mathf.Deg2Rad);

        for (int i = 0; i < hitCount; i++)
        {
            RaycastHit hit = m_hits[i];
            float dot = Vector3.Dot(Vector3.up, hit.normal);
            if (maxSlope < dot && (!m_hasFootHit || m_footHit.point.y < hit.point.y))
            {
                m_footHit = hit;
                m_hasFootHit = true;
            }
        }

        // only use IK if near the ground
        if (m_hasFootHit)
        {
#if UNITY_EDITOR
            if (m_debug)
            {
                Debug.DrawRay(m_footHit.point, m_radius * m_footHit.normal, Color.cyan);
            }
#endif
            // get animator IK weight
            float animWeight = 0;
            if (!string.IsNullOrEmpty(m_weightCurveName))
            {
                animWeight = m_anim.GetFloat(m_weightCurveName);
            }

            // Even if IK is not requested by the animation, check if the foot is passing though the floor.
            // If so, force IK to prevent that.
            bool forceIK = Vector3.Dot(m_foot.position - m_footHit.point, m_footHit.normal) < m_footHeight;
            float targetWeight = forceIK ? 1f : animWeight;
            Weight = Mathf.MoveTowards(Weight, targetWeight, Time.deltaTime * 15.0f);
            
            // Apply the IK contraint
            Vector3 targetPos = m_footHit.point + (m_footHeight * m_footHit.normal);
            Quaternion targetRot = Quaternion.LookRotation(m_footHit.normal, m_foot.up) * Quaternion.Euler(0, -135f, 0);

            if (DoIK(m_thigh, m_shin, m_foot, targetPos, targetRot))
            {
                m_toe.localRotation = Quaternion.Slerp(m_toe.localRotation, Quaternion.Euler(0f, -225f, -180f), Weight);
            }
        }
    }
}
