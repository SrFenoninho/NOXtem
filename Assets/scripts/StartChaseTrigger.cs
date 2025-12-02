using UnityEngine;

public class StartChaseTrigger : MonoBehaviour
{
    public MonsterChaseAI monster;   // Again, calling it "AI" is saying a lot, but whatever :)

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            monster.StartChasing();
            gameObject.SetActive(false); // make the trigger inactive after activation for obvious reasons
        }
    }
}
