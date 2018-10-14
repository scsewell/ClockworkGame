using UnityEngine;

[ExecuteInEditMode]
public class RigLookAt : MonoBehaviour, IRigConstraint
{
    [SerializeField]
    private Transform m_target = null;

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
            transform.rotation = Quaternion.LookRotation(disp, transform.parent.forward);
            transform.Rotate(new Vector3(-90, 90, 0), Space.Self);
        }
    }
}
    