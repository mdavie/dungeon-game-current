// Assets/Scripts/BCI/ClericERPMode.cs
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class ClericERPMode : MonoBehaviour
{
    [Header("References")]
    public ClassSelectionManager classSelector;  // assign your ClassSelectionManager
    public GameObject playerWeapon;              // optional: sword to hide for Cleric
    public Transform fireballOriginOverride;     // optional: set FireballCaster origin (e.g., player camera)

    [Header("ERP Visual")]
    public float erpOffsetY = 1.6f;              // place tag above monster head
    public float erpVisualScale = 1.0f;

    [Header("Scanning")]
    public float rescanInterval = 1.0f;          // while Cleric is active

    [Header("Debug")]
    public bool log = false;

    bool _active;
    Coroutine _scanCo;

    void Start()
    {
        if (FireballCaster.Instance == null)
        {
            // Add a caster if none exists yet
            var go = new GameObject("FireballCaster");
            var caster = go.AddComponent<FireballCaster>();
            if (fireballOriginOverride) caster.origin = fireballOriginOverride;
        }
        else if (fireballOriginOverride)
        {
            FireballCaster.Instance.origin = fireballOriginOverride;
        }

        // Start watching selection
        StartCoroutine(MonitorSelection());
    }

    IEnumerator MonitorSelection()
    {
        while (true)
        {
            bool cleric = classSelector && classSelector.Current == ClassSelectionManager.ClassType.Cleric;
            if (cleric != _active)
            {
                SetActive(cleric);
            }
            yield return null;
        }
    }

    public void SetActive(bool on)
    {
        _active = on;

        if (playerWeapon) playerWeapon.SetActive(!on); // hide sword when cleric

        if (on)
        {
            AttachToAllMonsters();
            if (_scanCo == null) _scanCo = StartCoroutine(RescanLoop());
        }
        else
        {
            DetachFromAllMonsters();
            if (_scanCo != null) { StopCoroutine(_scanCo); _scanCo = null; }
        }

        if (log) Debug.Log($"[ClericERPMode] {(on ? "ENABLED" : "DISABLED")}");
    }

    IEnumerator RescanLoop()
    {
        var wait = new WaitForSeconds(rescanInterval);
        while (_active)
        {
            AttachToAllMonsters();
            yield return wait;
        }
    }

    void AttachToAllMonsters()
    {
        var monsters = FindObjectsOfType<MonsterHealth>(true);
        for (int i = 0; i < monsters.Length; i++)
            EnsureBinder(monsters[i]);
    }

    void DetachFromAllMonsters()
    {
        var binders = FindObjectsOfType<BCIERPTargetBinder>(true);
        for (int i = 0; i < binders.Length; i++)
        {
            var onMonster = binders[i].GetComponent<MonsterHealth>() != null
                         || binders[i].GetComponentInParent<MonsterHealth>() != null;
            if (onMonster) Destroy(binders[i]);
        }
    }

    void EnsureBinder(MonsterHealth mh)
    {
        if (!mh || mh.IsDead) return;

        var binder = mh.GetComponent<BCIERPTargetBinder>();
        if (!binder) binder = mh.gameObject.AddComponent<BCIERPTargetBinder>();

        binder.visualMount = mh.transform;
        binder.localOffset = new Vector3(0f, erpOffsetY, 0f);
        binder.visualScale = erpVisualScale;

        // Ensure we have a monster-selectable component
        var selectable = mh.GetComponent<MonsterERPSelectable>();
        if (!selectable) selectable = mh.gameObject.AddComponent<MonsterERPSelectable>();

        // Wire binder's UnityEvent to the monster action (avoid duplicate listeners)
        // (Clear just our method if already present)
        binder.onBCISelected.RemoveListener(selectable.OnBCISelected);
        binder.onBCISelected.AddListener(selectable.OnBCISelected);
    }
}
