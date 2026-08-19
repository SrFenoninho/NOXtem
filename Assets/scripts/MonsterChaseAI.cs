using UnityEngine;

public class MonsterChaseAI : MonoBehaviour
{





    // ---------------------------------------------
    //  INSPECTOR
    // ---------------------------------------------
    public Transform player;
    public float speed = 3f;
    public float attackDamage = 10f;
    public float attackInterval = 1f;





    // ---------------------------------------------
    //  PRIVATE STATE
    // ---------------------------------------------
    private bool chasing = false;
    private float nextAttack = 0f;
    private bool playerInZone = false;
    private PlayerHealth playerHealth;





    // ---------------------------------------------
    //  UNITY
    // ---------------------------------------------
    void Update()
    {
        if (!chasing) return;

        Vector3 direction = (player.position - transform.position).normalized;
        transform.position += direction * speed * Time.deltaTime;
        transform.LookAt(transform.position - (player.position - transform.position));

        if (playerInZone && Time.time >= nextAttack && playerHealth != null)
        {
            playerHealth.TakeDamage(attackDamage, transform.position);
            nextAttack = Time.time + attackInterval;
        }
    }





    // ---------------------------------------------
    //  PUBLIC METHODS
    // ---------------------------------------------
    public void StartChasing()
    {
        chasing = true;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerHealth = other.GetComponent<PlayerHealth>();
            if (playerHealth != null)
                playerInZone = true;
        }
    }

    void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player") && playerHealth == null)
        {
            playerHealth = other.GetComponent<PlayerHealth>();
            playerInZone = true;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInZone = false;
            playerHealth = null;
        }
    }
}
