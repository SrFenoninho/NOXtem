using UnityEngine;
using TMPro;
using System.Collections;

public class PlayerComboSYS : MonoBehaviour
{




    // ---------------------------------------------
    //  INSPECTOR
    // ---------------------------------------------
    [Header("UI Settings")]

    public TMP_Text comboText;
    public string comboFormat = "COMBO x{0}";

    [Header("Combo Settings")]
    public float comboResetTime = 1f;
    public int minimumComboToShow = 2;

    [Header("Punch Scale Effect")]
    public float punchScale = 1.3f;
    public float punchDuration = 0.15f;





    // ---------------------------------------------
    //  PRIVATE STATE
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
        if (currentCombo > 0 && Time.time - lastHitTime > comboResetTime)
            ResetCombo();
    }





    // ---------------------------------------------
    //  PUBLIC METHODS
    // ---------------------------------------------
    public void RegisterHit()
    {
        currentCombo++;
        lastHitTime = Time.time;
        UpdateComboUI();
    }




    // ---------------------------------------------
    //  PRIVATE METHODS
    // ---------------------------------------------
    void UpdateComboUI()
    {
        if (comboText == null) return;

        if (currentCombo >= minimumComboToShow)
        {
            comboText.enabled = true;
            comboText.text = string.Format(comboFormat, currentCombo);

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

    IEnumerator PunchScale()
    {
        float elapsed = 0f;

        while (elapsed < punchDuration / 2f)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / (punchDuration / 2f);
            comboText.transform.localScale = Vector3.Lerp(originalScale, originalScale * punchScale, t);
            yield return null;
        }

        elapsed = 0f;

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
