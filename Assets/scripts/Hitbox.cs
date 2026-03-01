using UnityEngine;
using System.Collections.Generic;

public class Hitbox : MonoBehaviour
{
    private float currentDamage;
    private float currentKnockback;
    private float currentStunDuration;
    private string targetTag;
    private Collider myCollider;
    private List<GameObject> enemiesHit = new List<GameObject>();
    private PlayerComboSYS comboSystem;

    void Awake()
    {
        myCollider = GetComponent<Collider>();
        myCollider.enabled = false;
        myCollider.isTrigger = true;
        comboSystem = FindFirstObjectByType<PlayerComboSYS>();
    }

    public void EnableHitbox(float damage, string tag, PlayerCombat player = null, float knockback = 0f, float stunDuration = 0f)
    {
        currentDamage = damage;
        currentKnockback = knockback;
        currentStunDuration = stunDuration;
        targetTag = tag;
        enemiesHit.Clear();
        myCollider.enabled = true;
    }

    public void DisableHitbox()
    {
        myCollider.enabled = false;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(targetTag))
        {
            if (enemiesHit.Contains(other.gameObject)) return;

            EnemyAI enemy = other.GetComponent<EnemyAI>();
            if (enemy != null)
            {
                enemy.TakeDamage(currentDamage, currentKnockback, currentStunDuration);
                enemiesHit.Add(other.gameObject);

                if (comboSystem != null)
                    comboSystem.RegisterHit();
            }
        }
    }
}