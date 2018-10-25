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

    void StartInteraction();
    void EndInteraction();
}
