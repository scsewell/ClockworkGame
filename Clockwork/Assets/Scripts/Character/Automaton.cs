using UnityEngine;

public class Automaton : MonoBehaviour
{
    private Interactor m_interactor;
    private Movement m_movement;
    private CharacterAnimation m_anim;
    private bool m_interact = false;

    private void Awake()
    {
        m_interactor = GetComponent<Interactor>();
        m_movement = GetComponentInChildren<Movement>();
        m_anim = GetComponentInChildren<CharacterAnimation>();

        m_interactor.InteractionEnded += OnInteractionEnded;
    }

    private void OnDestroy()
    {
        m_interactor.InteractionEnded -= OnInteractionEnded;
    }

    private void OnInteractionEnded(IInteractable interactable)
    {
        // When interaction has stopped, make sure that the interact command is pressed again
        // before starting a new interaction.
        m_interact = false;
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

        if (Input.GetKeyDown(KeyCode.RightControl) || Input.GetKeyDown(KeyCode.Joystick1Button2))
        {
            m_interact = true;
        }
        else if (Input.GetKeyUp(KeyCode.RightControl) || Input.GetKeyUp(KeyCode.Joystick1Button2))
        {
            m_interact = false;
        }

        // check for interaction
        if (m_interact)
        {
            if (!m_interactor.IsInteracting)
            {
                m_interactor.StartInteraction(m_anim.ArmLength, m_anim.ShoulderPositions);
            }
        }
        else if (m_interactor.IsInteracting)
        {
            m_interactor.EndInteraction();
        }

        if (m_interactor.IsInteracting && !m_interactor.CurrentInteration.AllowMovement)
        {
            moveH = 0;
            jump = false;
        }

        PlayerInput input = new PlayerInput(moveH, jump, m_interact);

        // update sub components
        m_movement.SetInputs(input);
        m_anim.PreAnimationUpdate();
    }

    public void LateUpdate()
    {
        m_anim.PostAnimationUpdate();
    }
}
