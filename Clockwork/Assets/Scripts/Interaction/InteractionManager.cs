using System.Collections.Generic;
using Framework;

/// <summary>
/// Keeps track of all interactable elements that are loaded.
/// </summary>
public class InteractionManager : Singleton<InteractionManager>
{
    private readonly List<IInteractable> m_interactables = new List<IInteractable>();

    /// <summary>
    /// The interactables that are available for interaction.
    /// </summary>
    public IReadOnlyList<IInteractable> ActiveInteractables => m_interactables;

    public void Register(IInteractable interactable)
    {
        if (!m_interactables.Contains(interactable))
        {
            m_interactables.Add(interactable);
        }
    }

    public void Deregister(IInteractable interactable)
    {
        m_interactables.Remove(interactable);
    }
}
