using UnityEngine;

public class Lever : Interactable
{
    [Header("Lever Settings")]

    [SerializeField]
    [Tooltip("The transform to rotate as the lever is operated.")]
    private Transform m_handle = null;

    [SerializeField]
    [Tooltip("The axis the lever  handle rotates around.")]
    private Vector3 m_traversalAxis = Vector3.forward;

    [SerializeField]
    [Tooltip("The angle in degrees the lever can move between extremes.")]
    [Range(0f, 180f)]
    private float m_traversalArc = 30f;

    [SerializeField]
    [Tooltip("The time taken to switch which side the level is on.")]
    [Range(0f, 2f)]
    private float m_traversalTime = 0.5f;

    [SerializeField]
    [Tooltip("Should this lever use vertical input instead of horizontal input.")]
    private bool m_isVertical = false;
    
    private float m_value = 0;
    
    /// <summary>
    /// The [-1,1] value indicating the operation state of the lever.
    /// </summary>
    public float Value => m_value;

    public override bool AllowMovement => false;

    private void Update()
    {
        float input = IsGrabbed ? Input.GetAxis(m_isVertical ? "Vertical" : "Horizontal") : 0f;

        m_value = Mathf.MoveTowards(m_value, input, 2f * Time.deltaTime / m_traversalTime);

        float halfArc = 0.5f * m_traversalArc;
        float rotation = Mathf.SmoothStep(-halfArc, halfArc, 0.5f * (m_value + 1f));
        m_handle.localRotation = Quaternion.AngleAxis(rotation, m_traversalAxis);
    }
}
