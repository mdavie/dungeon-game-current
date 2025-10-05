using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BCIAutoWireButtons : MonoBehaviour
{
    [Header("Find criteria for buttons")]
    public string requiredTag  = "BCIButton";   // set this tag on your button prefab
    public string nameContains = "Button";      // optional fallback if tag not set

    [Header("Timing")]
    public int framesToWait = 5;                // delay for MapBuilder to finish

    void Start()
    {
        StartCoroutine(ScanAfterBuild());
    }

    private IEnumerator ScanAfterBuild()
    {
        for (int i = 0; i < framesToWait; i++) yield return null;

        int wired = 0;

        // Fast path: use tag search if provided
        if (!string.IsNullOrEmpty(requiredTag) && TagExists(requiredTag))
        {
            var tagged = GameObject.FindGameObjectsWithTag(requiredTag);
            foreach (var go in tagged)
                wired += EnsureBinder(go);
        }
        else
        {
            // Fallback: scan all GameObjects (handles inactive too)
            foreach (var go in FindAllGameObjects(includeInactive: true))
            {
                if (!go.activeInHierarchy) continue;

                bool pass = false;
                if (!string.IsNullOrEmpty(requiredTag) && go.CompareTag(requiredTag)) pass = true;
                else if (!string.IsNullOrEmpty(nameContains) && go.name.Contains(nameContains)) pass = true;

                if (pass) wired += EnsureBinder(go);
            }
        }

        Debug.Log($"[BCIAutoWireButtons] Wired {wired} runtime button(s) for ERP.");
    }

    private int EnsureBinder(GameObject go)
    {
        if (go.GetComponent<BCIERPTargetBinder>() == null)
        {
            var b = go.AddComponent<BCIERPTargetBinder>();
            // Optional defaults:
            b.localOffset = new Vector3(0f, 1.0f, 0f);
            b.visualScale = 1.0f;
            return 1;
        }
        return 0;
    }

    private static bool TagExists(string tag)
    {
        try { var _ = GameObject.FindGameObjectsWithTag(tag); return true; }
        catch { return false; }
    }

    // Cross-version helper to find all GameObjects, including inactive, without obsolete API
    private static IEnumerable<GameObject> FindAllGameObjects(bool includeInactive)
    {
#if UNITY_2022_2_OR_NEWER
        // New API: choose sorting = None for speed
        return Object.FindObjectsByType<GameObject>(
            includeInactive ? FindObjectsInactive.Include : FindObjectsInactive.Exclude,
            FindObjectsSortMode.None
        );
#else
        // Older Unity fallback (keeps behavior close to original)
        return Object.FindObjectsOfType<GameObject>(includeInactive);
#endif
    }
}
