using UnityEngine;
using TMPro;

public class LighterText : MonoBehaviour
{
    // ---------------------------------------------
    //  INSPETOR
    // ---------------------------------------------
    [Header("Definicoes")]
    public float fadeSpeed = 4f;

    // ---------------------------------------------
    //  ESTADO PRIVADO
    // ---------------------------------------------
    private Lighter lighter;
    private TMP_Text textComponent;
    private IntroManager introManager;
    
    private float currentAlpha = 0f;
    private float targetAlpha = 0f;
    private Color originalColor;

    // ---------------------------------------------
    //  UNITY
    // ---------------------------------------------
    void Start()
    {
        lighter = Object.FindFirstObjectByType<Lighter>();
        textComponent = GetComponent<TMP_Text>();
        introManager = Object.FindFirstObjectByType<IntroManager>();

        if (textComponent != null)
        {
            originalColor = textComponent.color; 
            SetAlpha(0f); 
        }
    }

    void Update()
    {
        if (DarknessManager.Instance == null || lighter == null || textComponent == null)
        {
            targetAlpha = 0f;
            ApplyFade();
            return;
        }

        if (introManager != null && Time.timeSinceLevelLoad < introManager.moveUnlockTime)
        {
            targetAlpha = 0f;
            currentAlpha = 0f; 
            SetAlpha(0f);
            return;
        }

        bool isDark = DarknessManager.Instance.IsDark() || DarknessManager.Instance.IsInDarkZone();
        bool lighterIsOff = !lighter.IsLit();
        
        bool showPrompt = isDark && lighterIsOff;

        targetAlpha = showPrompt ? 1f : 0f;

        ApplyFade();
    }

    // ---------------------------------------------
    //  FADE LOGIC
    // ---------------------------------------------
    private void ApplyFade()
    {
        if (textComponent == null) return;

        currentAlpha = Mathf.Lerp(currentAlpha, targetAlpha, Time.deltaTime * fadeSpeed);
        
        SetAlpha(currentAlpha);
    }

    private void SetAlpha(float alpha)
    {
        if (alpha < 0.01f && targetAlpha == 0f)
        {
            textComponent.enabled = false;
        }
        else
        {
            textComponent.enabled = true;
            Color c = originalColor;
            c.a = alpha;
            textComponent.color = c;
        }
    }
}
