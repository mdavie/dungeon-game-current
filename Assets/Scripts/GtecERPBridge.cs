using UnityEngine;

public class GtecERPBridge : MonoBehaviour
{
    // Hook this from your ERP pipeline’s selection event (int targetId)
    public void OnErpTargetSelected(int targetId)
    {
        BCIERPService.Instance?.OnTargetSelected(targetId);
    }
}
