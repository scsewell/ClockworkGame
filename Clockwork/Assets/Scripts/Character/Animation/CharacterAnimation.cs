using System;
using UnityEngine;

public class CharacterAnimation : MonoBehaviour
{
    [SerializeField]
    [Tooltip("The disttance moved during one loop of the walk animation.")]
    [Range(0f, 20f)]
    private float m_walkAnimationSpeed = 1.62f;

    [SerializeField]
    [Tooltip("The disttance moved during one loop of the run animation.")]
    [Range(0f, 20f)]
    private float m_runAnimationSpeed = 6.24f;

    [SerializeField]
    [Tooltip("The angle in degrees from the movement plane at which the pivot animations will start to play.")]
    [Range(0f, 45f)]
    private float m_pivotAngle = 20f;

    [Header("Constraints")]
    [SerializeField] private ArmIK m_leftArm;
    [SerializeField] private ArmIK m_rightArm;

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

    [Header("LookAt")]

    [SerializeField]
    private LookAtBone[] m_headLook;

    private Interactor m_interactor;
    private Movement m_movement;
    private Animator m_anim;
    private IRigConstraint[] m_contraints;
    private float m_lookAtWeight = 0f;

    public Transform LookAtTarget { get; set; } = null;
    public float LookAtWeight { get; set; } = 0f;

    private readonly Vector3[] m_shoulderPositions = new Vector3[2];
    public Vector3[] ShoulderPositions
    {
        get
        {
            m_shoulderPositions[0] = m_leftArm.ShoulderPosition;
            m_shoulderPositions[1] = m_rightArm.ShoulderPosition;
            return m_shoulderPositions;
        }
    }

    public float ArmLength => Mathf.Max(m_leftArm.MaxReach, m_rightArm.MaxReach);

    private void Awake()
    {
        m_interactor = GetComponentInParent<Interactor>();
        m_movement = GetComponentInParent<Movement>();
        m_anim = GetComponent<Animator>();

        // Get constraints and sort by update order
        m_contraints = GetComponentsInChildren<IRigConstraint>(true);
        Array.Sort(m_contraints, (a, b) => a.UpdateOrder.CompareTo(b.UpdateOrder));

        for (int i = 0; i < m_contraints.Length; i++)
        {
            m_contraints[i].Initialize();
        }

        foreach (LookAtBone bone in m_headLook)
        {
            bone.lastLookRotation = bone.transform.rotation;
        }

        m_interactor.InteractionStarted += OnInteractionStarted;
        m_interactor.InteractionEnded += OnInteractionEnded;
    }

    private void OnDestroy()
    {
        m_interactor.InteractionStarted -= OnInteractionStarted;
        m_interactor.InteractionEnded -= OnInteractionEnded;
    }

    private void OnInteractionStarted(IInteractable interactable)
    {
        m_leftArm.Target = m_interactor.GetFreeHandAnchorByDistance(m_leftArm.ShoulderPosition);
        m_rightArm.Target = m_interactor.GetFreeHandAnchorByDistance(m_rightArm.ShoulderPosition);
    }

    private void OnInteractionEnded(IInteractable interactable)
    {
        m_leftArm.Target = null;
        m_rightArm.Target = null;
    }
    
    public void PreAnimationUpdate()
    {
        // set animator properties
        Vector3 velocity = m_movement.Velocity;
        float speedH = Mathf.Abs(velocity.x);

        float walkRun = Mathf.InverseLerp(m_movement.Ground.MinSpeed, m_runAnimationSpeed, speedH);
        float walkRunSpeed = Mathf.LerpUnclamped(speedH / m_walkAnimationSpeed, speedH / m_runAnimationSpeed, walkRun);

        float speedSmoothing = m_movement.Ground.MaxSpeed * 4.0f;
        SetFloatLerp("SpeedH", Vector3.Dot(velocity, m_movement.transform.forward), speedSmoothing);
        SetFloatLerp("SpeedV", velocity.y, speedSmoothing);
        SetFloatLerp("WalkRun", walkRun, 4.0f);
        SetFloatSmooth("WalkRunSpeed", walkRunSpeed, 4.0f);
        SetFloatSmooth("AirAnimSpeed", 0.25f + (0.05f * velocity.magnitude), 8.0f);

        float angVel = m_movement.AngularVelocity;
        float angle = Mathf.Abs(Mathf.Asin(m_movement.transform.forward.z) * Mathf.Rad2Deg);
        m_anim.SetBool("PivotLeft", angle > m_pivotAngle && angVel < 0);
        m_anim.SetBool("PivotRight", angle > m_pivotAngle && angVel > 0);
        m_anim.SetFloat("PivotAnimSpeed", Mathf.Abs(angVel) / 180);

        m_anim.SetBool("Grounded", m_movement.IsGrounded);
    }

    public void PostAnimationUpdate()
    {
        // Update constraints
        m_lookAtWeight = Mathf.Lerp(m_lookAtWeight, LookAtWeight, Time.deltaTime / 0.2f);
        LookAt(m_headLook, LookAtTarget, m_lookAtWeight);
        
        for (int i = 0; i < m_contraints.Length; i++)
        {
            m_contraints[i].UpdateConstraint();
        }
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
