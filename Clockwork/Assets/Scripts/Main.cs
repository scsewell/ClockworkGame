using UnityEngine;
using Framework;

public class Main : ComponentSingleton<Main>
{
    [SerializeField]
    public Transform playerCenterOfMass;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F2))
        {
            QualitySettings.vSyncCount = (QualitySettings.vSyncCount + 1) % 3;
        }
    }
}
