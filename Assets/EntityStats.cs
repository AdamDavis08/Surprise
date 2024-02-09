using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityStats : MonoBehaviour
{
    public float health = 100f;
    public float damageCooldown = 1f;
    private float lastHitTime = 0f;

    public void TakeDamage(float damage)
    {
        if (Time.time < lastHitTime + damageCooldown) return;

        lastHitTime = Time.time;
        health -= damage;

        if (health <= 0)
        {
            transform.TryGetComponent<IEntity>(out IEntity entity);
            entity.Die();
        }
    }
}
