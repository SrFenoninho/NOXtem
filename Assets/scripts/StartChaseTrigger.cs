using UnityEngine;

public class StartChaseTrigger : MonoBehaviour
{
    // ---------------------------------------------
    //  INSPETOR
    // ---------------------------------------------
    // Outra vez, chamar-lhe "IA" já é dizer muito, mas pronto :)
    public MonsterChaseAI monster;

    // ---------------------------------------------
    //  TRIGGER
    // ---------------------------------------------
    // Ativa a perseguição quando o jogador entra na zona
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            monster.StartChasing();
            gameObject.SetActive(false); // desativar o trigger após uso — só deve disparar uma vez
        }
    }
}
