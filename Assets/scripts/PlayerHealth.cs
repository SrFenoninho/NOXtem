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
    [Header("Knockback Settings")]
    public float knockbackForce = 5f;
    public float knockbackDuration = 0.3f;

    private bool isKnockedBack = false;
    private float knockbackEndTime;
    private Vector3 knockbackDirection;
    private CharacterController characterController;
    void Start()
    {
        currentHealth = maxHealth;
        characterController = GetComponent<CharacterController>();
    }

    void Update()
    {
        if (isKnockedBack && characterController != null)
        {
            characterController.Move(knockbackDirection * knockbackForce * Time.deltaTime);
            if (Time.time >= knockbackEndTime)
            {
                isKnockedBack = false;
            }
        }
    }
    public void TakeDamage(float damageAmount, Vector3 damageSourcePosition)
    {
        if (Time.time < nextDamage)
            return;
        currentHealth -= damageAmount;
        nextDamage = Time.time + damageCooldown;
        Debug.Log("Player took " + damageAmount + " damage. Health: " + currentHealth);

        knockbackDirection = (transform.position - damageSourcePosition).normalized;
        knockbackDirection.y = 0; // keep knockback horizontal
        isKnockedBack = true;
        knockbackEndTime = Time.time + knockbackDuration;

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