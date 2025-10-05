using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIRaycastDebugger : MonoBehaviour
{
    public Font font;
    List<RaycastResult> results = new();

    void OnGUI()
    {
        if (EventSystem.current == null) return;

        var ped = new PointerEventData(EventSystem.current) { position = Input.mousePosition };
        results.Clear();
        EventSystem.current.RaycastAll(ped, results);

        var y = 10f;
        GUI.skin.label.font = font;
        GUI.Label(new Rect(10, y, 800, 20), $"Hits under mouse: {results.Count}"); y += 20;
        for (int i = 0; i < Mathf.Min(results.Count, 8); i++)
        {
            var r = results[i];
            GUI.Label(new Rect(10, y, 1200, 20), $"{i+1}. {r.gameObject.name} (canvas:{r.module?.name})");
            y += 20;
        }
    }
}
