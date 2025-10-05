using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
public class FixButtonRaycasts : MonoBehaviour
{
    void Awake()
    {
        // Turn OFF raycasts on all Graphics under this icon...
        foreach (var g in GetComponentsInChildren<Graphic>(true))
            g.raycastTarget = false;

        // ...but turn ON the one that actually has the Button component
        var btn = GetComponentInChildren<Button>(true);
        if (btn && btn.targetGraphic) btn.targetGraphic.raycastTarget = true;
    }
}
