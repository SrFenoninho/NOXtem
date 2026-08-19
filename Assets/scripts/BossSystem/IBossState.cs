using UnityEngine;

public interface IBossState
{



    // ---------------------------------------------
    //  PRIVATE METHODS
    // ---------------------------------------------
    void EnterState(BossController boss);
    void UpdateState(BossController boss);
    void ExitState(BossController boss);
}
