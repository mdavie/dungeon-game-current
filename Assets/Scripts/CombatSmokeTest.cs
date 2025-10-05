using System.Linq;
using UnityEngine;

public class CombatSmokeTest : MonoBehaviour
{
    public Camera playerCam;                 // assign Player Camera
    public float hitRange = 1.8f;
    public float hitRadius = 0.35f;
    public LayerMask hitMask = ~0;           // start with Everything; tighten later

    void Reset()
    {
        if (!playerCam) playerCam = Camera.main;
    }

    void Update()
    {
        // Space = do a forward spherecast and apply 1 damage
        if (Input.GetKeyDown(KeyCode.Space))
        {
            var hit = DoSpherecast();
            if (hit.HasValue)
            {
                var mh = hit.Value.collider.GetComponentInParent<MonsterHealth>();
                Debug.Log(mh ? $"[CombatSmokeTest] HIT {mh.name} -> TakeDamage(1)"
                             : $"[CombatSmokeTest] HIT {hit.Value.collider.name} (no MonsterHealth found)");
                if (mh) mh.TakeDamage(1);
            }
            else
            {
                Debug.Log("[CombatSmokeTest] swing: no hit");
            }
        }

        // K = kill nearest monster instantly (debug)
        if (Input.GetKeyDown(KeyCode.K))
        {
            var all = FindObjectsOfType<MonsterHealth>(true);
            if (all.Length == 0) { Debug.Log("[CombatSmokeTest] No MonsterHealth instances in scene."); return; }

            var me = playerCam ? playerCam.transform.position : transform.position;
            var nearest = all.OrderBy(m => Vector3.Distance(m.transform.position, me)).First();
            Debug.Log($"[CombatSmokeTest] Kill nearest: {nearest.name}");
            nearest.Kill();
        }

        // F1 = count monsters in scene
        if (Input.GetKeyDown(KeyCode.F1))
        {
            var all = FindObjectsOfType<MonsterHealth>(true);
            Debug.Log($"[CombatSmokeTest] MonsterHealth instances in scene: {all.Length}");
        }
    }

    RaycastHit? DoSpherecast()
    {
        if (!playerCam) return null;
        var ray = new Ray(playerCam.transform.position, playerCam.transform.forward);
        if (Physics.SphereCast(ray, hitRadius, out var hit, hitRange, hitMask, QueryTriggerInteraction.Ignore))
        {
            Debug.DrawLine(ray.origin, hit.point, Color.red, 0.5f);
            return hit;
        }
        Debug.DrawRay(ray.origin, ray.direction * hitRange, Color.yellow, 0.25f);
        return null;
    }
}
