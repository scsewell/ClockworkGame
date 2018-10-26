using System;
using UnityEngine;

/// <summary>
/// Stores information about how a hand interactes with an interactable.
/// </summary>
[Serializable]
public class HandAnchor
{
    public enum RotateMode
    {
        Locked,
        AboutPalm,
        Free,
    }

    public enum HandShape
    {
        Closed,
        Flat,
    }

    [SerializeField]
    [Tooltip("The transform at which to position a hand")]
    private Transform m_transform;

    [SerializeField]
    private RotateMode m_rotateMode = RotateMode.Locked;

    [SerializeField]
    public HandShape m_handShape = HandShape.Closed;
    public HandShape Shape => m_handShape;

    [HideInInspector]
    public bool occupied = false;

    public Vector3 Position => m_transform.position;

    public Vector3 GetHandPosition()
    {
        return m_transform.position;
    }

    public Quaternion GetHandRotation()
    {
        return m_transform.rotation;
    }
}
