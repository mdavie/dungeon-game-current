using UnityEngine;

public class ERPGlobalActions : MonoBehaviour
{
    [Tooltip("Only used by the *Int/String* overloads below.")]
    public int openAllDoorsTagId = 1;

    // ---- Use this if your ERP event is UnityEvent<int> ----
    public void OnErpTargetSelectedInt(int id)
    {
        if (id == openAllDoorsTagId)
            DoorUtils.OpenAllDoors();
    }

    // ---- Use this if your ERP event is UnityEvent<string> ----
    public void OnErpTargetSelectedString(string id)
    {
        if (int.TryParse(id, out var n) && n == openAllDoorsTagId)
            DoorUtils.OpenAllDoors();
    }

    // ---- Use this if your ERP event has NO parameters (UnityEvent) ----
    // (e.g., you wire this directly on the Tag #1 object’s event)
    public void OnErpTargetSelectedNoArgs()
    {
        DoorUtils.OpenAllDoors();
    }

    // Handy to call directly from any UnityEvent
    public void OpenAllDoors()  => DoorUtils.OpenAllDoors();
    public void CloseAllDoors() => DoorUtils.CloseAllDoors();
}
