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
    [Tooltip("The anglular velocity of the handle.")]
    [Range(0f, 720f)]
    private float m_traversalSpeed = 180f;
    
    private float m_value = 0;

    /// <summary>
    /// The [-1,1] value indicating the operation state of the lever.
    /// </summary>
    public float Value => m_value;

    private void Update()
    {
        
    }
}
