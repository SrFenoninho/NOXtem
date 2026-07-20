using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public enum TipoTransicao
{
    SemAnimacao,
    FadeSuave, // Fade de transparência clássico (Dissolve)
    EsquerdaParaDireita,
    DireitaParaEsquerda,
    CimaParaBaixo,
    BaixoParaCima
}

[System.Serializable]
public class PainelHQ
{
    [Tooltip("A imagem/desenho que queres mostrar")]
    public Sprite sprite;
    
    [Tooltip("Tempo (em segundos) que a imagem fica no ecrã antes de avançar sozinha (Mete 0 se quiseres que avance APENAS pelo clique)")]
    public float tempoDeEspera = 3f;

    [Tooltip("Qual é a animação para este desenho aparecer?")]
    public TipoTransicao tipoAnimacao = TipoTransicao.EsquerdaParaDireita;
    
    [Tooltip("Velocidade da transição (em segundos)")]
    public float velocidadeAnimacao = 1f;
}

public class HQCutscene : MonoBehaviour
{
    [Header("Início Automático")]
    [Tooltip("Se estiver ligado, a HQ começa imediatamente mal entras na cena!")]
    public bool arrancaSozinhoAoIniciar = true;

    [Header("Configurações Visuais")]
    [Tooltip("O teu painel principal onde o desenho novo aparece (Image Type deve ser Filled)")]
    public Image imagemDaHQ;
    
    [Tooltip("NOVO: Cria uma SEGUNDA Image no Unity para ficar por trás da imagemDaHQ, e arrasta-a para aqui! Isto vai mostrar a foto antiga durante a transição.")]
    public Image imagemFundo;
    
    [Tooltip("Cria aqui a lista de todas as tuas imagens")]
    public PainelHQ[] paineisHQ;

    [Header("Carregar Próxima Cena")]
    [Tooltip("Nome exato da cena (Ex: Floor1)")]
    public string nomeDaProximaCena;

    private int indiceAtual = 0;
    private bool cutsceneAtiva = false;
    private bool emAnimacao = false;
    private float timerLeitura = 0f;

    void Start()
    {
        // Garante que o Image Type está correto no Unity automaticamente
        if (imagemDaHQ != null) imagemDaHQ.type = Image.Type.Filled;
        if (imagemFundo != null) imagemFundo.type = Image.Type.Simple;

        if (arrancaSozinhoAoIniciar) IniciarHQ();
    }

