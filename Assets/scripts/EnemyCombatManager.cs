using UnityEngine;
using System.Collections.Generic;

// Apos todos terminarem, um breve tempo de espera antes do proximo grupo se formar.
public static class EnemyCombatManager
{
    // ---------------------------------------------
    //  ESTADO
    // ---------------------------------------------
    private static List<EnemyAI> registeredEnemies = new List<EnemyAI>();
    private static List<EnemyAI> currentAttackers = new List<EnemyAI>();

    public static int maxSimultaneousAttackers = 2;
    public static float groupCooldownMin = 0.5f;
    public static float groupCooldownMax = 1.2f;
    private static float nextGroupTime = 0f;

    // ---------------------------------------------
    //  REGISTO
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

    // ---------------------------------------------
    //  TOKENS DE ATAQUE
    // ---------------------------------------------
    // Conceder token se houver vagas abertas e o tempo de espera tiver passado
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

        // O tempo de espera so comeca quando o ultimo atacante do grupo termina
        if (currentAttackers.Count == 0)
            nextGroupTime = Time.time + Random.Range(groupCooldownMin, groupCooldownMax);
    }

    // ---------------------------------------------
    //  CONSULTAS
    // ---------------------------------------------
    public static bool HasToken(EnemyAI enemy) => currentAttackers.Contains(enemy);
    public static int AttackerCount => currentAttackers.Count;
    public static int EnemyCount => registeredEnemies.Count;

    // Reset total para garantir que transições de cena não quebrem a contagem na Build
    public static void Clear()
    {
        // Limpa nulls ou inimigos órfãos
        registeredEnemies.RemoveAll(e => e == null);
        registeredEnemies.Clear();
        currentAttackers.Clear();
        nextGroupTime = 0f;
    }
}