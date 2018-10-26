using UnityEngine;

public class Automaton : MonoBehaviour
{
    private Movement m_movement;
    private CharacterAnimation m_anim;

    private void Awake()
    {
        m_movement = GetComponentInChildren<Movement>();
        m_anim = GetComponentInChildren<CharacterAnimation>();
    }

    public void FixedUpdate()
    {
        m_movement.DoMovement();
    }

    public void Update()
    {
        // get input
        float moveH = Input.GetAxis("Horizontal");
        bool jump = Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.Joystick1Button0);
        bool interact = Input.GetKey(KeyCode.Joystick1Button1);

        // check for interaction
        Interactor interator = GetComponent<Interactor>();
        if (Input.GetKey(KeyCode.RightControl) || Input.GetKey(KeyCode.Joystick1Button2))
        {
            if (!interator.IsInteracting)
            {
                interator.StartInteraction(m_anim.ArmLength, m_anim.ShoulderPositions);
            }
        }
        else if (interator.IsInteracting)
        {
            interator.EndInteraction();
        }

        if (interator.IsInteracting && !interator.CurrentInteration.AllowMovement)
        {
            moveH = 0;
            jump = false;
        }

        PlayerInput input = new PlayerInput(moveH, jump, interact);

        // update sub components
        m_movement.SetInputs(input);
        m_anim.PreAnimationUpdate();
    }

    public void LateUpdate()
    {
        m_anim.PostAnimationUpdate();
    }
}
