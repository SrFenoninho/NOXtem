using UnityEngine;
using System.Collections;

public class BossState_Stunned : IBossState
{




    // ---------------------------------------------
    //  PRIVATE STATE
    // ---------------------------------------------
    private Coroutine routine;





    // ---------------------------------------------
    //  PUBLIC METHODS
    // ---------------------------------------------
    public void EnterState(BossController boss)
    {
        routine = boss.StartCoroutine(StunRoutine(boss));
    }

    public void UpdateState(BossController boss) {}

    public void ExitState(BossController boss)
    {
        if(routine != null) boss.StopCoroutine(routine);
    }





    // ---------------------------------------------
    //  PRIVATE METHODS
    // ---------------------------------------------
    private IEnumerator StunRoutine(BossController boss)
    {
        boss.movement.StopMovement();
        boss.combat.TriggerAnim("Hit");
        yield return new WaitForSeconds(0.6f);
        boss.TriggerPhase(boss.currentPhase);
    }
}
