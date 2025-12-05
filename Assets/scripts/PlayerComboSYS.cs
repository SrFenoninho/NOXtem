using UnityEngine;
using UnityEngine.UI;

public class PlayerComboSYS : MonoBehaviour
{
    [Header("UI Settings")]
    public Text comboText;
    public string comboFormat = "COMBO x{0}";

    [Header("Combo Settings")]
    public float comboResetTime = 1f;
    public int minimumComboToShow = 2;

    private int currentCombo = 0;
    private float lastHitTime = 0f;
    private Vector3 originalScale;

    void Start()
    {
        if (comboText != null)
        {
            originalScale = comboText.transform.localScale;
            comboText.text = "";
        }
    }

    void Update()
    {
        if (currentCombo > 0 && Time.time - lastHitTime > comboResetTime)
        {
            ResetCombo();
        }
    }
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
            comboText.text = string.Format(comboFormat, currentCombo);
            comboText.enabled = true;
        }
        else
        {
            comboText.text = "";
        }
    }

    void ResetCombo()
    {
        currentCombo = 0;

        if (comboText != null)
        {
            comboText.text = "";
        }
    }
    void ResetScale()
    {
        if (comboText != null)
        {
            comboText.transform.localScale = originalScale;
        }
    }
    public int GetCurrentCombo()
    {
        return currentCombo;
    }
    public void ForceReset()
    {
        ResetCombo();
    }
}