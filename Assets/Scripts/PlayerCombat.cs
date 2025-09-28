using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    [Header("References")]
    public Transform cam;         // assign Player Camera
    public Transform weapon;      // assign Player Weapon

    [Header("Attack Settings")]
    public float range = 3.5f;
    public float cooldown = 0.35f;
    public float thrustDistance = 0.15f;
    public float thrustTime = 0.12f;
    [Range(0f, 1f)] public float hitChance = 0.5f;

    [Header("Raycast Layers")]
    public LayerMask hitLayers = ~0; // include Default + Monster, exclude Ignore Raycast

    [Header("Audio")]
    public AudioSource sfxSource;  // assign the AudioSource on Player
    public AudioClip hitSfx;       // assign a short "hit" clip
    public AudioClip missSfx;      // optional "miss" clip

    private float nextReadyTime = 0f;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) && Time.time >= nextReadyTime)
        {
            nextReadyTime = Time.time + cooldown;
            StartCoroutine(Attack());
        }
    }

    IEnumerator Attack()
    {
        // Thrust forward
        Vector3 start = weapon != null ? weapon.localPosition : Vector3.zero;
        Vector3 end   = start + new Vector3(0f, 0f, thrustDistance);

        float t = 0f;
        while (t < thrustTime)
        {
            t += Time.deltaTime;
            if (weapon != null) weapon.localPosition = Vector3.Lerp(start, end, t / thrustTime);
            yield return null;
        }

        // Roll to hit + raycast
        bool rolledHit = Random.value < Mathf.Clamp01(hitChance);
        bool didHit = false;

        if (cam != null)
        {
            Ray ray = new Ray(cam.position, cam.forward);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, range, hitLayers))
            {
                MonsterHealth hp = hit.collider.GetComponentInParent<MonsterHealth>();
                if (hp != null && rolledHit)
                {
                    hp.TakeDamage(1);
                    didHit = true;

                    // Play hit SFX (prefer a Player AudioSource to avoid creating temp objects)
                    if (sfxSource != null && hitSfx != null)
                        sfxSource.PlayOneShot(hitSfx);
                    else if (hitSfx != null)
                        AudioSource.PlayClipAtPoint(hitSfx, hit.point);
                }
            }
        }

        // Miss SFX if desired
        if (!didHit && missSfx != null)
        {
            if (sfxSource != null) sfxSource.PlayOneShot(missSfx);
            else AudioSource.PlayClipAtPoint(missSfx, transform.position);
        }

        // Return weapon
        t = 0f;
        while (t < thrustTime)
        {
            t += Time.deltaTime;
            if (weapon != null) weapon.localPosition = Vector3.Lerp(end, start, t / thrustTime);
            yield return null;
        }
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        if (cam == null) return;
        Gizmos.DrawLine(cam.position, cam.position + cam.forward * range);
    }
#endif
}