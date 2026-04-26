using System.Collections.Generic;
using UnityEngine;

public enum GameState
{
    Gameplay,
    RadialMenu,
    Dialogue,
    Minigame,
    Cutscene,
    Inventory,
    Paused
}

public class GameStateManager : MonoBehaviour
{
    // ---------------------------------------------
    //  SINGLETON
    // ---------------------------------------------
    public static GameStateManager Instance { get; private set; }

    // ---------------------------------------------
    //  ESTADO PRIVADO
    // ---------------------------------------------
    private Stack<GameState> stateStack = new Stack<GameState>();

    public GameState CurrentState => stateStack.Count > 0 ? stateStack.Peek() : GameState.Gameplay;

    // ---------------------------------------------
    //  UNITY
    // ---------------------------------------------
    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        stateStack.Push(GameState.Gameplay);
    }

    // ---------------------------------------------
    //  GESTAO DE ESTADOS
    // ---------------------------------------------
    public void PushState(GameState state)
    {
        stateStack.Push(state);
        Debug.Log($"[GameState] Push: {state} | Stack: {stateStack.Count}");
    }

    public void PopState()
    {
        if (stateStack.Count > 1)
        {
            GameState removed = stateStack.Pop();
            Debug.Log($"[GameState] Pop: {removed} | Agora: {CurrentState}");
        }
    }

    // ---------------------------------------------
    //  CONSULTAS
    // ---------------------------------------------
    public bool Is(GameState state) => CurrentState == state;

    public bool CanOpenRadialMenu() => CurrentState == GameState.Gameplay;

    // Inventario so abre em Gameplay puro
    public bool CanOpenInventory() => CurrentState == GameState.Gameplay;

    public bool CanMove() => CurrentState == GameState.Gameplay;
}
