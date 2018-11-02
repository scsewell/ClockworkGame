using UnityEngine;

public class Moveable : Interactable
{
    [Header("Move Settings")]

    [SerializeField]
    [Range(0f, 10000f)]
    private float m_springStrength = 100f;

    [SerializeField]
    [Range(0f, 10000f)]
    private float m_springDamping = 100f;

    [SerializeField]
    [Range(0f, 1000f)]
    private float m_massScale = 1.0f;

    [SerializeField]
    [Range(-1f, 1f)]
    private float m_distance = 0.1f;

    private Rigidbody m_body;
    private Collider m_collider;
    private ConfigurableJoint m_joint;

    public override InteractionMovementMode MovementMode => InteractionMovementMode.NoRotate;

    private void Awake()
    {
        m_body = GetComponent<Rigidbody>();
        m_collider = GetComponentInChildren<Collider>();
    }

    public override void OnInteractStart(IInteractor source)
    {
        base.OnInteractStart(source);

        Rigidbody interactor = source.GameObject.GetComponentInParent<Rigidbody>();

        m_joint = gameObject.AddComponent<ConfigurableJoint>();

        // determine the anchor points
        CapsuleCollider collider = interactor.GetComponentInParent<CapsuleCollider>();
        float distanceOffset = m_distance + m_collider.bounds.extents.x;
        if (collider != null)
        {
            distanceOffset += collider.radius;
        }

        Vector3 delta = interactor.transform.position - transform.position;
        Vector3 anchor = new Vector3(Mathf.Sign(delta.x) * distanceOffset / transform.localScale.x, 0, 0);

        // set the joing properties
        m_joint.xMotion = ConfigurableJointMotion.Limited;
        m_joint.yMotion = ConfigurableJointMotion.Free;
        m_joint.zMotion = ConfigurableJointMotion.Free;

        m_joint.anchor = anchor;
        m_joint.autoConfigureConnectedAnchor = false;
        m_joint.connectedAnchor = Vector3.up;
        m_joint.connectedBody = interactor;
        m_joint.enableCollision = true;
        m_joint.massScale = m_massScale;
        
        var spring = m_joint.linearLimitSpring;
        spring.spring = m_springStrength;
        spring.damper = m_springDamping;
        m_joint.linearLimitSpring = spring;

        var limit = m_joint.linearLimit;
        limit.limit = 0.01f * m_distance;
        m_joint.linearLimit = limit;
    }

    public override void OnInteractEnd(IInteractor source)
    {
        base.OnInteractEnd(source);

        Destroy(m_joint);
    }
}
