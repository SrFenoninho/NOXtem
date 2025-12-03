using UnityEngine;

public class Hitbox : MonoBehaviour
{
    private float damage;
    private string Enemy;
    private bool active = false;

    public void EnableHitbox(float dmg, string tag)
    {
        damage = dmg;
        Enemy = tag;
        active = true;
    }

    public void DisableHitbox()
    {
        active = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!active) return;
        if (!other.CompareTag(Enemy)) return;

        EnemyAI enemy = other.GetComponentInParent<EnemyAI>();
        if (enemy != null)
        {
            enemy.TakeDamage(damage);
        }
    }
}
