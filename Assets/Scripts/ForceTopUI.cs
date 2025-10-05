using UnityEngine;

[RequireComponent(typeof(Canvas))]
public class ForceTopUI : MonoBehaviour
{
    public int sortingOrder = 1000;
    void Awake()
    {
        var c = GetComponent<Canvas>();
        c.overrideSorting = true;
        c.sortingOrder = sortingOrder; // for Overlay; for Camera/World also set sortingLayerName if needed
    }
}
