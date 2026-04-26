using UnityEngine;
using System.Collections;

public class MusicManager : MonoBehaviour
{
    // ---------------------------------------------
    //  INSPETOR
    // ---------------------------------------------
    public AudioClip[] tracks;
    public float volume = 0.5f;

    // ---------------------------------------------
    //  ESTADO PRIVADO
    // ---------------------------------------------
    private AudioSource audioSource;
    private int currentTrack = 0;
    private bool isStopped = false;
    // Impede o Update de reiniciar a musica quando esta pausada externamente
    private bool isPaused = false;
    private int savedTimeSamples = 0;

    // ---------------------------------------------
    //  UNITY
    // ---------------------------------------------
    void Start()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.volume = volume;
        PlayCurrentTrack();
    }

    void Update()
    {
        // Nao fazer nada se parado ou pausado
        if (isStopped || isPaused) return;

        if (!audioSource.isPlaying)
            NextTrack();
    }

    // ---------------------------------------------
    //  REPRODUCAO
    // ---------------------------------------------
    void PlayCurrentTrack()
    {
        if (tracks.Length == 0) return;
        audioSource.clip = tracks[currentTrack];
        audioSource.loop = tracks.Length == 1;
        audioSource.Play();
    }

    void NextTrack()
    {
        currentTrack = (currentTrack + 1) % tracks.Length;
        PlayCurrentTrack();
    }

    // Pausa e guarda a posicao exata
    public void PauseMusic()
    {
        if (audioSource.isPlaying)
        {
            savedTimeSamples = audioSource.timeSamples;
            audioSource.Pause();
        }
        isPaused = true;
    }

    // Retoma exatamente onde parou
    public void ResumeMusic()
    {
        if (isStopped) return;
        isPaused = false;
        audioSource.timeSamples = savedTimeSamples;
        audioSource.UnPause();
    }

    // ---------------------------------------------
    //  PARAGEM DEFINITIVA (com fade)
    // ---------------------------------------------
    public void StopMusic()
    {
        isStopped = true;
        StartCoroutine(FadeOut());
    }

    IEnumerator FadeOut()
    {
        float startVolume = audioSource.volume;
        // unscaledDeltaTime para o fade funcionar mesmo com timeScale = 0
        while (audioSource.volume > 0)
        {
            audioSource.volume -= startVolume * Time.unscaledDeltaTime * 2f;
            yield return null;
        }
        audioSource.Stop();
        audioSource.volume = startVolume;
    }
}
