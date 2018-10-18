using UnityEngine;

public class Automaton : MonoBehaviour
{
    [SerializeField]
    [Range(0, 20)]
    private float m_jumpSpeed = 10.0f;

    [SerializeField]
    private LayerMask m_groundLayers;

    private Rigidbody m_body;
    private CapsuleCollider m_collider;
    private Animator m_anim;
    private CharacterAnimation m_constraints;
    private bool m_facingRight = true;
    private bool m_isGrounded = true;

    private RaycastHit[] m_hits = new RaycastHit[20];

    private void Awake()
    {
        m_body = GetComponent<Rigidbody>();
        m_collider = GetComponent<CapsuleCollider>();
        m_anim = GetComponent<Animator>();
        m_constraints = GetComponent<CharacterAnimation>();
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
    }

    public void Update()
    {
        float speedH = Input.GetAxis("Horizontal");
        m_anim.SetFloat("MoveH", Mathf.MoveTowards(m_anim.GetFloat("MoveH"), Mathf.Abs(speedH), Time.deltaTime * 3.0f));
        
        if (m_isGrounded)
        {
            if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Joystick1Button0))
            {
                m_body.AddForce(m_jumpSpeed * Vector3.up, ForceMode.VelocityChange);
            }

            if (m_facingRight && speedH < -0.1f)
            {
                m_facingRight = false;
                m_anim.SetBool("PivotRight", true);
            }
            else if (!m_facingRight && speedH > 0.1f)
            {
                m_facingRight = true;
                m_anim.SetBool("PivotLeft", true);
            }
        }
        
        transform.rotation = Quaternion.Euler(0, Mathf.MoveTowards(transform.rotation.eulerAngles.y, m_facingRight ? 90 : 270, 1.365f * Time.deltaTime * 180), 0);

        float velocityTime = Time.deltaTime * 4.0f;
        m_anim.SetFloat("SpeedH", Mathf.Lerp(m_anim.GetFloat("SpeedH"), m_body.velocity.z, velocityTime));
        m_anim.SetFloat("SpeedV", Mathf.Lerp(m_anim.GetFloat("SpeedV"), m_body.velocity.y, velocityTime));
        m_anim.SetFloat("AirAnimSpeed", Mathf.MoveTowards(m_anim.GetFloat("AirAnimSpeed"), 0.25f + (0.05f * m_body.velocity.magnitude), Time.deltaTime / 0.1f));

        m_anim.SetBool("Grounded", m_isGrounded);
    }

    public void LateUpdate()
    {
        m_anim.SetBool("PivotLeft", false);
        m_anim.SetBool("PivotRight", false);

        m_constraints.VisualUpdate();
    }
}
