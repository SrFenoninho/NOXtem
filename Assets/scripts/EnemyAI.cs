using UnityEngine;
public class EnemyAI : MonoBehaviour
{
    public float maxHealth = 100f;
    public float speed = 3f;
    public float attackDamage = 10f;
    public float attackInterval = 1f;

    [Header("Knockback")]
    public float knockbackForce = 5f;
    public float knockbackDuration = 0.2f;

    private float currentHealth;
    private Transform player;
    private PlayerHealth playerHealth;
    private float nextAttack = 0f;

    public System.Action OnDeath;
    public bool isOriginal = false;

    private bool isKnockedBack = false;
    private float knockbackEndTime;
    private Vector3 knockbackDirection;

    void Start()
    {
        currentHealth = maxHealth;
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) player = playerObj.transform;
    }
    void Update()
    {
        if (isOriginal) return;
        if (player == null) return;

        if (isKnockedBack)
        {
            transform.position += knockbackDirection * knockbackForce * Time.deltaTime;
            if (Time.time >= knockbackEndTime)
            {
                isKnockedBack = false;
            }
            return;
        }

        Vector3 direction = (player.position - transform.position).normalized;
        transform.position += direction * speed * Time.deltaTime;
        transform.LookAt(transform.position - (player.position - transform.position));
    }
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerHealth = other.GetComponent<PlayerHealth>();
        }
    }
    void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player") && Time.time >= nextAttack)
        {
            if (playerHealth == null) playerHealth = other.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(attackDamage, transform.position);
                nextAttack = Time.time + attackInterval;
            }
        }
    }
    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerHealth = null;
        }
    }
    public void TakeDamage(float damage)
    {
        currentHealth = currentHealth - damage;
        Debug.Log("Enemy took " + damage + " damage. Remaining health: " + currentHealth);

        if (player != null)
        {
            knockbackDirection = (transform.position - player.position).normalized;
            isKnockedBack = true;
            knockbackEndTime = Time.time + knockbackDuration;
        }

        if (currentHealth <= 0)
        {
            OnDeath?.Invoke();
            Destroy(gameObject);
        }
    }
}