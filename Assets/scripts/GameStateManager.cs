using System.Collections.Generic;
using UnityEngine;

public enum GameState
{
    Gameplay,       // estado normal - tudo permitido
    RadialMenu,     // menu radial aberto
    Dialogue,       // dialogo a decorrer
    Minigame,       // minijogo ativo
    Cutscene,       // cutscene
    Inventory,      // inventario aberto
    Paused          // pausado
}

public class GameStateManager : MonoBehaviour
{
    public static GameStateManager Instance { get; private set; }

    private Stack<GameState> stateStack = new Stack<GameState>();

    public GameState CurrentState => stateStack.Count > 0 ? stateStack.Peek() : GameState.Gameplay;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        stateStack.Push(GameState.Gameplay);
    }

    // entra num novo estado (empilha por cima)
    public void PushState(GameState state)
    {
        stateStack.Push(state);
        Debug.Log($"[GameState] Push: {state} | Stack: {stateStack.Count}");
    }

    // sai do estado atual (volta ao anterior)
    public void PopState()
    {
        if (stateStack.Count > 1)
        {
            GameState removed = stateStack.Pop();
            Debug.Log($"[GameState] Pop: {removed} | Agora: {CurrentState}");
        }
    }

    // verifica se um estado especifico esta ativo
    public bool Is(GameState state) => CurrentState == state;

    // o menu radial so pode abrir se estivermos em Gameplay
    public bool CanOpenRadialMenu() => CurrentState == GameState.Gameplay;

    // o jogador pode mover-se?
    public bool CanMove() => CurrentState == GameState.Gameplay;
}