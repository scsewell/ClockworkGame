using System;
using UnityEngine;

public class RigLookAtConstraint : MonoBehaviour, IConstraint
{
    [SerializeField]
    [Tooltip("The time in seconds over which the bones smoothly switch to a new target.")]
    [Range(0f, 2f)]
    private float m_blendDuration = 0.2f;

    [Serializable]
    public class LookAtBone : Bone
    {
        [Tooltip("The strength of the look at effect on this bone.")]
        [Range(0f, 1f)]
        public float weight = 1.0f;

        [Tooltip("The maximum angle in degrees the bone may look away from the animated rotation.")]
        [Range(0f, 179f)]
        public float clampAngle = 45.0f;
    }
    
    [SerializeField]
    private LookAtBone[] m_bones;
    
    private Transform m_target = null;
    private bool m_targetChanged = false;
    private float m_weight = 0f;
    private float m_oldTargetBlend = 0;

    public Transform Target
    {
        get { return m_target; }
        set
        {
            if (m_target != value)
            {
                m_target = value;
                m_targetChanged = true;
            }
        }
    }

    public float Weight { get; set; } = 1f;

    public int UpdateOrder => -2000;

    public void Initialize()
    {
        foreach (LookAtBone bone in m_bones)
        {
            bone.StoreBlendTransform();
        }
    }

    public void UpdateConstraint()
    {
        m_weight = Mathf.Lerp(m_weight, Weight, Time.deltaTime / 0.2f);

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

        foreach (LookAtBone bone in m_bones)
        {
            Transform t = bone.transform;

            Quaternion lookRotation = t.rotation;
            float dirWeight = 0;

            if (Target != null)
            {
                Vector3 lookDir = (Target.position - t.position).normalized;
                Vector3 lookCross = Vector3.Cross(t.forward, lookDir).normalized;
                float angle = Vector3.SignedAngle(t.forward, lookDir, lookCross);

                float clampedAngle = Mathf.Clamp(angle, -bone.clampAngle, bone.clampAngle);
                Vector3 clampedDir = Quaternion.AngleAxis(clampedAngle, lookCross.normalized) * t.forward;

                dirWeight = 1f - Mathf.Clamp01(Mathf.InverseLerp(bone.clampAngle, 180f, Mathf.Abs(angle)));
                lookRotation = bone.LookAt(clampedDir);
            }
            
            // apply the look at rotation offset
            t.rotation = Quaternion.Slerp(t.rotation, lookRotation, m_weight * bone.weight * dirWeight);
        }

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
