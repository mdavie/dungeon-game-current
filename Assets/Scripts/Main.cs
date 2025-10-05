using UnityEngine;

public class Main : MonoBehaviour
{
    [Header("Scene Refs")]
    [SerializeField] private GameObject playerGameObj;
    [SerializeField] private Camera cam;

    public static Map map;
    public static PlayerData player;

    public const int   CELL_SIZE     = 20;
    public const float MOVE_LERP_TIME= 0.4f;
    public const float HEAD_HEIGHT   = 10f;
    public const float LIGHT_HEIGHT  = 12f;

    void Awake()
    {
        // Fixed seed for reproducible décor/monsters. Replace with SeedFrom(levelName) if desired.
        UnityEngine.Random.InitState(123456);
    }

    void Start()
    {
        if (playerGameObj == null) { Debug.LogError("Main: playerGameObj not assigned"); return; }
        if (cam == null)           { Debug.LogError("Main: cam not assigned"); return; }

        // Init Player
        player = new PlayerData(playerGameObj);

        // Position player light at a constant height
        var lightTr = playerGameObj.GetComponentInChildren<Light>()?.transform;
        if (lightTr != null)
            lightTr.position = new Vector3(0f, LIGHT_HEIGHT, 20f);

        // Build map
        var mb = GetComponent<MapBuilder>();
        if (mb == null) { Debug.LogError("Main: MapBuilder not found on the same GameObject"); return; }

        string levelName = "level-1";
        map = mb.buildMap(levelName);

        // Resize camera viewport to leave 240px for HUD on the right
        float newW = Mathf.Clamp01(1f - (240f / Mathf.Max(1, (float)Screen.width)));
        cam.rect = new Rect(0f, 0f, newW, 1f);
    }

    public PlayerAudio getAudio() => playerGameObj != null
        ? playerGameObj.GetComponentInChildren<PlayerAudio>()
        : null;
}

