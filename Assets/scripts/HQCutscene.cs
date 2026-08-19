using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public enum TipoTransicao
{
    SemAnimacao,
    FadeSuave,
    EsquerdaParaDireita,
    DireitaParaEsquerda,
    CimaParaBaixo,
    BaixoParaCima
}

[System.Serializable]
public class PainelHQ
{



    // ---------------------------------------------
    //  PRIVATE STATE
    // ---------------------------------------------
    [Tooltip("A imagem/desenho que queres mostrar")]




    // ---------------------------------------------
    //  INSPECTOR
    // ---------------------------------------------
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





    // ---------------------------------------------
    //  UNITY
    // ---------------------------------------------
    void Start()
    {
        if (imagemDaHQ != null) imagemDaHQ.type = Image.Type.Filled;
        if (imagemFundo != null) imagemFundo.type = Image.Type.Simple;

        if (arrancaSozinhoAoIniciar) IniciarHQ();
    }

    void Update()
    {
        if (!cutsceneAtiva || emAnimacao) return;

        if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return))
        {
            AvancarPainel();
        }
        else if (paineisHQ[indiceAtual].tempoDeEspera > 0)
        {
            timerLeitura += Time.deltaTime;
            if (timerLeitura >= paineisHQ[indiceAtual].tempoDeEspera)
            {
                AvancarPainel();
            }
        }
    }





    // ---------------------------------------------
    //  PUBLIC METHODS
    // ---------------------------------------------
    public void IniciarHQ()
    {
        if (paineisHQ == null || paineisHQ.Length == 0) return;

        indiceAtual = 0;
        imagemDaHQ.gameObject.SetActive(true);
        if (imagemFundo != null) imagemFundo.gameObject.SetActive(true);

        cutsceneAtiva = true;

        StartCoroutine(AnimarPainel(paineisHQ[indiceAtual]));
    }




    // ---------------------------------------------
    //  PRIVATE METHODS
    // ---------------------------------------------
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

        if (imagemFundo != null)
        {
            if (indiceAtual == 0) 
                imagemFundo.sprite = null; 
            else 
                imagemFundo.sprite = paineisHQ[indiceAtual - 1].sprite;

            imagemFundo.color = new Color(1f, 1f, 1f, 1f); 
        }

        imagemDaHQ.sprite = painelAtual.sprite;
        imagemDaHQ.color = new Color(1f, 1f, 1f, 1f);

        ConfigurarDirecaoImage(painelAtual.tipoAnimacao);

        if (painelAtual.tipoAnimacao == TipoTransicao.SemAnimacao || painelAtual.velocidadeAnimacao <= 0)
        {
            imagemDaHQ.fillAmount = 1f;
        }
        else if (painelAtual.tipoAnimacao == TipoTransicao.FadeSuave)
        {
            imagemDaHQ.fillAmount = 1f;

            float progresso = 0f;
            while (progresso < 1f)
            {
                progresso += Time.deltaTime / painelAtual.velocidadeAnimacao;
                imagemDaHQ.color = new Color(1f, 1f, 1f, Mathf.Clamp01(progresso));
                yield return null;
            }
        }
        else
        {
            float progresso = 0f;
            while (progresso < 1f)
            {
                progresso += Time.deltaTime / painelAtual.velocidadeAnimacao;
                imagemDaHQ.fillAmount = Mathf.Clamp01(progresso);
                yield return null;
            }
        }

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
            imagemDaHQ.gameObject.SetActive(false);
            if (imagemFundo != null) imagemFundo.gameObject.SetActive(false);
        }
    }
}
