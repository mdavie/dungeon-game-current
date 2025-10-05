using UnityEngine;
using UnityEngine.Events;

//
// NOTE:
// - This version does NOT require a ButtonLink type.
// - It will invoke common "press" methods by NAME if present (OnPressed / Press) on any component.
// - It still integrates with the BCISelectionRouter via IBCISelectable.
//   If you don't have BCISelectionRouter.cs in your project yet, either add it or
//   remove " : IBCISelectable " and the Register/Unregister calls.
//

// If IBCISelectable isn't already defined elsewhere, uncomment this interface.
// public interface IBCISelectable { void OnBCISelected(); }

[DisallowMultipleComponent]
public class BCIFlashTagButton : MonoBehaviour, IBCISelectable
{
    [Header("Identification")]
    [Tooltip("Must match the target ID your BCI pipeline outputs when this object is selected.")]
    public string targetId;

    [Header("Actions on Select")]
    [Tooltip("Invoked when the BCI selects this target.")]
    public UnityEvent onBCISelected;

    // Cache any component that exposes a "OnPressed" or "Press" method.
    private Component[] _allComponents;

    private void Awake()
    {
        // Cache all components for quick reflection later
        _allComponents = GetComponents<Component>();
    }

    private void OnEnable()
    {
        // Register with router if available
        var t = System.Type.GetType("BCISelectionRouter");
        if (!string.IsNullOrEmpty(targetId) && t != null)
        {
            // BCISelectionRouter.Register(string id, IBCISelectable selectable)
            var register = t.GetMethod("Register", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
            if (register != null)
            {
                register.Invoke(null, new object[] { targetId, this });
            }
        }
    }

    private void OnDisable()
    {
        // Unregister with router if available
        var t = System.Type.GetType("BCISelectionRouter");
        if (!string.IsNullOrEmpty(targetId) && t != null)
        {
            // BCISelectionRouter.Unregister(string id, IBCISelectable selectable)
            var unregister = t.GetMethod("Unregister", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
            if (unregister != null)
            {
                unregister.Invoke(null, new object[] { targetId, this });
            }
        }
    }

    /// <summary>
    /// Called by the BCISelectionRouter when this target is selected.
    /// </summary>
    public void OnBCISelected()
    {
        // 1) Designer hook
        onBCISelected?.Invoke();

        // 2) Try common button methods by NAME on any attached component:
        //    - "OnPressed" (our earlier convention)
        //    - "Press"     (common name in click handlers)
        if (InvokeFirstMethodNamed("OnPressed")) return;
        if (InvokeFirstMethodNamed("Press"))     return;

        // 3) If your project uses another name (e.g., "Click", "Toggle"),
        //    you can add more tries here:
        // if (InvokeFirstMethodNamed("Click")) return;
        // if (InvokeFirstMethodNamed("Toggle")) return;

        // 4) Nothing handled: that's OK—BCI selected this object but it's not wired to do anything yet.
        //    You can wire behavior via the UnityEvent above without writing code.
        // Debug.Log($"[BCIFlashTagButton] Selected '{name}' but no handler found.");
    }

    /// <summary>
    /// Helper: finds the first component on this GameObject that has a public, parameterless method
    /// with the given name, and invokes it. Returns true if something was invoked.
    /// </summary>
    private bool InvokeFirstMethodNamed(string methodName)
    {
        if (_allComponents == null || _allComponents.Length == 0) return false;

        for (int i = 0; i < _allComponents.Length; i++)
        {
            var c = _allComponents[i];
            if (c == null) continue;

            var type = c.GetType();
            var m = type.GetMethod(methodName, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic, null, System.Type.EmptyTypes, null);
            if (m != null)
            {
                try { m.Invoke(c, null); } catch { /* ignore */ }
                return true;
            }
        }
        return false;
    }
}
