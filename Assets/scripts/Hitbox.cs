using UnityEngine;
using System.Collections.Generic;

public class Hitbox : MonoBehaviour
{





    // ---------------------------------------------
    //  PRIVATE STATE
    // ---------------------------------------------
    private float currentDamage;
    private float currentKnockback;
    private float currentStunDuration;
    private string targetTag;
    private Collider myCollider;
    private List<GameObject> enemiesHit = new List<GameObject>();
    private PlayerComboSYS comboSystem;





    // ---------------------------------------------
    //  UNITY
    // ---------------------------------------------
    void Awake()
    {
        myCollider = GetComponent<Collider>();
        myCollider.enabled = false;
        myCollider.isTrigger = true;
        comboSystem = FindFirstObjectByType<PlayerComboSYS>();
    }





    // ---------------------------------------------
    //  PUBLIC METHODS
    // ---------------------------------------------
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
        if (other.CompareTag("Player")) return;
        if (enemiesHit.Contains(other.gameObject)) return;

        EnemyAI enemy = other.GetComponentInParent<EnemyAI>();
        FinalBossAI boss = other.GetComponentInParent<FinalBossAI>();
        BossHealth bossHealthNew = other.GetComponentInParent<BossHealth>();

        if (enemy != null)
        {
            enemy.TakeDamage(currentDamage, currentKnockback, currentStunDuration);
            enemiesHit.Add(other.gameObject);
            if (comboSystem != null) comboSystem.RegisterHit();
        }
        else if (bossHealthNew != null)
        {
            bossHealthNew.TakeDamage(currentDamage);
            enemiesHit.Add(other.gameObject);
            if (comboSystem != null) comboSystem.RegisterHit();
        }
        else if (boss != null)
        {
            boss.TakeDamage(currentDamage);
            enemiesHit.Add(other.gameObject);
            if (comboSystem != null) comboSystem.RegisterHit();
        }
        else if (other.CompareTag(targetTag))
        {
            enemiesHit.Add(other.gameObject);
        }
    }
}
