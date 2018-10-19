using UnityEngine;

public class Automaton : MonoBehaviour
{
    [System.Serializable]
    public class Movement
    {
        [SerializeField]
        [Range(0, 40)]
        private float m_acceleration = 10.0f;
        public float Acceleration => m_acceleration;

        [SerializeField]
        [Range(0, 20)]
        private float speed = 10.0f;
        public float Speed => speed;
    }

    [SerializeField]
    private Movement m_ground = new Movement();

    [SerializeField]
    private Movement m_air = new Movement();

    [SerializeField]
    [Range(0, 20)]
    private float m_minSpeed = 0.5f;

    [SerializeField]
    [Range(0, 10)]
    private float m_jumpSpeed = 10.0f;
    
    [SerializeField]
    private LayerMask m_groundLayers;
    
    private readonly RaycastHit[] m_hits = new RaycastHit[20];

    private Rigidbody m_body;
    private CapsuleCollider m_collider;
    private Animator m_anim;
    private CharacterAnimation m_constraints;
    private AnimationSounds m_sounds;

    private bool m_facingRight = true;
    private bool m_isGrounded = true;
    private float m_moveH = 0f;

    private void Awake()
    {
        m_body = GetComponent<Rigidbody>();
        m_collider = GetComponent<CapsuleCollider>();
        m_anim = GetComponent<Animator>();
        m_constraints = GetComponent<CharacterAnimation>();
        m_sounds = GetComponent<AnimationSounds>();
    }

    public void FixedUpdate()
    {
        // sample walkable surfaces under capsule
        Vector3 footSphereLocal = m_collider.center + ((m_collider.height / 2f) - m_collider.radius) * Vector3.down;
        Vector3 footSphere = transform.TransformPoint(footSphereLocal);
        float radius = m_collider.radius * 0.9f;
        float distance = (m_collider.radius * 0.1f) + 0.01f;
        int hitCount = Physics.SphereCastNonAlloc(footSphere, radius, Vector3.down, m_hits, distance, m_groundLayers, QueryTriggerInteraction.Ignore);
        
        m_isGrounded = hitCount > 0;

       // do movement
        Movement movement = m_isGrounded ? m_ground : m_air;
        Vector2 velocity = m_body.velocity;

        // make sure we only move forwards in the direction we are facing, but still can slow down
        bool facingDesiredDir = transform.forward.x * m_moveH > -0.1f;
        bool velocityIsOpposite = Mathf.Abs(velocity.x) > 0.01f && Mathf.Sign(velocity.x) != Mathf.Sign(m_moveH);

        float targetVelocity = (facingDesiredDir || velocityIsOpposite) ? movement.Speed * m_moveH : 0;
        velocity.x = Mathf.MoveTowards(velocity.x, targetVelocity, Time.deltaTime * movement.Acceleration);
        
        m_body.velocity = velocity;
    }

    public void Update()
    {
        float h = Input.GetAxis("Horizontal");
        m_moveH = Mathf.Abs(h) < 0.01f ? 0 : Mathf.Sign(h) * Mathf.Lerp(m_minSpeed / m_ground.Speed, 1.0f, Mathf.Abs(h));

        Vector3 velocity = m_body.velocity;

        if (m_isGrounded)
        {
            bool jump = Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.Joystick1Button0);
            
            if (jump && Mathf.Abs(transform.forward.x) > Mathf.Cos(10.0f * Mathf.Deg2Rad))
            {
                m_body.AddForce(m_jumpSpeed * Vector3.up, ForceMode.VelocityChange);
            }

            if (m_facingRight && Mathf.Abs(velocity.x) < 0.1f && m_moveH < -0.1f)
            {
                m_facingRight = false;
                m_anim.SetBool("PivotRight", true);
            }
            else if (!m_facingRight && Mathf.Abs(velocity.x) < 0.1f && m_moveH > 0.1f)
            {
                m_facingRight = true;
                m_anim.SetBool("PivotLeft", true);
            }
        }
        
        transform.rotation = Quaternion.Euler(0, Mathf.MoveTowards(transform.rotation.eulerAngles.y, m_facingRight ? 90 : 270, 1.65f * Time.deltaTime * 180), 0);

        const float walkAnimationSpeed = 1.62f;
        const float runAnimationSpeed = 6.24f;
        
        float speedH = Mathf.Abs(velocity.x);
        float walkRun = Mathf.InverseLerp(m_minSpeed, runAnimationSpeed, speedH);
        float walkRunSpeed = Mathf.LerpUnclamped(speedH / walkAnimationSpeed, speedH / runAnimationSpeed, walkRun);

        SetFloatLerp("SpeedH", Vector3.Dot(velocity, transform.forward), m_ground.Speed * 4.0f);
        SetFloatLerp("SpeedV", velocity.y, m_ground.Speed * 4.0f);
        SetFloatLerp("WalkRun", walkRun, 4.0f);
        SetFloatSmooth("WalkRunSpeed", walkRunSpeed, 4.0f);
        SetFloatSmooth("AirAnimSpeed", 0.25f + (0.05f * velocity.magnitude), 8.0f);
        m_anim.SetBool("Grounded", m_isGrounded);
    }

    private void SetFloatLerp(string name, float target, float rate)
    {
        m_anim.SetFloat(name, Mathf.MoveTowards(m_anim.GetFloat(name), target, Time.deltaTime * rate));
    }

    private void SetFloatSmooth(string name, float target, float rate)
    {
        m_anim.SetFloat(name, Mathf.Lerp(m_anim.GetFloat(name), target, Time.deltaTime * rate));
    }

    public void LateUpdate()
    {
        m_anim.SetBool("PivotLeft", false);
        m_anim.SetBool("PivotRight", false);

        m_constraints.VisualUpdate();
    }
}
