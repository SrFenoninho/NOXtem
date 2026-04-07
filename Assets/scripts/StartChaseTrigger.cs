using UnityEngine;

public class StartChaseTrigger : MonoBehaviour
{
    // ---------------------------------------------
    //  INSPETOR
    // ---------------------------------------------
    public MonsterChaseAI monster;

    // ---------------------------------------------
    //  TRIGGER
    // ---------------------------------------------
    // Ativa a perseguicao quando o jogador entra na zona
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            monster.StartChasing();
            gameObject.SetActive(false); // desativar o trigger apos uso - so deve disparar uma vez
        }
    }
}
