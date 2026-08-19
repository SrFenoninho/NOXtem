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
    //  INSPECTOR
    // ---------------------------------------------
    public static GameStateManager Instance { get; private set; }




    // ---------------------------------------------
    //  PRIVATE STATE
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
    //  PUBLIC METHODS
    // ---------------------------------------------
    public void PushState(GameState state)
    {
        stateStack.Push(state);
    }

    public void PopState()
    {
        if (stateStack.Count > 1)
        {
            GameState removed = stateStack.Pop();
        }
    }

    public bool Is(GameState state) => CurrentState == state;

    public bool CanOpenRadialMenu() => CurrentState == GameState.Gameplay;

    public bool CanOpenInventory() => CurrentState == GameState.Gameplay;

    public bool CanMove() => CurrentState == GameState.Gameplay;
}
