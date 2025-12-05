using UnityEngine;
using System.Collections.Generic;

public class Hitbox : MonoBehaviour
{
    private float currentDamage;
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

    public void EnableHitbox(float damage, string tag, PlayerCombat player = null)
    {
        currentDamage = damage;
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
                enemy.TakeDamage(currentDamage);
                enemiesHit.Add(other.gameObject);

                if (comboSystem != null)
                    comboSystem.RegisterHit();
            }
        }
    }
}