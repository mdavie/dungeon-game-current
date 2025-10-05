using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
[RequireComponent(typeof(Rigidbody))]
public class FireballProjectile : MonoBehaviour
{
    [Header("Target & Motion")]
    public Transform target;
    public float speed = 14f;
    public float maxLifetime = 6f;

    [Header("Collision (trigger sphere)")]
    public float triggerRadius = 0.35f;      // world-space hit size
    public bool autoSizeFromVisual = true;   // infer radius from the sphere mesh size
    public Renderer visualRenderer;          // drag the fireball's MeshRenderer
    public LayerMask hittableMask = ~0;      // (optional) set to Monster layer for clean hits

    [Header("Damage")]
    public bool killInstantly = true;        // or call TakeDamage(n) if you prefer staged damage
    public int damage = 3;

    SphereCollider _trigger;
    Rigidbody _rb;
    MonsterHealth _targetMH;

    void Awake()
    {
        _trigger = GetComponent<SphereCollider>();
        _trigger.isTrigger = true;

        _rb = GetComponent<Rigidbody>();
        _rb.isKinematic = true;     // we move it manually
        _rb.useGravity = false;
    }

    void Start()
    {
        if (target) _targetMH = target.GetComponentInParent<MonsterHealth>();

        // Auto-size the trigger to match the visual size so big fireballs hit earlier
        if (autoSizeFromVisual && !visualRenderer) visualRenderer = GetComponentInChildren<MeshRenderer>();
        float worldRadius = triggerRadius;
        if (autoSizeFromVisual && visualRenderer)
        {
            // a decent approximation: radius ≈ largest axis extent of the visual
            var ext = visualRenderer.bounds.extents;
            worldRadius = Mathf.Max(ext.x, Mathf.Max(ext.y, ext.z));
        }
        // convert world radius to the collider's local radius
        float uniformScale = transform.lossyScale.x;
        if (uniformScale <= 0.0001f) uniformScale = 1f;
        _trigger.radius = worldRadius / uniformScale;

        Destroy(gameObject, maxLifetime);
    }

    void Update()
    {
        if (!target || (_targetMH && _targetMH.IsDead)) { Destroy(gameObject); return; }

        Vector3 to = target.position - transform.position;
        float step = speed * Time.deltaTime;

        // arrive exactly on target if close
        if (step >= to.magnitude) transform.position = target.position;
        else                      transform.position += to.normalized * step;

        transform.forward = Vector3.Slerp(transform.forward, to.normalized, 0.35f);
    }

    void OnTriggerEnter(Collider other)
    {
        // optional layer mask filter
        if (((1 << other.gameObject.layer) & hittableMask) == 0) return;

        var mh = other.GetComponentInParent<MonsterHealth>();
        if (mh != null && !mh.IsDead)
        {
            if (killInstantly) mh.Kill();
            else               mh.TakeDamage(Mathf.Max(1, damage));
            Destroy(gameObject);
        }
    }
}
