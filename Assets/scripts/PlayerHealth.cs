using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerHealth : MonoBehaviour
{
    // ---------------------------------------------
    //  INSPETOR
    // ---------------------------------------------
    [Header("Health Settings")]
    public float maxHealth = 100f;
    public float currentHealth;

    [Header("Damage Settings")]
    public float damageCooldown = 1f;   // tempo de invencibilidade entre danos consecutivos
    private float nextDamage = 0f;

    [Header("Knockback Settings")]
    public float knockbackForce = 5f;
    public float knockbackDuration = 0.3f;

    // ---------------------------------------------
    //  ESTADO PRIVADO
    // ---------------------------------------------
    private bool isKnockedBack = false;
    private float knockbackEndTime;
    private Vector3 knockbackDirection;
    private CharacterController characterController;

    // ---------------------------------------------
    //  UNITY
    // ---------------------------------------------
    void Start()
    {
        currentHealth = maxHealth;
        characterController = GetComponent<CharacterController>();
    }

    void Update()
    {
        // Aplicar knockback horizontal enquanto ativo
        if (isKnockedBack && characterController != null)
        {
            characterController.Move(knockbackDirection * knockbackForce * Time.deltaTime);
            if (Time.time >= knockbackEndTime)
                isKnockedBack = false;
        }
    }

    // ---------------------------------------------
    //  DANO
    // ---------------------------------------------
    public void TakeDamage(float damageAmount, Vector3 damageSourcePosition)
    {
        // Ignorar dano durante o período de invencibilidade
        if (Time.time < nextDamage) return;

        currentHealth -= damageAmount;
        nextDamage = Time.time + damageCooldown;
        Debug.Log("Player took " + damageAmount + " damage. Health: " + currentHealth);

        // Calcular direção do knockback (sempre horizontal)
        knockbackDirection = (transform.position - damageSourcePosition).normalized;
        knockbackDirection.y = 0;
        isKnockedBack = true;
        knockbackEndTime = Time.time + knockbackDuration;

        if (currentHealth <= 0)
            Die();
    }

    // ---------------------------------------------
    //  MORTE
    // ---------------------------------------------
    void Die()
    {
        // Recarregar a cena atual ao morrer
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    // ---------------------------------------------
    //  HUD DE VIDA
    // ---------------------------------------------
    // Desenha a barra de vida no ecrã com OnGUI
    void OnGUI()
    {
        float boxWidth = 200;
        float boxHeight = 25;
        float margin = 10;

        float xPos = Screen.width - boxWidth - margin;
        float yPos = Screen.height - boxHeight - margin;

        GUI.Box(new Rect(xPos, yPos, boxWidth, boxHeight),
            "Health: " + currentHealth.ToString("F0") + " / " + maxHealth);
    }
}
