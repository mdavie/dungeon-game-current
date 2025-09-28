using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterHealth : MonoBehaviour
{
    [SerializeField] int maxHp = 5;
    int hp;

    // C# 4.0-friendly:
    public int CurrentHP { get { return hp; } }

    void Awake() { hp = maxHp; }

    public void TakeDamage(int amount)
    {
        hp = Mathf.Max(0, hp - amount);
        if (hp <= 0) Die();
    }

    void Die()
    {
        Destroy(gameObject);
    }
}