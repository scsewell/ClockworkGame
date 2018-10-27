using System;
using UnityEngine;

/// <summary>
/// Represents a rig bone.
/// </summary>
[Serializable]
public class Bone
{
    public Transform transform;

    public enum LookAxis
    {
        XNeg,
        XPos,
        YNeg,
        YPos,
        ZNeg,
        ZPos,
    }

    [SerializeField]
    private LookAxis m_lookAtAxis;
    public LookAxis LookAtAxis => m_lookAtAxis;

    [SerializeField]
    private Vector3 m_lookAtRotationOffset;
    public Quaternion LookAtRotationOffset => Quaternion.Euler(m_lookAtRotationOffset);

    private Vector3 m_lastPosition;
    private Quaternion m_lastRotation;

    private Vector3 m_blendPosition;
    private Quaternion m_blendRotation;
    
    public Vector3 Position => transform.position;
    public Quaternion Rotation => transform.rotation;

    public Vector3 LocalPosition => transform.localPosition;
    public Quaternion LocalRotation => transform.localRotation;

    public void StoreLastTransform()
    {
        m_lastPosition = LocalPosition;
        m_lastRotation = LocalRotation;
    }

    public void StoreBlendTransform()
    {
        m_blendPosition = m_lastPosition;
        m_blendRotation = m_lastRotation;
    }
    
    public void SetTransform(Vector3 position, Quaternion rotation)
    {
        transform.SetPositionAndRotation(position, rotation);
    }
    
    public void ApplyBlendTransform(float weight)
    {
        transform.localPosition = Vector3.Lerp(LocalPosition, m_blendPosition, weight);
        transform.localRotation = Quaternion.Slerp(LocalRotation, m_blendRotation, weight);
    }

    public Quaternion LookAt(Vector3 dir)
    {
        Vector3 axis = Vector3.zero;
        switch (m_lookAtAxis)
        {
            case LookAxis.XNeg: axis = Vector3.left; break;
            case LookAxis.XPos: axis = Vector3.right; break;
            case LookAxis.YNeg: axis = Vector3.down; break;
            case LookAxis.YPos: axis = Vector3.up; break;
            case LookAxis.ZNeg: axis = Vector3.back; break;
            case LookAxis.ZPos: axis = Vector3.forward; break;
        }

        return Quaternion.LookRotation(dir, transform.rotation * axis) * LookAtRotationOffset;
    }

    public Quaternion LookAt(Vector3 dir, Vector3 up)
    {
        return Quaternion.LookRotation(dir, up) * LookAtRotationOffset;
    }
}
