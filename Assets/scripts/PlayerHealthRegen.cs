using UnityEngine;

public class PlayerHealthRegen : MonoBehaviour
{
    [Header("Regeneration Settings")]
    public float regenAmount = 2f;
    public float regenInterval = 5f;

    private PlayerHealth playerHealth;
    private float nextRegen = 0f;

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
    void Regenerate()
    {
        if (playerHealth.currentHealth < playerHealth.maxHealth)
        {
            playerHealth.currentHealth += regenAmount;

            if (playerHealth.currentHealth > playerHealth.maxHealth)
            {
                playerHealth.currentHealth = playerHealth.maxHealth;
            }
        }
    }
}