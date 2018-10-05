using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

public class Rotator : MonoBehaviour
{
    [SerializeField]
    private float m_rotationSpeed = 10.0f;

    private void Update()
    {
        transform.Rotate(m_rotationSpeed * Time.deltaTime * Vector3.forward, Space.Self);
    }
}
