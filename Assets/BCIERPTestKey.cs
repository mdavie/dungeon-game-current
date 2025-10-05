using UnityEngine;

public class BCIERPTestKey : MonoBehaviour
{
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
            BCIERPService.Instance?.DebugSpawnTestAtCamera();
    }
}
