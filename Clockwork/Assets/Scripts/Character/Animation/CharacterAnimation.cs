using UnityEngine;
using Framework;

public class CharacterAnimation : MonoBehaviour
{
    [SerializeField]
    [Range(0, 20)]
    private float m_walkAnimationSpeed = 1.62f;

    [SerializeField]
    [Range(0, 20)]
    private float m_runAnimationSpeed = 6.24f;
    
    [System.Serializable]
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

    [Header("LookAt")]

    [SerializeField]
    private LookAtBone[] m_headLook;
    
    private Animator m_anim;
    private IRigConstraint[] m_contraints;
    private float m_lookAtWeight = 0f;

    public Transform LookAtTarget { get; set; } = null;
    public float LookAtWeight { get; set; } = 0f;
    
    private void Awake()
    {
        m_anim = GetComponent<Animator>();
        m_contraints = GetComponentsInChildren<IRigConstraint>(true);
        
        foreach (LookAtBone bone in m_headLook)
        {
            bone.lastLookRotation = bone.transform.rotation;
        }
    }

    public void VisualUpdate(Automaton automaton)
    {
        Vector3 velocity = automaton.Velocity;
        float speedH = Mathf.Abs(velocity.x);

        float walkRun = Mathf.InverseLerp(automaton.Ground.MinSpeed, m_runAnimationSpeed, speedH);
        float walkRunSpeed = Mathf.LerpUnclamped(speedH / m_walkAnimationSpeed, speedH / m_runAnimationSpeed, walkRun);

        float speedSmoothing = automaton.Ground.MaxSpeed * 4.0f;
        SetFloatLerp("SpeedH", Vector3.Dot(velocity, automaton.transform.forward), speedSmoothing);
        SetFloatLerp("SpeedV", velocity.y, speedSmoothing);
        SetFloatLerp("WalkRun", walkRun, 4.0f);
        SetFloatSmooth("WalkRunSpeed", walkRunSpeed, 4.0f);
        SetFloatSmooth("AirAnimSpeed", 0.25f + (0.05f * velocity.magnitude), 8.0f);
        m_anim.SetBool("Grounded", automaton.IsGrounded);
    }

    public void LateVisualUpdate()
    {
        m_lookAtWeight = Mathf.Lerp(m_lookAtWeight, LookAtWeight, Time.deltaTime / 0.2f);
        LookAt(m_headLook, LookAtTarget, m_lookAtWeight);

        for (int i = 0; i < m_contraints.Length; i++)
        {
            m_contraints[i].UpdateConstraint();
        }
    }

    public void PivotLeft()
    {
        m_anim.SetBool("PivotLeft", true);
        this.DelayedCall(() => m_anim.SetBool("PivotLeft", false), 0.1f);
    }

    public void PivotRight()
    {
        m_anim.SetBool("PivotRight", true);
        this.DelayedCall(() => m_anim.SetBool("PivotRight", false), 0.1f);
    }

    private void LookAt(LookAtBone[] bones, Transform target, float weight)
    {
        foreach (LookAtBone bone in m_headLook)
        {
            Transform t = bone.transform;

            Quaternion lookRotation = t.rotation;
            float dirWeight = 0;

            if (target != null)
            {
                Vector3 lookDir = (target.position - t.position).normalized;
                Vector3 lookCross = Vector3.Cross(t.forward, lookDir).normalized;
                float angle = Vector3.SignedAngle(t.forward, lookDir, lookCross);

                float clampedAngle = Mathf.Clamp(angle, -bone.clampAngle, bone.clampAngle);
                Vector3 clampedDir = Quaternion.AngleAxis(clampedAngle, lookCross.normalized) * t.forward;

                dirWeight = 1f - Mathf.Clamp01(Mathf.InverseLerp(bone.clampAngle, 180f, Mathf.Abs(angle)));
                lookRotation = Quaternion.LookRotation(clampedDir, -t.right) * Quaternion.Euler(0, 0, -90);
            }

            bone.lastDirWeight = Mathf.MoveTowards(bone.lastDirWeight, dirWeight, Time.deltaTime / (2f * bone.smoothing));
            bone.lastLookRotation = Quaternion.Slerp(bone.lastLookRotation, lookRotation, Time.deltaTime / bone.smoothing);
            t.rotation = Quaternion.Slerp(t.rotation, bone.lastLookRotation, weight * bone.weight * bone.lastDirWeight);
        }
    }

    private void SetFloatLerp(string name, float target, float rate)
    {
        m_anim.SetFloat(name, Mathf.MoveTowards(m_anim.GetFloat(name), target, Time.deltaTime * rate));
    }

    private void SetFloatSmooth(string name, float target, float rate)
    {
        m_anim.SetFloat(name, Mathf.Lerp(m_anim.GetFloat(name), target, Time.deltaTime * rate));
    }
}
