using UnityEngine;
using System.Collections.Generic;

public class CutsceneDirector : MonoBehaviour
{




    // ---------------------------------------------
    //  INSPECTOR
    // ---------------------------------------------
    [Header("Atores")]

    public CutsceneActor[] elenco;

    [Header("C�maras")]
    public Camera camaraDoFilme;
    public Camera camaraDoJogador;

    [Header("Configura��es da Cena")]
    public float tempoTotalDaCutscene = 15f;





    // ---------------------------------------------
    //  PUBLIC METHODS
    // ---------------------------------------------
    public void DispararCena()
    {
        GameStateManager.Instance?.PushState(GameState.Cutscene);

        if (camaraDoJogador != null) camaraDoJogador.enabled = false;
        if (camaraDoFilme != null) camaraDoFilme.gameObject.SetActive(true);

        foreach (CutsceneActor ator in elenco)
        {
            if (ator != null)
                ator.IniciarAcao();
        }

        Invoke("TerminarCena", tempoTotalDaCutscene);
    }





    // ---------------------------------------------
    //  PRIVATE METHODS
    // ---------------------------------------------
    private void TerminarCena()
    {
        if (camaraDoFilme != null) camaraDoFilme.gameObject.SetActive(false);
        if (camaraDoJogador != null) camaraDoJogador.enabled = true;

        GameStateManager.Instance?.PopState();
    }
}
