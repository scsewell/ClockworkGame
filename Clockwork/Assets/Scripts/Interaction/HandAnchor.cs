using System;
using UnityEngine;

/// <summary>
/// Stores information about how a hand interactes with an interactable.
/// </summary>
[Serializable]
public class HandAnchor
{
    [Tooltip("The transform at which to position a hand")]
    public Transform transform;

    public enum RotateMode
    {
        Locked,
        AboutPalm,
        Free,
    }

    public RotateMode rotateMode = RotateMode.Locked;

    public enum HandShape
    {
        Closed,
        Flat,
    }

    public HandShape handShape = HandShape.Closed;
}
