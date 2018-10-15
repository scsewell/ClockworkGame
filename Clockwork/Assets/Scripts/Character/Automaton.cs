using UnityEngine;

public class Automaton : MonoBehaviour
{
    private CharacterAnimation m_constraints;
    private Animator m_anim;
    private bool m_facingRight = true;

    private void Awake()
    {
        m_anim = GetComponent<Animator>();
        m_constraints = GetComponent<CharacterAnimation>();
    }

    public void Update()
    {
        float speedH = Input.GetAxis("Horizontal");
        m_anim.SetFloat("SpeedH", Mathf.MoveTowards(m_anim.GetFloat("SpeedH"), Mathf.Abs(speedH), Time.deltaTime * 3.0f));

        m_anim.SetBool("Flip", false);
        if (m_facingRight && speedH < -0.1f)
        {
            m_facingRight = false;
            m_anim.SetBool("Flip", true);
        }
        else if (!m_facingRight && speedH > 0.1f)
        {
            m_facingRight = true;
            m_anim.SetBool("Flip", true);
        }
        
        transform.rotation = Quaternion.Euler(0, Mathf.MoveTowards(transform.rotation.eulerAngles.y, m_facingRight ? 90 : 270, 1.365f * Time.deltaTime * 180), 0);
    }

    public void LateUpdate()
    {
        m_constraints.VisualUpdate();
    }
}
