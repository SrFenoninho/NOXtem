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

        // Restaura sempre as chaves com que o jogador entrou neste nível (do nível anterior)
        if (PlayerPrefs.HasKey("PlayerKeys"))
        {
            PlayerKeys keys = GetComponent<PlayerKeys>();
            if (keys != null)
            {
                keys.ClearKeys();
                string chavesJuntas = PlayerPrefs.GetString("PlayerKeys", "");
                if (!string.IsNullOrEmpty(chavesJuntas))
                {
                    string[] arrayChaves = chavesJuntas.Split(',');
                    foreach (string key in arrayChaves)
                    {
                        keys.AddKey(key);
                    }
                }
                Debug.Log($"📂 [Player] Inventário restaurado no início da cena com {keys.GetKeys().Count} chaves.");
            }
        }

        // Se veio do menu via "Continuar", aplica as definições de cena e flags (a posição padrão é usada)
        if (SaveSystem.carregarSaveAoIniciar)
        {
            SaveSystem.AplicarSaveAoPlayer(gameObject);
        }
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
    public void TakeDamage(float damageAmount, Vector3 damageSourcePosition)
    {
        // Se estiver a defender, bloqueia apenas ataques frontais (ângulo até 75 graus para a esquerda ou direita)
        if (isDefending)
        {
            Vector3 dirToSource = (damageSourcePosition - transform.position).normalized;
            dirToSource.y = 0;
            float angle = Vector3.Angle(transform.forward, dirToSource);

            if (angle <= 75f)
            {
                Debug.Log("🛡️ [PlayerHealth] Dano de " + damageAmount + " bloqueado frontalmente pela Parede de Defesa!");
                return;
            }
        }

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
        float boxWidth = 50;
        float boxHeight = 25;
        float margin = 20;

        float xPos = Screen.width - boxWidth - margin;
        float yPos = Screen.height - boxHeight - margin;

        GUI.Box(new Rect(xPos, yPos, boxWidth, boxHeight),
            currentHealth.ToString("F0"));
    }
}
