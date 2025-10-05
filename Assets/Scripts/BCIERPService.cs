using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Events;

[DefaultExecutionOrder(-1000)]
public class BCIERPService : MonoBehaviour
{
    public static BCIERPService Instance { get; private set; }

    [Header("Prefab & Parent")]
    [Tooltip("Prefab asset of a working ERP tag visual (drag from Project; must NOT contain BCIERPTargetBinder).")]
    public GameObject erpTargetPrefab;

    [Tooltip("ERPTags object from the Hierarchy that your ERP controller scans.")]
    public Transform erpGroupParent;

    [Header("Options")]
    [Tooltip("If ON, wires any zero-arg UnityEvent found on the visual to the binder's OnBCISelected. Safe, but optional.")]
    public bool autoWireZeroArgUnityEvents = true;

    // ---- internal state ----
    readonly Dictionary<int, BCIERPTargetBinder> _idToBinder = new();
    readonly Dictionary<BCIERPTargetBinder, (int id, GameObject vis)> _binderToData = new();
    int _nextId = 1;
    const int MAX_TAGS = 512; // hard cap so a bug can’t explode the scene

    void Awake()
    {
        if (Instance && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public int RegisterTarget(BCIERPTargetBinder binder)
    {
        if (binder == null) throw new ArgumentNullException(nameof(binder));

        // Already registered? Reuse.
        if (_binderToData.TryGetValue(binder, out var existing))
            return existing.id;

        // Sanity checks
        if (erpTargetPrefab == null)
        {
            Debug.LogError("[BCIERPService] erpTargetPrefab is NULL. Create a prefab from a working ERPFlashTag3D instance and assign it.");
            return -1;
        }
        if (erpGroupParent == null)
        {
            Debug.LogError("[BCIERPService] erpGroupParent is NULL. Drag the ERPTags scene object here.");
            return -1;
        }
        if (_idToBinder.Count >= MAX_TAGS)
        {
            Debug.LogError("[BCIERPService] Refusing to register more tags (MAX_TAGS reached). Check for a spawn loop.");
            return -1;
        }
        if (PrefabContainsBinder(erpTargetPrefab))
        {
            Debug.LogError("[BCIERPService] The assigned erpTargetPrefab contains BCIERPTargetBinder. "
                + "This will cause an infinite spawn loop. Create a prefab that has ONLY the visual (no binder).");
            return -1;
        }

        int id = _nextId++;

        // Create visual under the ERP group
        var vis = Instantiate(erpTargetPrefab, erpGroupParent);
        vis.name = $"ERPTag_{binder.gameObject.name}_{id}";

        // Make it follow the monster
        var follow = vis.GetComponent<FollowWorldTarget>() ?? vis.AddComponent<FollowWorldTarget>();
        follow.target = binder.visualMount ? binder.visualMount : binder.transform;
        follow.worldOffset = binder.localOffset;

        // Scale
        vis.transform.localScale = Vector3.one * Mathf.Max(0.001f, binder.visualScale);

        // Optionally set an int id if the visual exposes one
        TrySetIntPropertyOrField(vis, "TargetId", id);
        TrySetIntPropertyOrField(vis, "targetId", id);
        TrySetIntPropertyOrField(vis, "Id",       id);
        TrySetIntPropertyOrField(vis, "id",       id);
        TrySetIntPropertyOrField(vis, "ID",       id);

        // Optional: wire any zero-arg UnityEvents to binder.OnBCISelected
        if (autoWireZeroArgUnityEvents)
            TryAttachZeroArgUnityEvents(vis, binder);

        // Track both ways
        _idToBinder[id] = binder;
        _binderToData[binder] = (id, vis);

        if (binder.log) Debug.Log($"[BCIERPService] Registered id={id} for {binder.name} (created {vis.name})");
        return id;
    }

    public void UnregisterTarget(int id, BCIERPTargetBinder binder)
    {
        if (_binderToData.TryGetValue(binder, out var data))
        {
            if (data.vis) Destroy(data.vis);
            _binderToData.Remove(binder);
        }
        _idToBinder.Remove(id);
    }

    public void OnTargetSelected(int id)
    {
        if (_idToBinder.TryGetValue(id, out var binder) && binder != null)
            binder.OnBCISelected();
        else
            Debug.LogWarning($"[BCIERPService] Selection for unknown id {id}");
    }

    // ---------- helpers ----------
    static bool PrefabContainsBinder(GameObject prefab)
    {
        // Safe: only inspects the prefab asset; doesn’t instantiate it
        return prefab.GetComponentInChildren<BCIERPTargetBinder>(true) != null;
    }

    static void TrySetIntPropertyOrField(GameObject go, string name, int value)
    {
        foreach (var c in go.GetComponentsInChildren<Component>(true))
        {
            if (!c) continue;
            var t = c.GetType();
            var p = t.GetProperty(name, BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic);
            if (p != null && p.CanWrite && p.PropertyType == typeof(int)) { p.SetValue(c, value); return; }
            var f = t.GetField(name, BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic);
            if (f != null && f.FieldType == typeof(int)) { f.SetValue(c, value); return; }
        }
    }

    static void TryAttachZeroArgUnityEvents(GameObject vis, BCIERPTargetBinder binder)
    {
        foreach (var c in vis.GetComponentsInChildren<Component>(true))
        {
            if (!c) continue;
            var t = c.GetType();

            foreach (var f in t.GetFields(BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic))
            {
                if (typeof(UnityEvent).IsAssignableFrom(f.FieldType))
                {
                    (f.GetValue(c) as UnityEvent)?.AddListener(binder.OnBCISelected);
                }
            }
            foreach (var p in t.GetProperties(BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic))
            {
                if (p.CanRead && typeof(UnityEvent).IsAssignableFrom(p.PropertyType))
                {
                    (p.GetValue(c, null) as UnityEvent)?.AddListener(binder.OnBCISelected);
                }
            }
        }
    }

    // Optional debug: call from a test key script
    public void DebugSpawnTestAtCamera()
    {
        if (erpTargetPrefab == null) { Debug.LogError("[BCIERPService] erpTargetPrefab is NULL."); return; }
        if (erpGroupParent == null) { Debug.LogError("[BCIERPService] erpGroupParent is NULL."); return; }
        if (PrefabContainsBinder(erpTargetPrefab))
        {
            Debug.LogError("[BCIERPService] erpTargetPrefab contains BCIERPTargetBinder — fix the prefab.");
            return;
        }

        var cam = Camera.main;
        var pos = cam ? cam.transform.position + cam.transform.forward * 2f + Vector3.up * 1.5f : Vector3.zero;
        var go = Instantiate(erpTargetPrefab, erpGroupParent);
        go.name = "ERP_TestSpawn";
        go.transform.position = pos;
        Debug.Log("[BCIERPService] Spawned test ERP tag under ERPTags.");
    }
}
