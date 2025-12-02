using UnityEngine;
using UnityEngine.SceneManagement;
public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    public float maxHealth = 100f;
    public float currentHealth;
    [Header("Damage Settings")]
    public float damageCooldown = 1f; // this is the cooldown time between damages
    private float nextDamage = 0f;
    void Start()
    {
        currentHealth = maxHealth;
    }
    public void TakeDamage(float damageAmount)
    {
        if (Time.time < nextDamage)
            return;
        currentHealth -= damageAmount;
        nextDamage = Time.time + damageCooldown;
        Debug.Log("Player took " + damageAmount + " damage. Health: " + currentHealth);
        if (currentHealth <= 0)
        {
            Die();
        }
    }
    void Die()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
    void OnGUI() // is a simple way to display health on screen, but actually, I like it :D
    {
        float boxWidth = 200;
        float boxHeight = 25;
        float margin = 10;

        float xPos = Screen.width - boxWidth - margin;
        float yPos = Screen.height - boxHeight - margin;

        GUI.Box(new Rect(xPos, yPos, boxWidth, boxHeight), "Health: " + currentHealth.ToString("F0") + " / " + maxHealth);
    }
}