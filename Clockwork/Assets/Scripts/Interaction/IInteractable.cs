using System.Collections.Generic;

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
