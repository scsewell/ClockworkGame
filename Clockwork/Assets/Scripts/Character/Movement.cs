using System;
using UnityEngine;

public class Movement : MonoBehaviour
{
    [Header("Grounding")]

    [SerializeField]
    private LayerMask m_groundLayers;

    [SerializeField]
    [Range(0f, 5f)]
    private float m_groundingAssistDistance = 0.15f;

    [SerializeField]
    [Range(0f, 5f)]
    private float m_groundingVelocity = 0.25f;


    [Header("Pivoting")]

    [SerializeField]
    [Range(0f, 5f)]
    private float m_pivotSpeed = 1.65f;


    [Header("Jumping")]

    [SerializeField]
    [Range(0f, 1f)]
    private float m_jumpBufferDuration = 0.15f;

    [SerializeField]
    [Range(0f, 1f)]
    private float m_jumpWait = 0.1f;

    [SerializeField]
    [Range(0f, 10f)]
    private float m_jumpSpeed = 10.0f;


    [Header("Mediums")]

    [SerializeField]
    private Medium m_ground = new Medium();
    public Medium Ground => m_ground;

    [SerializeField]
    private Medium m_air = new Medium();
    public Medium Air => m_air;

    [Serializable]
    public class Medium
    {
        [SerializeField]
        [Range(0, 40)]
        private float m_acceleration = 10.0f;
        public float Acceleration => m_acceleration;

        [SerializeField]
        [Range(0, 20)]
        private float minSpeed = 0.5f;
        public float MinSpeed => minSpeed;

        [SerializeField]
        [Range(0, 20)]
        private float maxSpeed = 5.0f;
        public float MaxSpeed => maxSpeed;
    }

    
    private readonly RaycastHit[] m_hits = new RaycastHit[20];
    private CapsuleCollider m_collider;
    private Rigidbody m_body;

    private bool m_jump = false;
    private float m_moveH = 0;
    private bool m_facingRight = true;
    private float m_lastJumpTime = float.NegativeInfinity;
    private float m_lastJumpLandTime = float.NegativeInfinity;
    private bool m_jumping = false;

    public bool IsGrounded { get; private set; } = true;
    public Vector3 Velocity => m_body.velocity;
    public float AngularVelocity { get; private set; } = 0f;
    
    private void Awake()
    {
        m_collider = GetComponent<CapsuleCollider>();
        m_body = GetComponent<Rigidbody>();
    }

    public void DoMovement()
    {
        // update grounding
        bool previouslyGrounded = IsGrounded;

        RaycastHit groundHit;
        IsGrounded = CheckGrounded(out groundHit, 0.01f);

        if (!previouslyGrounded && IsGrounded && m_jumping)
        {
            m_jumping = false;
            m_lastJumpLandTime = Time.time;
        }

        // do movement
        Medium medium = IsGrounded ? m_ground : m_air;
        Vector2 velocity = m_body.velocity;

        const float deadzone = 0.01f;
        float moveH = Mathf.Abs(m_moveH) < deadzone ? 0 : Mathf.Sign(m_moveH) * Mathf.Lerp(medium.MinSpeed / medium.MaxSpeed, 1.0f, Mathf.Abs(m_moveH));

        // make sure we only move forwards in the direction we are facing, but still can slow down
        bool facingDesiredDir = transform.forward.x * moveH > 0f;
        bool velocityIsOpposite = Mathf.Abs(velocity.x) > 0.01f && Mathf.Sign(velocity.x) != Mathf.Sign(moveH);
        
        float targetVelocity = (facingDesiredDir || velocityIsOpposite) ? Mathf.Abs(transform.forward.x) * medium.MaxSpeed * moveH : 0;
        velocity.x = Mathf.MoveTowards(velocity.x, targetVelocity, Time.deltaTime * medium.Acceleration);

        if (IsGrounded)
        {
            // rotate the character along the direction of travel
            if (m_facingRight && Mathf.Abs(velocity.x) < 0.1f && moveH < -deadzone)
            {
                m_facingRight = false;
            }
            else if (!m_facingRight && Mathf.Abs(velocity.x) < 0.1f && moveH > deadzone)
            {
                m_facingRight = true;
            }

            if (m_jump)
            {
                velocity.y = m_jumpSpeed;
                m_jump = false;
                m_jumping = true;
            }
        }
        else if (previouslyGrounded && !m_jumping)
        {
            IsGrounded = CheckGrounded(out groundHit, m_groundingAssistDistance);
            if (Mathf.Abs(Vector3.Angle(groundHit.normal, Vector3.up)) < 85f)
            {
                velocity = Vector3.ProjectOnPlane(velocity, groundHit.normal);
                velocity.y -= m_groundingVelocity;
            }
        }

        // calculate the next rotation
        float lastRotation = transform.rotation.eulerAngles.y;
        float targetRotation = m_facingRight ? 90 : 270;
        float rotation = Mathf.MoveTowards(transform.rotation.eulerAngles.y, targetRotation, m_pivotSpeed * 180 * Time.deltaTime);
        AngularVelocity = (rotation - lastRotation) / Time.deltaTime;

        // update motion
        m_body.velocity = velocity;
        transform.rotation = Quaternion.Euler(0, rotation, 0);
    }

    public void SetInputs(PlayerInput input)
    {
        m_moveH = input.moveH;

        // buffer jump inputs
        bool jumpReady = Time.time - m_lastJumpLandTime > m_jumpWait;
        bool isFacingSide = Mathf.Abs(transform.forward.x) > Mathf.Cos(10.0f * Mathf.Deg2Rad);

        if (IsGrounded && isFacingSide && jumpReady && (input.jump || Time.time - m_lastJumpTime < m_jumpBufferDuration))
        {
            m_jump = true;
            m_lastJumpTime = float.NegativeInfinity;
            m_lastJumpLandTime = float.NegativeInfinity;
        }
        else if (input.jump)
        {
            m_lastJumpTime = Time.time;
        }
    }
    
    private bool CheckGrounded(out RaycastHit groundHit, float distance)
    {
        bool grounded = false;
        groundHit = default;

        // sample walkable surfaces under capsule
        Vector3 footSphereLocal = m_collider.center + ((m_collider.height / 2f) - m_collider.radius) * Vector3.down;
        Vector3 footSphere = transform.TransformPoint(footSphereLocal);
        float radius = m_collider.radius * 0.99f;
        float dist = (m_collider.radius * 0.01f) + distance;
        int hitCount = Physics.SphereCastNonAlloc(footSphere, radius, Vector3.down, m_hits, dist, m_groundLayers, QueryTriggerInteraction.Ignore);

        // find flattest surface
        float bestGroundNormal = 0;
        for (int i = 0; i < hitCount; i++)
        {
            RaycastHit hit = m_hits[i];
            float dot = Vector3.Dot(Vector3.up, hit.normal);
            if (bestGroundNormal < dot)
            {
                bestGroundNormal = dot;
                groundHit = hit;
                grounded = true;
            }
        }

        return grounded;
    }
}
