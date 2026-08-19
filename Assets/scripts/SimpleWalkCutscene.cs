using UnityEngine;
using System.Collections;

public class SimpleWalkCutscene : MonoBehaviour
{



    // ---------------------------------------------
    //  INSPECTOR
    // ---------------------------------------------
    [Header("Início Automático")]



    // ---------------------------------------------
    //  PRIVATE STATE
    // ---------------------------------------------
    [Tooltip("Liga isto para a cena iniciar a caminhada automaticamente mal abre!")]
    public bool iniciarLogoNoStart = true;

    [Header("Objetos Principais")]
    [Tooltip("Arrasta o teu Player (o objeto que tem o CharacterController e o FPMove)")]

    public GameObject jogador;

    [Tooltip("O objeto secreto que vai aparecer quando o jogador parar")]
    public GameObject objetoARevelar;

    [Header("Bloqueio de Controlos (Opcional)")]
    [Tooltip("Podes arrastar aqui outros scripts que queiras desligar (ex: Inventory). O script já deteta o FPMove/TPMove sozinho!")]
    public MonoBehaviour[] outrosScriptsParaDesligar;

    [Header("Definições da Caminhada")]
    [Tooltip("Quantos segundos o jogador vai andar sozinho?")]
    public float tempoDeCaminhada = 5f;
    [Tooltip("Qual a velocidade do passo automático?")]
    public float velocidade = 3f;


    private bool jaTocou = false;
    private CharacterController controlador;

    private MonoBehaviour fpMoveScript;
    private MonoBehaviour tpMoveScript;





    // ---------------------------------------------
    //  UNITY
    // ---------------------------------------------
    void Start()
    {
        if (jogador == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) jogador = p;
        }

        if (jogador != null)
        {
            controlador = jogador.GetComponent<CharacterController>();

            fpMoveScript = jogador.GetComponent("FPMove") as MonoBehaviour;
            tpMoveScript = jogador.GetComponent("TPMove") as MonoBehaviour;
        }

        if (objetoARevelar != null)
        {
            objetoARevelar.SetActive(false);
        }

        if (iniciarLogoNoStart)
        {
            IniciarCutscene();
        }
    }


    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject == jogador && !jaTocou)
        {
            IniciarCutscene();
        }
    }




    // ---------------------------------------------
    //  PUBLIC METHODS
    // ---------------------------------------------
    public void IniciarCutscene()
    {
        if (!jaTocou)
        {
            StartCoroutine(SequenciaCutscene());
        }
    }




    // ---------------------------------------------
    //  PRIVATE METHODS
    // ---------------------------------------------
    IEnumerator SequenciaCutscene()
    {
        jaTocou = true;

        if (jogador == null)
        {
            yield break;
        }

        if (fpMoveScript != null) fpMoveScript.enabled = false;
        if (tpMoveScript != null) tpMoveScript.enabled = false;

        foreach (var script in outrosScriptsParaDesligar)
        {
            if (script != null) script.enabled = false;
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        float timer = 0f;
        while (timer < tempoDeCaminhada)
        {
            timer += Time.deltaTime;

            if (controlador != null && controlador.enabled)
            {
                Vector3 direcao = Camera.main != null ? Camera.main.transform.forward : jogador.transform.forward;
                direcao.y = 0;
                direcao.Normalize();

                Vector3 movimento = direcao * velocidade;

                if (!controlador.isGrounded)
                {
                    movimento.y = -9.8f * 3f; 
                }

                controlador.Move(movimento * Time.deltaTime);
            }

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            yield return null;
        }

        if (objetoARevelar != null)
        {
            objetoARevelar.SetActive(true);
        }

        if (fpMoveScript != null) fpMoveScript.enabled = true;
        if (tpMoveScript != null) tpMoveScript.enabled = true;

        foreach (var script in outrosScriptsParaDesligar)
        {
            if (script != null) script.enabled = true;
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        Collider meuCollider = GetComponent<Collider>();
        if (meuCollider != null) meuCollider.enabled = false;
    }
}
