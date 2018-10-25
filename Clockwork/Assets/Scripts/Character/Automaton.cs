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

        PlayerInput input = new PlayerInput(moveH, jump, interact);

        // check for interaction
        //Interactor interator = GetComponent<Interactor>();
        //if (Input.GetKey(KeyCode.Joystick1Button1))
        //{
        //    if (!interator.IsInteracting)
        //    {
        //        interator.StartInteraction();
        //    }
        //}
        //else if (interator.IsInteracting)
        //{
        //    interator.EndInteraction();
        //}

        // update sub components
        m_movement.SetInputs(input);
        m_anim.PreAnimationUpdate();
    }

    public void LateUpdate()
    {
        m_anim.PostAnimationUpdate();
    }
}
