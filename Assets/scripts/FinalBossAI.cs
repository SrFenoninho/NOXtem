using UnityEngine;

[RequireComponent(typeof(BossController))]
public class FinalBossAI : MonoBehaviour
{





    // ---------------------------------------------
    //  PRIVATE STATE
    // ---------------------------------------------
    private BossController bossController;





    // ---------------------------------------------
    //  UNITY
    // ---------------------------------------------
    void Awake()
    {
        bossController = GetComponent<BossController>();
    }





    // ---------------------------------------------
    //  PUBLIC METHODS
    // ---------------------------------------------
    public void TakeDamage(float amount)
    {
        if (bossController != null && bossController.health != null)
        {
            bossController.health.TakeDamage(amount);
        }
    }

    public bool IsReadyForExecution()
    {
        if (bossController != null)
        {
            return bossController.currentPhase == BossController.BossPhase.ReadyToDie;
        }
        return false;
    }

    public void ExecuteFinalCutscene(Transform playerTransform)
    {
        if (bossController != null)
        {
            bossController.ExecuteFinalCutscene();
        }
    }
}
