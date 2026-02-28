using UnityEngine;
using TMPro;                                    // TextMeshPro instead of legacy Text
using System.Collections;

public class PlayerComboSYS : MonoBehaviour
{
    [Header("UI Settings")]
    public TMP_Text comboText;                  // Drag your TextMeshPro object here in Inspector
    public string comboFormat = "COMBO x{0}";

    [Header("Combo Settings")]
    public float comboResetTime = 1f;           // Time without a hit before combo resets
    public int minimumComboToShow = 2;          // Only show counter from x2 onwards

    [Header("Punch Scale Effect")]
    public float punchScale = 1.3f;             // How big the text gets on each hit
    public float punchDuration = 0.15f;         // How fast the punch animation is

    private int currentCombo = 0;
    private float lastHitTime = 0f;
    private Vector3 originalScale;
    private Coroutine punchCoroutine;

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
        // Reset combo if no hit landed within comboResetTime
        if (currentCombo > 0 && Time.time - lastHitTime > comboResetTime)
            ResetCombo();
    }

    // Called by PlayerCombat.OnHitLanded() when an attack connects
    public void RegisterHit()
    {
        currentCombo++;
        lastHitTime = Time.time;
        UpdateComboUI();
    }

    void UpdateComboUI()
    {
        if (comboText == null) return;

        if (currentCombo >= minimumComboToShow)
        {
            comboText.enabled = true;
            comboText.text = string.Format(comboFormat, currentCombo);

            // Punch scale effect on every hit
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

    // Simple scale punch without DOTween
    IEnumerator PunchScale()
    {
        float elapsed = 0f;

        // Scale up
        while (elapsed < punchDuration / 2f)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / (punchDuration / 2f);
            comboText.transform.localScale = Vector3.Lerp(originalScale, originalScale * punchScale, t);
            yield return null;
        }

        elapsed = 0f;

        // Scale back down
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