    void Update()
    {
        if (!cutsceneAtiva || emAnimacao) return;

        // 1. Passar pelo clique
        if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return))
        {
            AvancarPainel();
        }
        // 2. Passar pelo tempo
        else if (paineisHQ[indiceAtual].tempoDeEspera > 0)
        {
            timerLeitura += Time.deltaTime;
            if (timerLeitura >= paineisHQ[indiceAtual].tempoDeEspera)
            {
                AvancarPainel();
            }
        }
    }

    public void IniciarHQ()
    {
        if (paineisHQ == null || paineisHQ.Length == 0) return;

        indiceAtual = 0;
        imagemDaHQ.gameObject.SetActive(true);
        if (imagemFundo != null) imagemFundo.gameObject.SetActive(true);
        
        cutsceneAtiva = true;

        StartCoroutine(AnimarPainel(paineisHQ[indiceAtual]));
    }

    void AvancarPainel()
    {
        indiceAtual++;
        timerLeitura = 0f;

        if (indiceAtual < paineisHQ.Length)
        {
            StartCoroutine(AnimarPainel(paineisHQ[indiceAtual]));
        }
        else
        {
            FinalizarHQ();
        }
    }

    IEnumerator AnimarPainel(PainelHQ painelAtual)
    {
        emAnimacao = true;
        
        // --- 1. CONFIGURAR O FUNDO (FOTO ANTERIOR) ---
        if (imagemFundo != null)
        {
            // Se for o primeiro desenho de todos, deixamos o fundo preto ou transparente
            if (indiceAtual == 0) 
                imagemFundo.sprite = null; 
            else 
                imagemFundo.sprite = paineisHQ[indiceAtual - 1].sprite; // Mostra a imagem antiga!
            
            // Fundo é sempre 100% visível durante a transição
            imagemFundo.color = new Color(1f, 1f, 1f, 1f); 
        }

        // --- 2. CONFIGURAR A FRENTE (NOVA FOTO) ---
        imagemDaHQ.sprite = painelAtual.sprite;
        imagemDaHQ.color = new Color(1f, 1f, 1f, 1f); // Reset ao alpha
        
        ConfigurarDirecaoImage(painelAtual.tipoAnimacao);

        // --- 3. EXECUTAR ANIMAÇÃO ---
        if (painelAtual.tipoAnimacao == TipoTransicao.SemAnimacao || painelAtual.velocidadeAnimacao <= 0)
        {
            imagemDaHQ.fillAmount = 1f;
        }
        else if (painelAtual.tipoAnimacao == TipoTransicao.FadeSuave)
        {
            imagemDaHQ.fillAmount = 1f; // A imagem tem de estar toda preenchida
            
            float progresso = 0f;
            while (progresso < 1f)
            {
                progresso += Time.deltaTime / painelAtual.velocidadeAnimacao;
                // Fade In pelo Alpha (Transparência) da Imagem da Frente
                imagemDaHQ.color = new Color(1f, 1f, 1f, Mathf.Clamp01(progresso));
                yield return null;
            }
        }
        else
        {
            // Animar pelo preenchimento (Slide / Wipe de Direções)
            float progresso = 0f;
            while (progresso < 1f)
            {
                progresso += Time.deltaTime / painelAtual.velocidadeAnimacao;
                imagemDaHQ.fillAmount = Mathf.Clamp01(progresso);
                yield return null;
            }
        }

        // No fim da animação, garante que está 100% visível
        imagemDaHQ.color = new Color(1f, 1f, 1f, 1f);
        imagemDaHQ.fillAmount = 1f;
        emAnimacao = false;
    }

    void ConfigurarDirecaoImage(TipoTransicao direcao)
    {
        switch (direcao)
        {
            case TipoTransicao.EsquerdaParaDireita:
                imagemDaHQ.fillMethod = Image.FillMethod.Horizontal;
                imagemDaHQ.fillOrigin = (int)Image.OriginHorizontal.Left;
                break;
            case TipoTransicao.DireitaParaEsquerda:
                imagemDaHQ.fillMethod = Image.FillMethod.Horizontal;
                imagemDaHQ.fillOrigin = (int)Image.OriginHorizontal.Right;
                break;
            case TipoTransicao.CimaParaBaixo:
                imagemDaHQ.fillMethod = Image.FillMethod.Vertical;
                imagemDaHQ.fillOrigin = (int)Image.OriginVertical.Top;
                break;
            case TipoTransicao.BaixoParaCima:
                imagemDaHQ.fillMethod = Image.FillMethod.Vertical;
                imagemDaHQ.fillOrigin = (int)Image.OriginVertical.Bottom;
                break;
        }
        
        // Se a transição for de direção (deslizar/wipe), o fillAmount começa vazio (0%)
        if (direcao != TipoTransicao.SemAnimacao && direcao != TipoTransicao.FadeSuave)
        {
            imagemDaHQ.fillAmount = 0f;
        }
    }

    void FinalizarHQ()
    {
        cutsceneAtiva = false;
        
        if (!string.IsNullOrEmpty(nomeDaProximaCena))
        {
            LoadingManager.Carregar(nomeDaProximaCena);
        }
        else
        {
            // Debug.LogWarning("HQ acabou, mas não puseste o nome da próxima cena!");
            imagemDaHQ.gameObject.SetActive(false);
            if (imagemFundo != null) imagemFundo.gameObject.SetActive(false);
        }
    }
}
