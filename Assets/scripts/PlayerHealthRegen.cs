using UnityEngine;

public class PlayerHealthRegen : MonoBehaviour
{
    // ---------------------------------------------
    //  INSPETOR
    // ---------------------------------------------
    [Header("Regeneration Settings")]
    public float regenAmount = 2f;      // vida recuperada por intervalo
    public float regenInterval = 5f;    // segundos entre cada regeneração

    // ---------------------------------------------
    //  ESTADO PRIVADO
    // ---------------------------------------------
    private PlayerHealth playerHealth;
    private float nextRegen = 0f;

    // ---------------------------------------------
    //  UNITY
    // ---------------------------------------------
    void Start()
    {
        playerHealth = GetComponent<PlayerHealth>();
    }

    void Update()
    {
        if (Time.time >= nextRegen)
        {
            Regenerate();
            nextRegen = Time.time + regenInterval;
        }
    }

    // ---------------------------------------------
    //  REGENERAÇÃO
    // ---------------------------------------------
    void Regenerate()
    {
        if (playerHealth.currentHealth < playerHealth.maxHealth)
        {
            playerHealth.currentHealth += regenAmount;

            // Garantir que não ultrapassa o máximo
            if (playerHealth.currentHealth > playerHealth.maxHealth)
                playerHealth.currentHealth = playerHealth.maxHealth;
        }
    }
}
