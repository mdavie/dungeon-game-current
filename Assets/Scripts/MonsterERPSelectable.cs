// Assets/Scripts/BCI/MonsterERPSelectable.cs
using UnityEngine;

[RequireComponent(typeof(MonsterHealth))]
public class MonsterERPSelectable : MonoBehaviour
{
    // Called by the ERP binder when the player selects this target
    public void OnBCISelected()
    {
        FireballCaster.Instance?.CastAt(transform);
    }
}
