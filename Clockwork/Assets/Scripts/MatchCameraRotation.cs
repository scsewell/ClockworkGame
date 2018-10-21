using UnityEngine;

public class MatchCameraRotation : MonoBehaviour
{   
    private void LateUpdate()
    {
        transform.rotation = Camera.main.transform.rotation;
    }
}
