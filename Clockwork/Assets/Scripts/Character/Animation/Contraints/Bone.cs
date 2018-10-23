using System;
using UnityEngine;

[Serializable]
public class Bone
{
    public Transform transform;

    public enum LookAtAxis
    {
        XNeg,
        XPos,
        YNeg,
        YPos,
        ZNeg,
        ZPos,
    }

    [SerializeField]
    private LookAtAxis m_lookAtAxis;
    [SerializeField]
    private Vector3 m_lookAtRotationOffset;

    [HideInInspector]
    public Vector3 lastPosition;
    [HideInInspector]
    public Vector3 lastRotation;
    
    private Quaternion LookAt(Vector3 dir)
    {
        Vector3 axis = Vector3.zero;
        switch (m_lookAtAxis)
        {
            case LookAtAxis.XNeg: axis = Vector3.left; break;
            case LookAtAxis.XPos: axis = Vector3.right; break;
            case LookAtAxis.YNeg: axis = Vector3.down; break;
            case LookAtAxis.YPos: axis = Vector3.up; break;
            case LookAtAxis.ZNeg: axis = Vector3.back; break;
            case LookAtAxis.ZPos: axis = Vector3.forward; break;
        }

        return Quaternion.LookRotation(dir, transform.rotation * axis) * Quaternion.Euler(m_lookAtRotationOffset);
    }

    private Quaternion LookAt(Vector3 dir, Vector3 up)
    {
        return Quaternion.LookRotation(dir, up) * Quaternion.Euler(m_lookAtRotationOffset);
    }
}
