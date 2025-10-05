using UnityEngine;

public class FireballCaster : MonoBehaviour
{
    public static FireballCaster Instance { get; private set; }

    [Header("Spawn")]
    public Transform origin;                  // drag Player Camera (or a child empty) here
    public GameObject projectilePrefab;       // your Fireball prefab (below)
    public Vector3 spawnOffset = new Vector3(0, 0.8f, 0.2f);

    void Awake()
    {
        if (Instance && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void CastAt(Transform target)
    {
        if (!projectilePrefab || !origin || !target) return;

        var pos = origin.position + origin.TransformVector(spawnOffset);
        var go  = Instantiate(projectilePrefab, pos, Quaternion.identity);
        var fb  = go.GetComponent<FireballProjectile>();
        if (!fb) fb = go.AddComponent<FireballProjectile>();
        fb.target = target;
    }
}
