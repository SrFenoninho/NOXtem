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
    [HideInInspector] public bool isDefending = false;
    public float damageCooldown = 1f;   // tempo de invencibilidade entre danos consecutivos
    private float nextDamage = 0f;

    [Header("Knockback Settings")]
    public float knockbackForce = 5f;
    public float knockbackDuration = 0.3f;

    // ---------------------------------------------
    //  ESTADO PRIVADO
    // ---------------------------------------------
    private float currentKnockbackForce;
    private bool isKnockedBack = false;
    private float knockbackEndTime;
    private Vector3 knockbackDirection;
    private CharacterController characterController;
    private Animator anim;
    private PlayerCombat playerCombat;

    // ---------------------------------------------
    //  UNITY
    // ---------------------------------------------
    void Start()
    {
        currentHealth = maxHealth;
        characterController = GetComponent<CharacterController>();
        anim = GetComponentInChildren<Animator>();
        playerCombat = GetComponent<PlayerCombat>();
    }

    void Update()
    {
        // Aplicar knockback horizontal enquanto ativo
        if (isKnockedBack && characterController != null)
        {
            characterController.Move(knockbackDirection * currentKnockbackForce * Time.deltaTime);
            if (Time.time >= knockbackEndTime)
                isKnockedBack = false;
        }
    }

    // ---------------------------------------------
    //  DANO
    // ---------------------------------------------
    public void TakeDamage(float damageAmount, Vector3 damageSourcePosition)
    {
        // Ignorar dano durante o periodo de invencibilidade
        if (Time.time < nextDamage) return;

        currentHealth -= damageAmount;
        nextDamage = Time.time + damageCooldown;
        Debug.Log("Player took " + damageAmount + " damage. Health: " + currentHealth);

        currentKnockbackForce = knockbackForce;

        // Calcular direcao do knockback (sempre horizontal)
        knockbackDirection = (transform.position - damageSourcePosition).normalized;
        knockbackDirection.y = 0;
        isKnockedBack = true;
        knockbackEndTime = Time.time + knockbackDuration;

        // Disparar animação de dano e cancelar ataque atual
        if (anim != null) anim.SetTrigger("takeDamage");
        if (playerCombat != null) playerCombat.CancelAttack();

        if (currentHealth <= 0)
            Die();
    }

    public void TakeAreaDamageWithKnockback(float damageAmount, Vector3 damageSourcePosition, float customForce, float customDuration)
    {
        if (Time.time < nextDamage) return;

        currentHealth -= damageAmount;
        nextDamage = Time.time + damageCooldown;

        currentKnockbackForce = customForce;
        
        knockbackDirection = (transform.position - damageSourcePosition).normalized;
        knockbackDirection.y = 0;
        isKnockedBack = true;
        knockbackEndTime = Time.time + customDuration;

        if (anim != null) anim.SetTrigger("takeDamage");
        if (playerCombat != null) playerCombat.CancelAttack();

        if (currentHealth <= 0)
            Die();
    }

    // ---------------------------------------------
    //  MORTE
    // ---------------------------------------------
    void Die()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    // ---------------------------------------------
    //  HUD DE VIDA
    // ---------------------------------------------
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
