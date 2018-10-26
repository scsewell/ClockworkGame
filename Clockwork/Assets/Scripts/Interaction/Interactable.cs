using System.Collections.Generic;
using UnityEngine;

public abstract class Interactable : MonoBehaviour, IInteractable
{
    [SerializeField]
    [Tooltip("The places where a character's hands are positioned during interactions.")]
    private List<HandAnchor> m_handAnchors = new List<HandAnchor>();
    public List<HandAnchor> HandAnchors => m_handAnchors;
    
    /// <summary>
    /// The current interaction source.
    /// </summary>
    public IInteractor CurrentInteractor { get; private set; } = null;

    /// <summary>
    /// Is this interactable is currently interacting.
    /// </summary>
    public bool IsInteracting => CurrentInteractor != null;

    /// <summary>
    /// Should interactors be allowed to move while interacting with this.
    /// </summary>
    public virtual bool AllowMovement { get; } = true;

    protected virtual void OnEnable()
    {
        InteractionManager.Instance.Register(this);
    }

    protected virtual void OnDisable()
    {
        EndInteraction();
        InteractionManager.Instance.Deregister(this);
    }

    /// <summary>
    /// Called when an interactor starts an interaction.
    /// </summary>
    /// <param name="source">The source that initiated the interaction.</param>
    public virtual void OnInteractStart(IInteractor source)
    {
        CurrentInteractor = source;
    }

    /// <summary>
    /// Called when an interactor stops an interaction.
    /// </summary>
    /// <param name="source">The source that stopped the interaction.</param>
    public virtual void OnInteractEnd(IInteractor source)
    {
        if (source == CurrentInteractor)
        {
            CurrentInteractor = null;
        }
        else
        {
            Debug.LogWarning($"Interaction source \"{source.Name}\" tried to stop interacting with \"{name}\", but they are not interacting.");
        }
    }

    /// <summary>
    /// Stops the current interaction.
    /// </summary>
    public void EndInteraction()
    {
        if (IsInteracting)
        {
            CurrentInteractor.EndInteraction();
        }
    }
}
