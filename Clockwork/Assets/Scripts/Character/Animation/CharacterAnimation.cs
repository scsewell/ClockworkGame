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

    [Header("Tilting")]

    [SerializeField]
    [Tooltip("How many degrees to tilt per unit of acceleration.")]
    [Range(0f, 30f)]
    private float m_tiltScale = 3.0f;

    [SerializeField]
    [Tooltip("How maximum tilt in degrees.")]
    [Range(0f, 30f)]
    private float m_maxTilt = 10f;

    [SerializeField]
    [Tooltip("The amount of smoothing applied to the tilt effect.")]
    [Range(0.01f, 1f)]
    private float m_tiltSmoothing = 0.25f;

    [Header("Leaning")]

    [SerializeField]
    [Tooltip("The strength of the leaning effect.")]
    [Range(0.01f, 2f)]
    private float m_leanScale = 0.25f;

    [SerializeField]
    [Tooltip("The amount of smoothing applied to the lean effect.")]
    [Range(0.01f, 1f)]
    private float m_leanSmoothing = 0.25f;

    [Header("Constraints")]
    [SerializeField] private ArmIK m_leftArm;
    [SerializeField] private ArmIK m_rightArm;
    
    private Interactor m_interactor;
    private Movement m_movement;
    private Animator m_anim;
    private IConstraint[] m_contraints;
    private float m_tilt = 0;
    private Quaternion m_lean = Quaternion.identity;

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
        m_contraints = GetComponentsInChildren<IConstraint>(true);
        Array.Sort(m_contraints, (a, b) => a.UpdateOrder.CompareTo(b.UpdateOrder));

        for (int i = 0; i < m_contraints.Length; i++)
        {
            m_contraints[i].Initialize();
        }
        
        m_interactor.InteractionEnded += OnInteractionEnded;
    }

    private void OnDestroy()
    {
        m_interactor.InteractionEnded -= OnInteractionEnded;
    }
    
    private void OnInteractionEnded(IInteractable interactable)
    {
        m_leftArm.Target = null;
        m_rightArm.Target = null;
    }

    public void PreAnimationUpdate()
    {
        // Decide where to put hands. If nothing can be reached stop interaction
        if (m_interactor.IsInteracting && !AssignHandAnchors(m_interactor.CurrentInteration, m_leftArm, m_rightArm))
        {
            m_interactor.EndInteraction();
        }

        // update animator
        SetAnimatorProperties();

        // set rotation
        UpdateTilt();
        UpdateLean();
        transform.rotation = Quaternion.Euler(0, 0, m_tilt) * m_lean * transform.parent.rotation;
    }

    public void PostAnimationUpdate()
    {
        // update all bone constraints
        for (int i = 0; i < m_contraints.Length; i++)
        {
            m_contraints[i].UpdateConstraint();
        }
        
        m_interactor.IsGrabbing = m_leftArm.IsGrabbingTarget || m_rightArm.IsGrabbingTarget;
    }

    private void SetAnimatorProperties()
    {
        Movement.Medium medium = m_movement.GetCurrentMedium();
        Vector3 velocity = m_movement.Velocity;
        Vector3 forward = m_movement.transform.forward;
        float forwardVelocity = Vector3.Dot(velocity, forward);

        float speedH = Mathf.Abs(velocity.x);

        float walkRun = Mathf.InverseLerp(medium.MinSpeed, m_runAnimationSpeed, speedH);
        float walkRunSpeed = Mathf.LerpUnclamped(speedH / m_walkAnimationSpeed, speedH / m_runAnimationSpeed, walkRun);

        float speedSmoothing = medium.MaxSpeed * 4.0f;
        SetFloatLerp("SpeedH", Mathf.Abs(forwardVelocity), speedSmoothing);
        SetFloatLerp("VelocityH", forwardVelocity, speedSmoothing);
        SetFloatLerp("VelocityV", velocity.y, speedSmoothing);
        SetFloatLerp("WalkRun", walkRun, 4.0f);
        SetFloatSmooth("WalkRunSpeed", walkRunSpeed * Mathf.Sign(forwardVelocity), 4.0f);
        SetFloatSmooth("AirAnimSpeed", 0.25f + (0.05f * velocity.magnitude), 8.0f);

        float angVel = m_movement.AngularVelocity;
        float angle = Mathf.Abs(Mathf.Asin(forward.z) * Mathf.Rad2Deg);
        m_anim.SetBool("PivotLeft", angle > m_pivotAngle && angVel < 0);
        m_anim.SetBool("PivotRight", angle > m_pivotAngle && angVel > 0);
        m_anim.SetFloat("PivotAnimSpeed", Mathf.Abs(angVel) / 180);

        m_anim.SetBool("Grounded", m_movement.IsGrounded);
    }

    /// <summary>
    /// Angle the character in the direction it is accelerating
    /// </summary>
    private void UpdateTilt()
    {
        float tilt = Mathf.Clamp(-m_movement.Acceleration.x * m_tiltScale, -m_maxTilt, m_maxTilt);
        m_tilt = Mathf.Lerp(m_tilt, tilt, Time.deltaTime / m_tiltSmoothing);
    }

    /// <summary>
    /// Angle the character torwards elements being interacted with.
    /// </summary>
    private void UpdateLean()
    {
        Vector3 leftDir = m_leftArm.Target != null ? transform.position - m_leftArm.Target.Position : Vector3.zero;
        Vector3 rightDir = m_rightArm.Target != null ? transform.position - m_rightArm.Target.Position : Vector3.zero;
        Vector3 interactDir = Vector3.ProjectOnPlane(leftDir + rightDir, Vector3.up);

        Quaternion lean = Quaternion.identity;
        if (interactDir.magnitude > 0.01f)
        {
            Vector3 leanDir = interactDir.normalized + (Vector3.up * (interactDir.magnitude * m_leanScale));
            Vector3 cross = Vector3.Cross(leanDir, Vector3.up);
            float angle = Vector3.SignedAngle(interactDir.normalized, leanDir, cross);
            lean = Quaternion.AngleAxis(angle, cross);
        }

        m_lean = Quaternion.Slerp(m_lean, lean, Time.deltaTime / m_leanSmoothing);
    }

    private HandAnchor[] m_handTargetsTemp = new HandAnchor[0];

    /// <summary>
    /// Sets arms to use the closest free hand anchor on the specified interactable.
    /// </summary>
    /// <returns>True if any hand target was assigned.</returns>
    private bool AssignHandAnchors(IInteractable interactable, params ArmIK[] arms)
    {
        Array.Resize(ref m_handTargetsTemp, arms.Length);

        for (int i = 0; i < arms.Length; i++)
        {
            m_handTargetsTemp[i] = null;
        }

        bool assignedAnyTarget = false;
        foreach (HandAnchor anchor in interactable.HandAnchors)
        {
            int closestIndex = -1;
            float closestDistance = ArmLength;

            for (int i = 0; i < arms.Length; i++)
            {
                ArmIK arm = arms[i];
                Vector3 shoulder = arm.ShoulderPosition;
                float distance = Vector3.Distance(shoulder, anchor.Position);

                if (distance < closestDistance && (m_handTargetsTemp[i] == null || distance < Vector3.Distance(shoulder, m_handTargetsTemp[i].Position)))
                {
                    closestIndex = i;
                    closestDistance = distance;
                }
            }

            if (closestIndex >= 0)
            {
                m_handTargetsTemp[closestIndex] = anchor;
                assignedAnyTarget = true;
            }
        }

        for (int i = 0; i < arms.Length; i++)
        {
            arms[i].Target = m_handTargetsTemp[i];
        }

        return assignedAnyTarget;
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
