using UnityEngine;

[ExecuteAlways]
public class RigLookAt : MonoBehaviour, IRigConstraint
{
    [SerializeField]
    private Transform m_target = null;

    /// <summary>
    /// The update order for the constraint. Lower values are evaluated first.
    /// </summary>
    public int UpdateOrder => 0;

    public void Initialize()
    {
    }

    public void UpdateConstraint()
    {
        SetRotation();
    }

#if UNITY_EDITOR
    public void LateUpdate()
    {
        if (!Application.isPlaying && m_target != null)
        {
            SetRotation();
        }
    }
#endif

    private void SetRotation()
    {
        Vector3 disp = m_target.position - transform.position;
        if (disp != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(disp, transform.up) * Quaternion.Euler(0, 90, 0);
        }
    }
}
    