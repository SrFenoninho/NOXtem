using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class IntroManager : MonoBehaviour
{
    [Header("Audio")]
    public AudioClip introAudio;
    private AudioSource audioSource;

    [Header("Fade")]
    public Image fadeImage;          // Image preta no Canvas que cobre o ecra
    public float fadeDuration = 4f;  // duração do fade in

    [Header("Timings")]
    public float moveUnlockTime = 6f; // segundos até o player se poder mover

    [Header("Referencias")]
    public FPMove playerMovement;
    public Lighter lighter;

    void Start()
    {
        audioSource = GetComponent<AudioSource>() ?? gameObject.AddComponent<AudioSource>();

        // ecra completamente preto no inicio
        if (fadeImage != null) SetAlpha(1f);

        // bloquear movimento
        if (playerMovement != null) playerMovement.inputBlocked = true;

        if (introAudio != null)
            audioSource.PlayOneShot(introAudio);

        StartCoroutine(IntroSequence());
    }

    IEnumerator IntroSequence()
    {
        // fade in imediato — 4 segundos de preto para gameplay
        StartCoroutine(FadeIn());

        // aguardar 6 segundos antes de libertar o movimento
        yield return new WaitForSeconds(moveUnlockTime);

        if (playerMovement != null) playerMovement.inputBlocked = false;

        // acender o isqueiro automaticamente
        if (lighter != null) lighter.ForceLight(true);
    }

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

    void SetAlpha(float a)
    {
        if (fadeImage == null) return;
        Color c = fadeImage.color;
        c.a = a;
        fadeImage.color = c;
    }
}