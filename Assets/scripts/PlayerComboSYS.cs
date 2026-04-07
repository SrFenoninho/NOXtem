using UnityEngine;
using TMPro;
using System.Collections;

public class PlayerComboSYS : MonoBehaviour
{
    // ---------------------------------------------
    //  INSPETOR
    // ---------------------------------------------
    [Header("UI Settings")]
    public TMP_Text comboText;                  // Arrastar o TextMeshPro aqui no Inspector
    public string comboFormat = "COMBO x{0}";

    [Header("Combo Settings")]
    public float comboResetTime = 1f;           // segundos sem acertar antes do combo reiniciar
    public int minimumComboToShow = 2;          // so mostrar o contador a partir de x2

    [Header("Punch Scale Effect")]
    public float punchScale = 1.3f;             // escala maxima do texto a cada acerto
    public float punchDuration = 0.15f;         // duracao da animacao de impacto

    // ---------------------------------------------
    //  ESTADO PRIVADO
    // ---------------------------------------------
    private int currentCombo = 0;
    private float lastHitTime = 0f;
    private Vector3 originalScale;
    private Coroutine punchCoroutine;

    // ---------------------------------------------
    //  UNITY
    // ---------------------------------------------
    void Start()
    {
        if (comboText != null)
        {
            originalScale = comboText.transform.localScale;
            comboText.text = "";
            comboText.enabled = false;
        }
    }

    void Update()
    {
        // Reiniciar combo se nao acertar dentro do comboResetTime
        if (currentCombo > 0 && Time.time - lastHitTime > comboResetTime)
            ResetCombo();
    }

    // ---------------------------------------------
    //  REGISTO DE ACERTO
    // ---------------------------------------------
    // Chamado pela Hitbox quando um ataque acerta num inimigo
    public void RegisterHit()
    {
        currentCombo++;
        lastHitTime = Time.time;
        UpdateComboUI();
    }

    // ---------------------------------------------
    //  UI
    // ---------------------------------------------
    void UpdateComboUI()
    {
        if (comboText == null) return;

        if (currentCombo >= minimumComboToShow)
        {
            comboText.enabled = true;
            comboText.text = string.Format(comboFormat, currentCombo);

            // Efeito de impacto a cada acerto - cancela o anterior se ainda estiver a correr
            if (punchCoroutine != null)
                StopCoroutine(punchCoroutine);
            punchCoroutine = StartCoroutine(PunchScale());
        }
        else
        {
            comboText.text = "";
            comboText.enabled = false;
        }
    }

    // ---------------------------------------------
    //  ANIMAcaO DE ESCALA
    // ---------------------------------------------
    // Escala o texto para cima e depois para baixo - sem DOTween
    IEnumerator PunchScale()
    {
        float elapsed = 0f;

        // Escalar para cima
        while (elapsed < punchDuration / 2f)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / (punchDuration / 2f);
            comboText.transform.localScale = Vector3.Lerp(originalScale, originalScale * punchScale, t);
            yield return null;
        }

        elapsed = 0f;

        // Escalar para baixo
        while (elapsed < punchDuration / 2f)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / (punchDuration / 2f);
            comboText.transform.localScale = Vector3.Lerp(originalScale * punchScale, originalScale, t);
            yield return null;
        }

        comboText.transform.localScale = originalScale;
        punchCoroutine = null;
    }

    // ---------------------------------------------
    //  RESET
    // ---------------------------------------------
    void ResetCombo()
    {
        currentCombo = 0;

        if (punchCoroutine != null)
        {
            StopCoroutine(punchCoroutine);
            punchCoroutine = null;
        }

        if (comboText != null)
        {
            comboText.transform.localScale = originalScale;
            comboText.text = "";
            comboText.enabled = false;
        }
    }

    public int GetCurrentCombo() => currentCombo;
    public void ForceReset() => ResetCombo();
}
