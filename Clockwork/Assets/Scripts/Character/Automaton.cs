using UnityEngine;

public class Automaton : MonoBehaviour
{
    [SerializeField]
    private LayerMask m_groundLayers;

    [System.Serializable]
    public class Movement
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

    [Space]

    [SerializeField]
    private Movement m_ground = new Movement();
    public Movement Ground => m_ground;

    [SerializeField]
    private Movement m_air = new Movement();
    public Movement Air => m_air;

    [Space]

    [SerializeField]
    [Range(0, 1)]
    private float m_jumpBufferDuration = 0.15f;

    [SerializeField]
    [Range(0, 1)]
    private float m_jumpWait = 0.1f;

    [SerializeField]
    [Range(0, 10)]
    private float m_jumpSpeed = 10.0f;

    private readonly RaycastHit[] m_hits = new RaycastHit[20];
    private Rigidbody m_body;
    private CapsuleCollider m_collider;
    private CharacterAnimation m_anim;

    private bool m_facingRight = true;
    private float m_moveH = 0f;
    private float m_lastJumpTime = float.NegativeInfinity;
    private float m_lastJumpLandTime = float.NegativeInfinity;
    private bool m_jump = false;
    private bool m_jumping = false;

    public bool IsGrounded { get; private set; } = true;
    public Vector3 Velocity => m_body.velocity;

    private void Awake()
    {
        m_body = GetComponent<Rigidbody>();
        m_collider = GetComponent<CapsuleCollider>();
        m_anim = GetComponent<CharacterAnimation>();
    }

    public void FixedUpdate()
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
        Movement movement = IsGrounded ? m_ground : m_air;
        Vector2 velocity = m_body.velocity;

        float moveH = Mathf.Abs(m_moveH) < 0.01f ? 0 : Mathf.Sign(m_moveH) * Mathf.Lerp(movement.MinSpeed / movement.MaxSpeed, 1.0f, Mathf.Abs(m_moveH));

        // make sure we only move forwards in the direction we are facing, but still can slow down
        bool facingDesiredDir = transform.forward.x * moveH > -0.1f;
        bool velocityIsOpposite = Mathf.Abs(velocity.x) > 0.01f && Mathf.Sign(velocity.x) != Mathf.Sign(moveH);

        float targetVelocity = (facingDesiredDir || velocityIsOpposite) ? movement.MaxSpeed * moveH : 0;
        velocity.x = Mathf.MoveTowards(velocity.x, targetVelocity, Time.deltaTime * movement.Acceleration);

        // rotate the character along the direction of travel
        if (IsGrounded)
        {
            if (m_facingRight && Mathf.Abs(velocity.x) < 0.1f && moveH < -0.1f)
            {
                m_facingRight = false;
                m_anim.PivotRight();
            }
            else if (!m_facingRight && Mathf.Abs(velocity.x) < 0.1f && moveH > 0.1f)
            {
                m_facingRight = true;
                m_anim.PivotLeft();
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
            IsGrounded = CheckGrounded(out groundHit, 0.15f);
            if (Mathf.Abs(Vector3.Angle(groundHit.normal, Vector3.up)) < 85f)
            {
                velocity = Vector3.ProjectOnPlane(velocity, groundHit.normal);
                velocity.y -= 0.25f;
            }
        }

        // update motion
        m_body.velocity = velocity;
        transform.rotation = Quaternion.Euler(0, Mathf.MoveTowards(transform.rotation.eulerAngles.y, m_facingRight ? 90 : 270, 1.65f * Time.deltaTime * 180), 0);
    }

    public void Update()
    {
        // buffer walk input
        m_moveH = Input.GetAxis("Horizontal");

        // buffer jump inputs
        bool jump = Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.Joystick1Button0);
        bool jumpReady = Time.time - m_lastJumpLandTime > m_jumpWait;
        bool isFacingSide = Mathf.Abs(transform.forward.x) > Mathf.Cos(10.0f * Mathf.Deg2Rad);

        if (IsGrounded && isFacingSide && jumpReady && (jump || Time.time - m_lastJumpTime < m_jumpBufferDuration))
        {
            m_jump = true;
            m_lastJumpTime = float.NegativeInfinity;
            m_lastJumpLandTime = float.NegativeInfinity;
        }
        else if (jump)
        {
            m_lastJumpTime = Time.time;
        }
        
        // update sub components
        m_anim.VisualUpdate(this);
    }

    public void LateUpdate()
    {
        m_anim.LateVisualUpdate(this);
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
