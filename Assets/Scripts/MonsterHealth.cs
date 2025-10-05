using UnityEngine;
using UnityEngine.Events;

[DisallowMultipleComponent]
public class MonsterHealth : MonoBehaviour
{
    [Header("Health")]
    [Tooltip("Number of hits required to kill this monster.")]
    public int hitsToKill = 3;

    [Tooltip("Invoked on every successful hit.")]
    public UnityEvent onHit;

    [Tooltip("Invoked once when the monster dies.")]
    public UnityEvent onDeath;

    [Header("Visual/Cleanup")]
    [Tooltip("Renderers to hide immediately on death (optional).")]
    public Renderer[] renderersToHide;

    [Tooltip("Disable all colliders on death so the player can walk through.")]
    public bool disableCollidersOnDeath = true;

    [Tooltip("Destroy this GameObject after death (seconds). Set 0 to keep it.")]
    public float destroyDelay = 1.25f;

    private int _hits;
    private bool _dead;

    public bool IsDead => _dead;
    public int Hits => _hits;
    public int RemainingHits => Mathf.Max(0, hitsToKill - _hits);

    // ---------- Public API ----------
    /// <summary>Compatibility with existing code: Treats damage as 'hits'.</summary>
    public void TakeDamage(int amount = 1) => Hit(amount);

    /// <summary>Apply one or more hits. Kills the monster when hits >= hitsToKill.</summary>
    public void Hit(int amount = 1)
    {
        if (_dead) return;
        _hits += Mathf.Max(1, amount);
        onHit?.Invoke();
        if (_hits >= hitsToKill)
            Die();
    }

    /// <summary>Force kill (e.g., for debug or spells).</summary>
    public void Kill()
    {
        if (_dead) return;
        _hits = hitsToKill;
        Die();
    }

    // ---------- Internals ----------
    private void Die()
    {
        if (_dead) return;
        _dead = true;

        if (disableCollidersOnDeath)
        {
            var cols = GetComponentsInChildren<Collider>(true);
            for (int i = 0; i < cols.Length; i++)
                cols[i].enabled = false; // lets the player walk past
        }

        if (renderersToHide != null)
            foreach (var r in renderersToHide) if (r) r.enabled = false;

        onDeath?.Invoke();

        if (destroyDelay > 0f)
            Destroy(gameObject, destroyDelay);
    }
}
