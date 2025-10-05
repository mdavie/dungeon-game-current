using System.Collections;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class PlayerWeaponAttack : MonoBehaviour
{
    [Header("References")]
    public Transform weaponVisual;     // assign your Player Weapon transform (the thing that moves)
    public Camera playerCam;           // usually Player Camera
    public ClassSelectionManager classSelector; // optional: gate attack to Warrior

    [Header("Attack")]
    public KeyCode attackKey = KeyCode.Space;
    public float thrustDistance = 0.5f;     // how far the weapon moves forward (local z)
    public float thrustTime = 0.12f;        // time forward
    public float recoverTime = 0.12f;       // time back
    public float cooldown = 0.15f;

    [Header("Hit Detection")]
    public float hitRange = 1.6f;           // how far from camera we can hit
    public float hitRadius = 0.35f;         // sphere radius
    public LayerMask hitMask;               // set to your Monster layer (and/or Default if monsters are Default)

    [Header("FX (optional)")]
    public AudioClip swingSfx;
    public AudioClip hitSfx;

    AudioSource _audio;
    bool _busy;

    void Awake()
    {
        _audio = GetComponent<AudioSource>();
        if (!playerCam) playerCam = Camera.main;
    }

    void Update()
    {
        if (Input.GetKeyDown(attackKey))
            TryAttack();
    }

    void TryAttack()
    {
        // Only allow Warrior to attack (optional)
        if (classSelector != null && classSelector.Current != ClassSelectionManager.ClassType.Warrior)
            return;

        if (_busy) return;
        StartCoroutine(AttackRoutine());
    }

    IEnumerator AttackRoutine()
    {
        _busy = true;

        if (swingSfx) _audio.PlayOneShot(swingSfx);

        // Move weapon forward
        Vector3 start = weaponVisual ? weaponVisual.localPosition : Vector3.zero;
        Vector3 fwd   = weaponVisual ? Vector3.forward : Vector3.zero; // local forward
        float t = 0f;
        while (t < thrustTime)
        {
            t += Time.deltaTime;
            float a = Mathf.Clamp01(t / thrustTime);
            if (weaponVisual)
                weaponVisual.localPosition = start + fwd * (thrustDistance * a);
            yield return null;
        }

        // Do the hit at the apex of the thrust
        DoHit();

        // Recover back
        t = 0f;
        while (t < recoverTime)
        {
            t += Time.deltaTime;
            float a = Mathf.Clamp01(t / recoverTime);
            if (weaponVisual)
                weaponVisual.localPosition = Vector3.Lerp(start + fwd * thrustDistance, start, a);
            yield return null;
        }

        // Cooldown
        yield return new WaitForSeconds(cooldown);
        _busy = false;
    }

    void DoHit()
    {
        if (!playerCam) return;

        Ray ray = new Ray(playerCam.transform.position, playerCam.transform.forward);
        if (Physics.SphereCast(ray, hitRadius, out RaycastHit hit, hitRange, hitMask, QueryTriggerInteraction.Ignore))
        {
            var mh = hit.collider.GetComponentInParent<MonsterHealth>();
            if (mh != null && !mh.IsDead)
            {
                mh.Hit(1); // 1 hit; will die after 3
                if (hitSfx) _audio.PlayOneShot(hitSfx);
            }
        }
    }
}
