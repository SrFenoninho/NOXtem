//Calling it "AI" is saying a lot, but whatever :P
using UnityEngine;

public class MonsterChaseAI : MonoBehaviour
{
    public Transform player;
    public float speed = 3f;
    public float attackDamage = 10f;
    public float attackInterval = 1f;

    private bool chasing = false;
    private float nextAttack = 0f;
    private bool playerInZone = false;
    private PlayerHealth playerHealth;

    void Update()
    {
        if (!chasing) return;

        Vector3 direction = (player.position - transform.position).normalized;
        transform.position += direction * speed * Time.deltaTime; // Follow the player
        transform.LookAt(player);

        if (playerInZone && Time.time >= nextAttack && playerHealth != null)
        {
            playerHealth.TakeDamage(attackDamage);
            nextAttack = Time.time + attackInterval;
        }
    }

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
            {
                playerInZone = true;
                Debug.Log("Player entered damage zone!");
            }
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
            Debug.Log("Player left damage zone!");
        }
    }
}