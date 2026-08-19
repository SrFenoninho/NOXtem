using UnityEngine;

public class StartChaseTrigger : MonoBehaviour
{





    // ---------------------------------------------
    //  INSPECTOR
    // ---------------------------------------------
    public MonsterChaseAI monster;





    // ---------------------------------------------
    //  UNITY
    // ---------------------------------------------
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            monster.StartChasing();
            gameObject.SetActive(false);
        }
    }
}
