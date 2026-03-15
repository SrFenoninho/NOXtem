using System.Collections.Generic;
using UnityEngine;

public enum GameState
{
    Gameplay,       // estado normal — tudo permitido
    RadialMenu,     // menu radial aberto
    Dialogue,       // diálogo a decorrer
    Minigame,       // minijogo ativo
    Cutscene,       // cutscene
    Inventory,      // inventário aberto
    Paused          // pausado
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
    //  GESTÃO DE ESTADOS
    // ---------------------------------------------
    // Entra num novo estado (empilha por cima)
    public void PushState(GameState state)
    {
        stateStack.Push(state);
        Debug.Log($"[GameState] Push: {state} | Stack: {stateStack.Count}");
    }

    // Sai do estado atual (volta ao anterior)
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
    // Verifica se um estado específico está ativo
    public bool Is(GameState state) => CurrentState == state;

    // O menu radial só pode abrir se estivermos em Gameplay
    public bool CanOpenRadialMenu() => CurrentState == GameState.Gameplay;

    // O jogador pode mover-se?
    public bool CanMove() => CurrentState == GameState.Gameplay;
}