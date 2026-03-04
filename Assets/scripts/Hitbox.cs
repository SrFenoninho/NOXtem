using UnityEngine;
using System.Collections.Generic;

public class Hitbox : MonoBehaviour
{
    // ---------------------------------------------
    //  ESTADO PRIVADO
    // ---------------------------------------------
    private float currentDamage;
    private float currentKnockback;
    private float currentStunDuration;
    private string targetTag;
    private Collider myCollider;
    private List<GameObject> enemiesHit = new List<GameObject>(); // evitar acertar no mesmo inimigo duas vezes por ataque
    private PlayerComboSYS comboSystem;

    // ---------------------------------------------
    //  UNITY
    // ---------------------------------------------
    void Awake()
    {
        myCollider = GetComponent<Collider>();
        myCollider.enabled = false;     // começa desativado — só ativo durante janela de ataque
        myCollider.isTrigger = true;
        comboSystem = FindFirstObjectByType<PlayerComboSYS>();
    }

    // ---------------------------------------------
    //  ATIVAR / DESATIVAR
    // ---------------------------------------------
    public void EnableHitbox(float damage, string tag, PlayerCombat player = null, float knockback = 0f, float stunDuration = 0f)
    {
        currentDamage = damage;
        currentKnockback = knockback;
        currentStunDuration = stunDuration;
        targetTag = tag;
        enemiesHit.Clear();             // limpar lista a cada novo ataque
        myCollider.enabled = true;
    }

    public void DisableHitbox()
    {
        myCollider.enabled = false;
    }

    // ---------------------------------------------
    //  DETEÇÃO DE COLISÃO
    // ---------------------------------------------
    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(targetTag)) return;

        // Ignorar inimigos já atingidos neste ataque
        if (enemiesHit.Contains(other.gameObject)) return;

        EnemyAI enemy = other.GetComponent<EnemyAI>();
        if (enemy != null)
        {
            enemy.TakeDamage(currentDamage, currentKnockback, currentStunDuration);
            enemiesHit.Add(other.gameObject);

            // Registar acerto no sistema de combos
            if (comboSystem != null)
                comboSystem.RegisterHit();
        }
    }
}
