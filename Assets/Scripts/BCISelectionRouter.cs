using System.Collections.Generic;
using UnityEngine;

public static class BCISelectionRouter
{
    private static readonly Dictionary<string, IBCISelectable> _byId = new Dictionary<string, IBCISelectable>();

    public static void Register(string id, IBCISelectable selectable)
    {
        if (string.IsNullOrEmpty(id) || selectable == null) return;
        _byId[id] = selectable;
    }

    public static void Unregister(string id, IBCISelectable selectable)
    {
        if (string.IsNullOrEmpty(id)) return;
        if (_byId.TryGetValue(id, out var current) && current == selectable)
            _byId.Remove(id);
    }

    /// <summary>
    /// Call this from your BCI pipeline when a target with 'id' is selected.
    /// </summary>
    public static void Select(string id)
    {
        if (string.IsNullOrEmpty(id)) return;
        if (_byId.TryGetValue(id, out var selectable) && selectable != null)
        {
            selectable.OnBCISelected();
        }
        else
        {
            Debug.LogWarning($"[BCISelectionRouter] No selectable registered for id '{id}'.");
        }
    }
}

public interface IBCISelectable
{
    void OnBCISelected();
}
