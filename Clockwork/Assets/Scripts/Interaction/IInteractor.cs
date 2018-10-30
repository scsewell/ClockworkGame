public interface IInteractor
{
    /// <summary>
    /// The interaction that this interactor is currently interacting with.
    /// </summary>
    IInteractable CurrentInteration { get; }

    /// <summary>
    /// Is this interactor is currently interacting.
    /// </summary>
    bool IsInteracting { get; }

    /// <summary>
    /// The name of the interactor.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Is the interactor grabbing onto the the current interactable.
    /// </summary>
    bool IsGrabbing { get; }

    /// <summary>
    /// Ends the current interaction if interacting.
    /// </summary>
    void EndInteraction();
}
