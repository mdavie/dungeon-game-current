using UnityEngine;

[RequireComponent(typeof(MonsterHealth))]
public class FireballTarget : MonoBehaviour
{
    [Tooltip("Optional override; if empty, this transform is used.")]
    public Transform targetOverride;

    // Use this method in the ERP Flash 'On Selection' (no parameters)
    public void CastAtSelf()
    {
        var t = targetOverride ? targetOverride : transform;
        FireballCaster.Instance?.CastAt(t);
    }
}
