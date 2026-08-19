using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerHealth : MonoBehaviour
{




    // ---------------------------------------------
    //  INSPECTOR
    // ---------------------------------------------
    [Header("Health Settings")]
    public float maxHealth = 100f;

    public float currentHealth;

    [Header("Damage Settings")]
    [HideInInspector] public bool isDefending = false;
    public float damageCooldown = 1f;




    // ---------------------------------------------
    //  PRIVATE STATE
    // ---------------------------------------------
    private float nextDamage = 0f;

    [Header("Knockback Settings")]
    public float knockbackForce = 5f;
    public float knockbackDuration = 0.3f;

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
            }
        }

        if (SaveSystem.carregarSaveAoIniciar)
        {
            SaveSystem.AplicarSaveAoPlayer(gameObject);
        }
    }

    void Update()
    {
        if (isKnockedBack && characterController != null)
        {
            characterController.Move(knockbackDirection * currentKnockbackForce * Time.deltaTime);
            if (Time.time >= knockbackEndTime)
                isKnockedBack = false;
        }
    }





    // ---------------------------------------------
    //  PUBLIC METHODS
    // ---------------------------------------------
    public void TakeDamage(float damageAmount, Vector3 damageSourcePosition)
    {
        if (isDefending)
        {
            Vector3 dirToSource = (damageSourcePosition - transform.position).normalized;
            dirToSource.y = 0;
            float angle = Vector3.Angle(transform.forward, dirToSource);

            if (angle <= 75f)
            {
                return;
            }
        }

        if (Time.time < nextDamage) return;

        currentHealth -= damageAmount;
        nextDamage = Time.time + damageCooldown;

        currentKnockbackForce = knockbackForce;

        knockbackDirection = (transform.position - damageSourcePosition).normalized;
        knockbackDirection.y = 0;
        isKnockedBack = true;
        knockbackEndTime = Time.time + knockbackDuration;

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
    //  PRIVATE METHODS
    // ---------------------------------------------
    void Die()
    {
        LoadingManager.Carregar(SceneManager.GetActiveScene().buildIndex);
    }

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
