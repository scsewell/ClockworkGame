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
                    InteractionEnded?.Invoke(m_currentInteration);
                }

                m_currentInteration = value;

                if (m_currentInteration != null)
                {
                    m_currentInteration.OnInteractStart(this);
                    InteractionStarted?.Invoke(m_currentInteration);
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

    /// <summary>
    /// Invoked when an interaction has started.
    /// </summary>
    public event InteractionStartedHandler InteractionStarted;
    public delegate void InteractionStartedHandler(IInteractable interactable);

    /// <summary>
    /// invoked when an iteraction has ended.
    /// </summary>
    public event InteractionEndedHandler InteractionEnded;
    public delegate void InteractionEndedHandler(IInteractable interactable);


    /// <summary>
    /// Starts an interaction with the closest interactable to a set of points.
    /// </summary>
    /// <param name="distance">The maximum distance for interactions.</param>
    /// <param name="points">The points to test the distance from.</param>
    public void StartInteraction(float distance, params Vector3[] points)
    {
        CurrentInteration = GetClosestInteractable(distance, points);
    }

    /// <summary>
    /// Ends the current interaction if interacting.
    /// </summary>
    public void EndInteraction()
    {
        CurrentInteration = null;
    }

    /// <summary>
    /// Gets the closest hand anchor on the current interactable to a point.
    /// </summary>
    /// <param name="point">The point to measure distance from.</param>
    /// <returns>The closest hand anchor if any is found.</returns>
    public HandAnchor GetFreeHandAnchorByDistance(Vector3 point)
    {
        HandAnchor closest = null;
        float minDistance = float.PositiveInfinity;

        foreach (HandAnchor anchor in CurrentInteration.HandAnchors)
        {
            if (!anchor.occupied)
            {
                float distance = Vector3.Distance(point, anchor.Position);
                if (minDistance > distance)
                {
                    minDistance = distance;
                    closest = anchor;
                }
            }
        }

        return closest;
    }
    
    private IInteractable GetClosestInteractable(float distance, params Vector3[] points)
    {
        IInteractable closest = null;
        float minDistance = distance;
        
        foreach (IInteractable interactable in InteractionManager.Instance.ActiveInteractables)
        {
            foreach (HandAnchor anchor in interactable.HandAnchors)
            {
                for (int i = 0; i < points.Length; i++)
                {
                    float d = Vector3.Distance(points[i], anchor.Position);
                    if (minDistance >= d)
                    {
                        minDistance = d;
                        closest = interactable;
                    }
                }
            }
        }

        return closest;
    }
}
