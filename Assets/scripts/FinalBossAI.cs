using UnityEngine;

[RequireComponent(typeof(BossController))]
public class FinalBossAI : MonoBehaviour
{
    // CÓDIGO DE COMPATIBILIDADE (LEGACY WRAPPER)
    // Mantemos os métodos antigos vivos aqui para que guiões como o Hitbox.cs 
    // e o BossPillar.cs não deem erros de compilação.
    // Este guião apenas reencaminha as ordens para o novo BossController modular.

    private BossController bossController;

    void Awake()
    {
        bossController = GetComponent<BossController>();
    }

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

    // A assinatura original recebia Transform (playerTransform)
    public void ExecuteFinalCutscene(Transform playerTransform)
    {
        if (bossController != null)
        {
            bossController.ExecuteFinalCutscene();
        }
    }
}
