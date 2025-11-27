//Calling it "AI" is saying a lot, but whatever :P
using UnityEngine;

public class MonsterChaseAI : MonoBehaviour
{
    public Transform player;
    public float speed = 3f;
    private bool chasing = false;
    void Update()
    {
        if (!chasing) return; 

        Vector3 direction = (player.position - transform.position).normalized;
        transform.position += direction * speed * Time.deltaTime; // Follow the player
        transform.LookAt(player);
    }

    public void StartChasing()
    {
        chasing = true;
    }
}
