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
        [Range(0f, 180f)]
        public float clampAngle = 45.0f;

        [Tooltip("The angle in degrees between the look at and animaiton rotation at which the look at effect is fully blended out.")]
        [Range(0f, 180f)]
        public float blendAngle = 55.0f;

        [HideInInspector] public float currentWeight = 0f;
        [HideInInspector] public Quaternion lastLookOffset = Quaternion.identity;
    }

    [SerializeField]
    private LookAtBone[] m_bones;
    
    private Transform m_target = null;
    private bool m_targetChanged = false;
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
            Quaternion lookRotation = bone.Rotation;
            float dirWeight = 0;

            if (Target != null)
            {
                // get the rotation that looks as the target in world space
                Vector3 lookDir = (Target.position - bone.Position).normalized;
                Quaternion targetRotation = bone.LookAt(lookDir);

                // clamp the rotation to not go too far from the current bone rotation
                float angle = Quaternion.Angle(bone.Rotation, targetRotation);
                dirWeight = 1f - Mathf.Clamp01(Mathf.InverseLerp(bone.clampAngle, bone.blendAngle, Mathf.Abs(angle)));
                lookRotation = Quaternion.RotateTowards(bone.Rotation, targetRotation, bone.clampAngle);

                // transform the look rotation to at offset relative to the current rotation and smoothly move the look rotation offset to this new rotation
                Quaternion lookOffset = Quaternion.Inverse(bone.Rotation) * lookRotation;
                bone.lastLookOffset = Quaternion.Slerp(bone.lastLookOffset, lookOffset, Time.deltaTime * 12 * (dirWeight + 0.2f));
                lookRotation = bone.Rotation * bone.lastLookOffset;
            }

            // apply the look at rotation offset
            bone.currentWeight = Mathf.MoveTowards(bone.currentWeight, Weight * bone.weight * dirWeight, Time.deltaTime / m_blendDuration);
            bone.transform.rotation = Quaternion.Slerp(bone.Rotation, lookRotation, bone.currentWeight);
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
