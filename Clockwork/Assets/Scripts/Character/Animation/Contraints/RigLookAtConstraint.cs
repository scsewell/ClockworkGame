using System;
using UnityEngine;

public class RigLookAtConstraint : MonoBehaviour, IConstraint
{
    [Serializable]
    private class LookAtBone
    {
        public Transform transform = null;
        [Range(0f, 1f)]
        public float weight = 1.0f;
        [Range(0f, 179f)]
        public float clampAngle = 45.0f;
        [Range(0.01f, 1f)]
        public float smoothing = 0.125f;

        [HideInInspector]
        public float lastDirWeight = 0f;
        [HideInInspector]
        public Quaternion lastLookRotation = Quaternion.identity;
    }
    
    [SerializeField]
    private LookAtBone[] m_bones;
    
    private float m_weight = 0f;

    public Transform Target { get; set; } = null;
    public float Weight { get; set; } = 1f;

    public int UpdateOrder => -2000;

    public void Initialize()
    {
        foreach (LookAtBone bone in m_bones)
        {
            bone.lastLookRotation = bone.transform.rotation;
        }
    }

    public void UpdateConstraint()
    {
        m_weight = Mathf.Lerp(m_weight, Weight, Time.deltaTime / 0.2f);

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
                lookRotation = Quaternion.LookRotation(clampedDir, -t.right) * Quaternion.Euler(0, 0, -90);
            }

            bone.lastDirWeight = Mathf.MoveTowards(bone.lastDirWeight, dirWeight, Time.deltaTime / (2f * bone.smoothing));
            bone.lastLookRotation = Quaternion.Slerp(bone.lastLookRotation, lookRotation, Time.deltaTime / bone.smoothing);
            t.rotation = Quaternion.Slerp(t.rotation, bone.lastLookRotation, m_weight * bone.weight * bone.lastDirWeight);
        }
    }
}
