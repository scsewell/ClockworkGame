using UnityEngine;

public class PositionAudioListener : MonoBehaviour
{   
    private void LateUpdate()
    {
        transform.position = Main.Instance.playerCenterOfMass.position;
    }
}
