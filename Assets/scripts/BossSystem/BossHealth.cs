using UnityEngine;

public class BossHealth : MonoBehaviour
{



    // ---------------------------------------------
    //  INSPECTOR
    // ---------------------------------------------
    public float maxHealth = 300f;

    public float currentHealth;

    public bool isInvulnerable = false;





    // ---------------------------------------------
    //  PRIVATE STATE
    // ---------------------------------------------
    private BossController boss;

    private GameObject bossHealthCanvas;
    private UnityEngine.UI.Text bossHealthText;

    private int consecutiveHits = 0;
    private float lastHitTime = 0f;





    // ---------------------------------------------
    //  PUBLIC METHODS
    // ---------------------------------------------
    public void Initialize(BossController controller)
    {
        boss = controller;
        currentHealth = maxHealth;
        isInvulnerable = false;
        CreateHealthText();
    }

    public void TakeDamage(float amount)
    {
        if (isInvulnerable) return;

        currentHealth -= amount;

        if (Time.time - lastHitTime > 2.0f) consecutiveHits = 0;
        lastHitTime = Time.time;
        consecutiveHits++;

        boss.OnTookDamage(consecutiveHits);

        UpdateHealthUI();
        CheckThresholds();
    }

    public void ResetConsecutiveHits()
    {
        consecutiveHits = 0;
    }





    // ---------------------------------------------
    //  PRIVATE METHODS
    // ---------------------------------------------
    private void CheckThresholds()
    {
        if (currentHealth <= 25f && boss.currentPhase != BossController.BossPhase.Cutscene)
        {
            boss.currentPhase = BossController.BossPhase.Cutscene;
            boss.ExecuteFinalCutscene();
        }
        else if (currentHealth <= 125f && boss.currentPhase == BossController.BossPhase.Phase2)
            boss.TriggerPhase(BossController.BossPhase.PillarEvent2);
        else if (currentHealth <= 225f && boss.currentPhase == BossController.BossPhase.Phase1)
            boss.TriggerPhase(BossController.BossPhase.PillarEvent1);
    }

    private void CreateHealthText()
    {
        bossHealthCanvas = new GameObject("BossHealthCanvas");
        Canvas c = bossHealthCanvas.AddComponent<Canvas>();
        c.renderMode = RenderMode.ScreenSpaceOverlay;
        c.sortingOrder = 100;
        bossHealthCanvas.AddComponent<UnityEngine.UI.CanvasScaler>();

        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(bossHealthCanvas.transform);
        bossHealthText = textObj.AddComponent<UnityEngine.UI.Text>();

        Font arialFont = (Font)Resources.GetBuiltinResource(typeof(Font), "LegacyRuntime.ttf");
        bossHealthText.font = arialFont;
        bossHealthText.fontSize = 24;
        bossHealthText.color = Color.red;
        bossHealthText.alignment = TextAnchor.UpperCenter;

        UpdateHealthUI();

        RectTransform rect = textObj.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 1f);
        rect.anchorMax = new Vector2(0.5f, 1f);
        rect.pivot = new Vector2(0.5f, 1f);
        rect.anchoredPosition = new Vector2(0, -20);
        rect.sizeDelta = new Vector2(300, 50);
    }

    private void UpdateHealthUI()
    {
        if (bossHealthText != null)
        {
            bossHealthText.text = "BOSS HP: " + currentHealth;
        }
    }
}
