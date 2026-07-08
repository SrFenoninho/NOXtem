using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class GeneratorMinigame : MonoBehaviour, IInteractable
{
    // ---------------------------------------------
    //  INSPETOR
    // ---------------------------------------------
    [Header("UI do Minijogo")]
    public GameObject minigameUI;
    public Image radialImage; 
    [Tooltip("Arrasta o crosshair para aqui para o esconder no minijogo!")]
    public RawImage crosshairUI; 
    
    [Header("Mecânica")]
    public int puxoesNecessarios = 3;
    public float velocidadeInicial = 0.5f;
    public float winZonaInicio = 0.8f;
    public float winZonaFim = 0.95f;

    [Header("Resultado (Portão)")]
    public Transform portaoParaSubir;
    public float alturaSubida = 4f; 
    public float tempoDeSubida = 4f;

    [Header("Áudio")]
    public AudioSource audioSource;
    public AudioClip somPuxarAcerto;
    public AudioClip somPuxarErro;
    public AudioClip somMotorLigado;

    // ---------------------------------------------
    //  ESTADO PRIVADO
    // ---------------------------------------------
    private bool aJogar = false;
    private bool jaResolvido = false;
    private float progressoRoda = 0f;
    private int puxoesDados = 0;
    private float velocidadeAtual;
    private float direcaoRoda = 1f; 

    // ---------------------------------------------
    //  INICIO DO JOGO
    // ---------------------------------------------
    private void Start()
    {
        if (minigameUI != null)
        {
            minigameUI.SetActive(false);
        }
    }

    // ---------------------------------------------
    //  IINTERACTABLE
    // ---------------------------------------------
    public void Interact(GameObject interactor)
    {
        if (jaResolvido || aJogar) return;

        GameStateManager.Instance?.PushState(GameState.Minigame);
        minigameUI.SetActive(true);
        
        // Esconde o Crosshair do ecrã
        if (crosshairUI != null)
        {
            crosshairUI.enabled = false;
        }

        puxoesDados = 0;
        progressoRoda = 0f;
        direcaoRoda = 1f; 
        velocidadeAtual = velocidadeInicial;
        radialImage.fillAmount = 0f;
        
        Time.timeScale = 0f;
        aJogar = true;
    }

    public string GetInteractMessage() => jaResolvido ? "Gerador Ligado" : "Ligar Gerador";

    // ---------------------------------------------
    //  UPDATE COM VAI-E-VEM
    // ---------------------------------------------
    private void Update()
    {
        if (!aJogar) return;

        progressoRoda += Time.unscaledDeltaTime * velocidadeAtual * direcaoRoda;
        
        if (progressoRoda >= 1.0f)
        {
            progressoRoda = 1.0f;
            direcaoRoda = -1f; 
        }
        else if (progressoRoda <= 0f)
        {
            progressoRoda = 0f;
            direcaoRoda = 1f; 
        }

        radialImage.fillAmount = progressoRoda;

        if (Input.GetMouseButtonDown(0))
        {
            if (progressoRoda >= winZonaInicio && progressoRoda <= winZonaFim)
                RegistrarAcerto();
            else
                RegistrarErro(); 
        }
    }

    // ---------------------------------------------
    //  AÇÕES
    // ---------------------------------------------
    private void RegistrarAcerto()
    {
        puxoesDados++;
        progressoRoda = 0f; 
        direcaoRoda = 1f; 
        velocidadeAtual += 0.2f; 
        
        if (audioSource != null && somPuxarAcerto != null)
            audioSource.PlayOneShot(somPuxarAcerto);

        if (puxoesDados >= puxoesNecessarios)
            GanharMinijogo();
    }

    private void RegistrarErro()
    {
        progressoRoda = 0f; 
        direcaoRoda = 1f; 
        
        if (audioSource != null && somPuxarErro != null)
            audioSource.PlayOneShot(somPuxarErro);
    }

    // ---------------------------------------------
    //  FIM
    // ---------------------------------------------
    private void GanharMinijogo()
    {
        aJogar = false;
        jaResolvido = true;
        
        minigameUI.SetActive(false);
        GameStateManager.Instance?.PopState(); 

        // Restaura o Crosshair!
        if (crosshairUI != null)
        {
            crosshairUI.enabled = true;
        }

        Time.timeScale = 1f;

        if (audioSource != null && somMotorLigado != null)
            audioSource.PlayOneShot(somMotorLigado);

        if (portaoParaSubir != null)
            StartCoroutine(SubirPortaoRoutine());
    }

    private IEnumerator SubirPortaoRoutine()
    {
        Vector3 startPos = portaoParaSubir.position;
        Vector3 targetPos = startPos + new Vector3(0, alturaSubida, 0);
        float elapsed = 0f;

        while (elapsed < tempoDeSubida)
        {
            elapsed += Time.deltaTime;
            portaoParaSubir.position = Vector3.Lerp(startPos, targetPos, elapsed / tempoDeSubida);
            yield return null;
        }

        portaoParaSubir.position = targetPos;
    }
}
