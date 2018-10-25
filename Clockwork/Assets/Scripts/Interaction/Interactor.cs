using UnityEngine;

/// <summary>
/// Handles character interactions with scene objects.
/// </summary>
public class Interactor : MonoBehaviour, IInteractor
{
    private IInteractable m_currentInteration = null;

    /// <summary>
    /// The interaction that this interactor is currently interacting with.
    /// </summary>
    public IInteractable CurrentInteration
    {
        get { return m_currentInteration; }
        private set
        {
            if (m_currentInteration != value)
            {
                if (m_currentInteration != null)
                {
                    m_currentInteration.OnInteractEnd(this);
                }

                m_currentInteration = value;

                if (m_currentInteration != null)
                {
                    m_currentInteration.OnInteractStart(this);
                }
            }
        }
    }

    /// <summary>
    /// Is this interactor is currently interacting.
    /// </summary>
    public bool IsInteracting => CurrentInteration != null;

    /// <summary>
    /// The name of the interactor.
    /// </summary>
    public string Name => name;

    public void StartInteraction()
    {
        CurrentInteration = GetClosestInteractable();
    }

    public void EndInteraction()
    {
        CurrentInteration = null;
    }
    
    /// <summary>
    /// Gets the interactable nearest to any of the provided points.
    /// </summary>
    private IInteractable GetClosestInteractable(params Vector3[] points)
    {
        IInteractable closest = null;
        float minDistance = float.PositiveInfinity;

        foreach (IInteractable interactable in InteractionManager.Instance.ActiveInteractables)
        {
            foreach (HandAnchor anchor in interactable.HandAnchors)
            {
                for (int i = 0; i < points.Length; i++)
                {
                    float distance = Vector3.Distance(points[i], anchor.transform.position);
                    if (minDistance > distance)
                    {
                        minDistance = distance;
                        closest = interactable;
                    }
                }
            }
        }

        return closest;
    }

    private HandAnchor GetHandAnchorByDistance(IInteractable interactable, Vector3 point)
    {
        HandAnchor closest = null;
        float minDistance = float.PositiveInfinity;

        foreach (HandAnchor anchor in interactable.HandAnchors)
        {
            float distance = Vector3.Distance(point, anchor.transform.position);
            if (minDistance > distance)
            {
                minDistance = distance;
                closest = anchor;
            }
        }

        return closest;
    }
}
