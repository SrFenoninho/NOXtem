using UnityEngine;

public class MonsterChaseAI : MonoBehaviour
{
    // ---------------------------------------------
    //  INSPETOR
    // ---------------------------------------------
    public Transform player;
    public float speed = 3f;
    public float attackDamage = 10f;
    public float attackInterval = 1f;

    // ---------------------------------------------
    //  ESTADO PRIVADO
    // ---------------------------------------------
    private bool chasing = false;           // só se move depois de ser ativado
    private float nextAttack = 0f;
    private bool playerInZone = false;      // verdadeiro quando o jogador está dentro do trigger de ataque
    private PlayerHealth playerHealth;

    // ---------------------------------------------
    //  UNITY
    // ---------------------------------------------
    void Update()
    {
        if (!chasing) return;

        // Mover em direção ao jogador
        Vector3 direction = (player.position - transform.position).normalized;
        transform.position += direction * speed * Time.deltaTime;
        transform.LookAt(transform.position - (player.position - transform.position));

        // Atacar se o jogador estiver na zona de dano
        if (playerInZone && Time.time >= nextAttack && playerHealth != null)
        {
            playerHealth.TakeDamage(attackDamage, transform.position);
            nextAttack = Time.time + attackInterval;
        }
    }

    // ---------------------------------------------
    //  ATIVAÇÃO
    // ---------------------------------------------
    // Chamado pelo StartChaseTrigger quando o jogador entra na zona
    public void StartChasing()
    {
        chasing = true;
    }

    // ---------------------------------------------
    //  ZONA DE ATAQUE
    // ---------------------------------------------
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
        // Fallback caso o jogador entre antes da referência estar pronta
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
            Debug.Log("Jogador saiu da zona de dano!");
        }
    }
}
