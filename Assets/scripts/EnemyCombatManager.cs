using UnityEngine;
using System.Collections.Generic;

public static class EnemyCombatManager
{




    // ---------------------------------------------
    //  PRIVATE STATE
    // ---------------------------------------------
    private static List<EnemyAI> registeredEnemies = new List<EnemyAI>();
    private static List<EnemyAI> currentAttackers = new List<EnemyAI>();




    // ---------------------------------------------
    //  INSPECTOR
    // ---------------------------------------------
    public static int maxSimultaneousAttackers = 2;
    public static float groupCooldownMin = 0.5f;
    public static float groupCooldownMax = 1.2f;

    private static float nextGroupTime = 0f;





    // ---------------------------------------------
    //  PUBLIC METHODS
    // ---------------------------------------------
    public static void Register(EnemyAI enemy)
    {
        if (!registeredEnemies.Contains(enemy))
            registeredEnemies.Add(enemy);
    }

    public static void Unregister(EnemyAI enemy)
    {
        registeredEnemies.Remove(enemy);
        currentAttackers.Remove(enemy);
    }

    public static bool RequestAttackToken(EnemyAI requester)
    {
        if (currentAttackers.Contains(requester)) return false;
        if (currentAttackers.Count >= maxSimultaneousAttackers) return false;
        if (Time.time < nextGroupTime) return false;

        currentAttackers.Add(requester);
        return true;
    }

    public static void ReleaseToken(EnemyAI attacker)
    {
        if (!currentAttackers.Remove(attacker)) return;

        if (currentAttackers.Count == 0)
            nextGroupTime = Time.time + Random.Range(groupCooldownMin, groupCooldownMax);
    }

    public static bool HasToken(EnemyAI enemy) => currentAttackers.Contains(enemy);
    public static int AttackerCount => currentAttackers.Count;
    public static int EnemyCount => registeredEnemies.Count;

    public static void Clear()
    {
        registeredEnemies.RemoveAll(e => e == null);
        registeredEnemies.Clear();
        currentAttackers.Clear();
        nextGroupTime = 0f;
    }
}
