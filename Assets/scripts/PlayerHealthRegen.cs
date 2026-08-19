using UnityEngine;

public class PlayerHealthRegen : MonoBehaviour
{




    // ---------------------------------------------
    //  INSPECTOR
    // ---------------------------------------------
    [Header("Regeneration Settings")]
    public float regenAmount = 2f;
    public float regenInterval = 5f;





    // ---------------------------------------------
    //  PRIVATE STATE
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
    //  PRIVATE METHODS
    // ---------------------------------------------
    void Regenerate()
    {
        if (playerHealth.currentHealth < playerHealth.maxHealth)
        {
            playerHealth.currentHealth += regenAmount;

            if (playerHealth.currentHealth > playerHealth.maxHealth)
                playerHealth.currentHealth = playerHealth.maxHealth;
        }
    }
}
