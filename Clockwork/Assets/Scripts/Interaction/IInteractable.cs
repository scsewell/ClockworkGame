using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The interface for things in the scene the character can interact with.
/// </summary>
public interface IInteractable
{
    /// <summary>
    /// The parts of the interactable a character can place their hands.
    /// </summary>
    List<HandAnchor> HandAnchors { get; }

    /// <summary>
    /// How should interactors be allowed to move while interacting with this.
    /// </summary>
    InteractionMovementMode MovementMode { get; }
    
    /// <summary>
    /// The position of the interactable.
    /// </summary>
    Vector3 Position { get; }

    /// <summary>
    /// Called when an interactor starts an interaction.
    /// </summary>
    /// <param name="source">The source that initiated the interaction.</param>
    void OnInteractStart(IInteractor source);

    /// <summary>
    /// Called when an interactor stops an interaction.
    /// </summary>
    /// <param name="source">The source that stopped the interaction.</param>
    void OnInteractEnd(IInteractor source);
}
