using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class IntroManager : MonoBehaviour
{
    // ---------------------------------------------
    //  INSPETOR
    // ---------------------------------------------
    [Header("Audio")]
    public AudioClip introAudio;
    private AudioSource audioSource;

    [Header("Fade")]
    public Image fadeImage;
    public float fadeDuration = 4f;

    [Header("Timings")]
    public float moveUnlockTime = 6f;

    [Header("Referências")]
    public FPMove playerMovement;
    public Lighter lighter;

    // ---------------------------------------------
    //  UNITY
    // ---------------------------------------------
    void Start()
    {
        audioSource = GetComponent<AudioSource>() ?? gameObject.AddComponent<AudioSource>();

        if (fadeImage != null) SetAlpha(1f);

        if (playerMovement != null) playerMovement.inputBlocked = true;
        if (lighter != null) lighter.inputBlocked = true;

        // Bloquear menu radial durante a intro
        GameStateManager.Instance?.PushState(GameState.Cutscene);

        if (introAudio != null)
            audioSource.PlayOneShot(introAudio);

        StartCoroutine(IntroSequence());
    }

    // ---------------------------------------------
    //  SEQUÊNCIA DA INTRO
    // ---------------------------------------------
    IEnumerator IntroSequence()
    {
        StartCoroutine(FadeIn());

        yield return new WaitForSeconds(moveUnlockTime);

        if (playerMovement != null) playerMovement.inputBlocked = false;
        if (lighter != null) lighter.inputBlocked = false;
        if (lighter != null) lighter.ForceLight(true);

        // Libertar o menu radial
        GameStateManager.Instance?.PopState();
    }

    // ---------------------------------------------
    //  FADE
    // ---------------------------------------------
    IEnumerator FadeIn()
    {
        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            SetAlpha(1f - Mathf.Clamp01(elapsed / fadeDuration));
            yield return null;
        }
        SetAlpha(0f);
        if (fadeImage != null) fadeImage.gameObject.SetActive(false);
    }

    // ---------------------------------------------
    //  AUXILIARES
    // ---------------------------------------------
    void SetAlpha(float a)
    {
        if (fadeImage == null) return;
        Color c = fadeImage.color;
        c.a = a;
        fadeImage.color = c;
    }
}