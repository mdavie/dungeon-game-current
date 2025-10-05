using UnityEngine;
using UnityEngine.Events;
using System.Reflection;

[DisallowMultipleComponent]
public class BCIERPTargetBinder : MonoBehaviour
{
    [Header("Visual Mount (used by the ERP service)")]
    [Tooltip("Where to anchor the ERP tag visual. Defaults to this transform.")]
    public Transform visualMount;

    [Tooltip("Offset for the ERP tag relative to the mount (treated as world offset by the service).")]
    public Vector3 localOffset = new Vector3(0f, 1.0f, 0f);

    [Tooltip("Uniform scale applied to the ERP tag visual.")]
    public float visualScale = 1.0f;

    [Header("Selection")]
    [Tooltip("Invoked when this ERP target is selected.")]
    public UnityEvent onBCISelected;

    [Tooltip("Also try common no-arg methods on components (OnPressed/Press). Turn off if you only use the UnityEvent.")]
    public bool callCommonPressMethods = true;

    [Tooltip("Extra method names to try (no-arg) when selected, in addition to OnPressed/Press.")]
    public string[] extraMethodNames;

    [Header("Debug")]
    public bool log = false;

    [HideInInspector] public int targetId = -1;

    // Cached components; refreshed on enable and before we invoke to tolerate runtime changes
    private Component[] _components;

    void Awake()
    {
        if (!visualMount) visualMount = transform;
        _components = GetComponents<Component>();
    }

    void OnEnable()
    {
        if (BCIERPService.Instance == null)
        {
            Debug.LogError("[BCIERPTargetBinder] BCIERPService missing in scene.");
            return;
        }
        targetId = BCIERPService.Instance.RegisterTarget(this);
        if (log) Debug.Log($"[BCIERPTargetBinder] Registered id={targetId} for {name}");
    }

    void OnDisable()
    {
        if (targetId > 0 && BCIERPService.Instance != null)
        {
            BCIERPService.Instance.UnregisterTarget(targetId, this);
            if (log) Debug.Log($"[BCIERPTargetBinder] Unregistered id={targetId} for {name}");
        }
        targetId = -1;
    }

    /// <summary>Called by ERP when this target is selected.</summary>
    public void OnBCISelected()
    {
        if (log) Debug.Log($"[BCIERPTargetBinder] Selected {name} (id={targetId})");

        // 1) Designer / code hooks
        onBCISelected?.Invoke();

        // 2) Optional reflective fallbacks (keeps older button scripts working)
        if (!callCommonPressMethods) return;

        // Refresh components in case new scripts were added after Awake
        _components = GetComponents<Component>();

        if (InvokeMethodIfExists("OnPressed")) return;
        if (InvokeMethodIfExists("Press"))     return;

        if (extraMethodNames != null)
        {
            for (int i = 0; i < extraMethodNames.Length; i++)
            {
                var n = extraMethodNames[i];
                if (!string.IsNullOrEmpty(n) && InvokeMethodIfExists(n)) return;
            }
        }
        // If nothing handled, that's fine—some targets only use the UnityEvent.
    }

    private bool InvokeMethodIfExists(string methodName)
    {
        if (_components == null) return false;

        for (int i = 0; i < _components.Length; i++)
        {
            var c = _components[i];
            if (c == null) continue;

            // public or non-public, instance, no parameters
            var m = c.GetType().GetMethod(
                methodName,
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                binder: null,
                types: System.Type.EmptyTypes,
                modifiers: null
            );

            if (m != null)
            {
                try
                {
                    m.Invoke(c, null);
                    if (log) Debug.Log($"[BCIERPTargetBinder] Invoked {c.GetType().Name}.{methodName} on {name}");
                }
                catch (System.Exception ex)
                {
                    Debug.LogWarning($"[BCIERPTargetBinder] Exception invoking {c.GetType().Name}.{methodName}: {ex.Message}");
                }
                return true;
            }
        }
        return false;
    }

    // Handy in Play Mode: right-click the component header and choose "Simulate Select"
    [ContextMenu("Simulate Select")]
    void ContextSimulateSelect()
    {
        if (!Application.isPlaying)
        {
            Debug.LogWarning("[BCIERPTargetBinder] Simulate Select only works in Play Mode.");
            return;
        }
        OnBCISelected();
    }
}
