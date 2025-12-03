using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    public float maxHealth = 100f;
    public float speed = 3f;
    public float attackDamage = 10f;
    public float attackInterval = 1f;

    private float currentHealth;
    private Transform player;
    private PlayerHealth playerHealth;
    private float nextAttack = 0f;

    public System.Action OnDeath;

    void Start()
    {
        currentHealth = maxHealth;
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) player = playerObj.transform;
    }

    void Update()
    {
        if (player == null) return;
        Vector3 direction = (player.position - transform.position).normalized;
        transform.position += direction * speed * Time.deltaTime;
        transform.LookAt(player);
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
                playerHealth.TakeDamage(attackDamage);
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
        currentHealth -= damage;
        Debug.Log("Inimigo tomou " + damage + " de dano. Vida restante: " + currentHealth);

        if (currentHealth <= 0)
        {
            OnDeath?.Invoke();
            Destroy(gameObject);
        }
    }

}