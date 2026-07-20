using UnityEngine;
using System.Collections;

public class SimpleWalkCutscene : MonoBehaviour
{
    [Header("Início Automático")]
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
    
    // Scripts nativos detetados automaticamente
    private MonoBehaviour fpMoveScript;
    private MonoBehaviour tpMoveScript;

    void Start()
    {
        // Se te esqueceres de arrastar o jogador, o script tenta encontrá-lo sozinho!
        if (jogador == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) jogador = p;
        }

        if (jogador != null)
        {
            controlador = jogador.GetComponent<CharacterController>();
            
            // Tenta encontrar automaticamente o FPMove e TPMove no jogador
            fpMoveScript = jogador.GetComponent("FPMove") as MonoBehaviour;
            tpMoveScript = jogador.GetComponent("TPMove") as MonoBehaviour;
        }
        
        // Garante que o objeto começa escondido!
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

    public void IniciarCutscene()
    {
        if (!jaTocou)
        {
            StartCoroutine(SequenciaCutscene());
        }
    }

    IEnumerator SequenciaCutscene()
    {
        jaTocou = true;
        
        if (jogador == null)
        {
            // Debug.LogError("SimpleWalkCutscene: O jogador não foi encontrado!");
            yield break;
        }

        // 1. DESLIGAR CONTROLOS E ESCONDER O RATO (CURSOR)
        if (fpMoveScript != null) fpMoveScript.enabled = false;
        if (tpMoveScript != null) tpMoveScript.enabled = false;

        foreach (var script in outrosScriptsParaDesligar)
        {
            if (script != null) script.enabled = false;
        }

        // Tranca e esconde o cursor (rato) imediatamente!
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // 2. ANDAR SOZINHO DURANTE X SEGUNDOS
        float timer = 0f;
        while (timer < tempoDeCaminhada)
        {
            timer += Time.deltaTime;

            if (controlador != null && controlador.enabled)
            {
                // Usa a direção que a CÂMARA Principal está a olhar (crucial para o FPMove)
                Vector3 direcao = Camera.main != null ? Camera.main.transform.forward : jogador.transform.forward;
                direcao.y = 0; // Para não voar nem enfiar-se na terra
                direcao.Normalize();

                Vector3 movimento = direcao * velocidade;
                
                // Adiciona gravidade pesada
                if (!controlador.isGrounded)
                {
                    movimento.y = -9.8f * 3f; 
                }

                controlador.Move(movimento * Time.deltaTime);
            }

            // Reforça que o cursor continua trancado todos os frames caso outro script tente ligá-lo (ex: Menu de Pausa bugado)
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            yield return null;
        }

        // 3. REVELAR O OBJETO SECRETO
        if (objetoARevelar != null)
        {
            objetoARevelar.SetActive(true);
        }

        // 4. DEVOLVER OS CONTROLOS AO JOGADOR
        if (fpMoveScript != null) fpMoveScript.enabled = true;
        if (tpMoveScript != null) tpMoveScript.enabled = true;

        foreach (var script in outrosScriptsParaDesligar)
        {
            if (script != null) script.enabled = true;
        }

        // No final o cursor deve continuar trancado para jogar em FPMove
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        Collider meuCollider = GetComponent<Collider>();
        if (meuCollider != null) meuCollider.enabled = false;
    }
}
