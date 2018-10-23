﻿using UnityEngine;

public class LegIK : TwoBoneIK
{
    [SerializeField]
    private LayerMask m_groundLayers = Physics.DefaultRaycastLayers;

    [SerializeField]
    [Tooltip("The height of the foot bone above the floor when resting on a flat surface.")]
    [Range(0f, 0.5f)]
    private float m_footHeight = 0.1f;

    [SerializeField]
    [Tooltip("The maximum angle in degrees on which the foot can rest.")]
    [Range(0f, 0.5f)]
    private float m_radius = 0.1f;

    [SerializeField]
    [Tooltip("The maximum angle in degrees on which the foot can rest.")]
    [Range(0f, 90f)]
    private float m_maxSlope = 45f;


    [SerializeField]
    [Tooltip("The speed of the foot for which the fastest blend duration is used.")]
    [Range(0f, 20f)]
    private float m_blendVelocity = 10f;

    [SerializeField]
    [Tooltip("The time in seconds over which the bones smoothly switch to the target at maximum foot speed.")]
    [Range(0f, 2f)]
    private float m_blendDuration = 0.2f;

    [SerializeField]
    private string m_weightCurveName = string.Empty;

    [Header("Bones")]
    
    [SerializeField]
    private Bone m_thigh;
    [SerializeField]
    private Bone m_shin;
    [SerializeField]
    private Bone m_foot;
    [SerializeField]
    private Bone m_toe;

    private Animator m_anim;
    private readonly RaycastHit[] m_hits = new RaycastHit[10];
    private Vector3 m_lastFootPos;
    private Vector3 m_spherePos;
    private bool m_hasFootHit = false;
    private float m_blend = 0;
    private float m_blendOut = 0;

    public override void Initialize()
    {
        m_anim = GetComponentInParent<Animator>();
        m_lastFootPos = m_foot.Position;
    }

    private void OnDrawGizmos()
    {
        if (m_debug)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(m_spherePos, m_radius);
        }
    }

    public override void UpdateConstraint()
    {
        // find all ground under the foot
        Vector3 searchDir = Vector3.down;
        float legLength = Vector2.Distance(m_thigh.Position, m_shin.Position) + Vector2.Distance(m_shin.Position, m_foot.Position);
        Vector3 searchPos = m_foot.Position - ((legLength + m_radius) * searchDir);
        float searchDistance = legLength + m_footHeight + m_freeDistance;

        int hitCount = Physics.SphereCastNonAlloc(searchPos, m_radius, searchDir, m_hits, 2f * legLength, m_groundLayers, QueryTriggerInteraction.Ignore);

        // find highest surface that is flat enough for the foot
        bool previouslyOnGround = m_hasFootHit;

        RaycastHit footHit = default;
        m_hasFootHit = false;
        float maxSlope = Mathf.Cos(m_maxSlope * Mathf.Deg2Rad);

        for (int i = 0; i < hitCount; i++)
        {
            RaycastHit hit = m_hits[i];
            float dot = Vector3.Dot(Vector3.up, hit.normal);
            if (maxSlope < dot && (!m_hasFootHit || footHit.point.y < hit.point.y))
            {
                footHit = hit;
                m_hasFootHit = true;
            }
        }
        
        if (m_hasFootHit)
        {
            m_spherePos = footHit.point + (m_radius * footHit.normal);

#if UNITY_EDITOR
            if (m_debug)
            {
                Debug.DrawLine(footHit.point, m_spherePos, Color.cyan);
            }
#endif
            
            // calculate the foot transform
            Vector3 targetPos = footHit.point + (m_footHeight * footHit.normal);
            Quaternion targetRot = m_foot.LookAt(footHit.normal);

            // only blend in the IK if the target foot position can actually be reached
            float targetDistance = Vector3.Distance(targetPos, m_thigh.Position);
            float maxDistance = GetMaxDistance(m_thigh, m_shin, m_foot);

            // get animator IK weight
            float animWeight = 0;
            if (!string.IsNullOrEmpty(m_weightCurveName))
            {
                animWeight = m_anim.GetFloat(m_weightCurveName);
            }

            // Even if IK is not requested by the animation, check if the foot is passing though the floor.
            // If so, force IK to prevent that.
            bool forceIK = Vector3.Dot(m_foot.Position - footHit.point, footHit.normal) < m_footHeight;
            float targetWeight = targetDistance < maxDistance ? (forceIK ? 1f : animWeight) : 0f;

            // blend proportionally to foot speed
            float speed = (m_lastFootPos - m_foot.Position).magnitude / Time.deltaTime;
            float duration = Mathf.Lerp(5 * m_blendDuration, m_blendDuration, speed / 10.0f);
            m_blend = Mathf.MoveTowards(m_blend, targetWeight, Time.deltaTime / duration);
            
            // Apply the IK contraint
            float weight = Weight * m_blend;
            DoIK(m_thigh, m_shin, m_foot, targetPos, targetRot, weight);

            // make sure the toes are not rotated since it is assumed the foot is flat against the surface
            m_toe.transform.localRotation = Quaternion.Slerp(m_toe.LocalRotation, m_toe.LookAtRotationOffset, weight);
        }
        else
        {
            m_spherePos = searchPos + (searchDistance * searchDir);
            m_blend = 0;
        }

#if UNITY_EDITOR
        if (m_debug)
        {
            Debug.DrawLine(searchPos, m_spherePos, Color.cyan);
        }
#endif

        // record the foot position
        m_lastFootPos = m_foot.Position;
    }
}
