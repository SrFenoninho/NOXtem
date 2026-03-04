using UnityEngine;
using System.Collections;

public class MusicManager : MonoBehaviour
{
    // ---------------------------------------------
    //  INSPETOR
    // ---------------------------------------------
    public AudioClip[] tracks;  // lista de faixas a reproduzir em sequência
    public float volume = 0.5f;

    // ---------------------------------------------
    //  ESTADO PRIVADO
    // ---------------------------------------------
    private AudioSource audioSource;
    private int currentTrack = 0;

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
        // Avançar para a próxima faixa quando a atual terminar
        if (!audioSource.isPlaying)
            NextTrack();
    }

    // ---------------------------------------------
    //  REPRODUÇÃO
    // ---------------------------------------------
    void PlayCurrentTrack()
    {
        if (tracks.Length == 0) return;
        audioSource.clip = tracks[currentTrack];
        audioSource.loop = tracks.Length == 1; // em loop se só houver uma faixa
        audioSource.Play();
    }

    void NextTrack()
    {
        currentTrack = (currentTrack + 1) % tracks.Length;
        PlayCurrentTrack();
    }

    // ---------------------------------------------
    //  FADE OUT
    // ---------------------------------------------
    public void StopMusic()
    {
        StartCoroutine(FadeOut());
    }

    IEnumerator FadeOut()
    {
        float startVolume = audioSource.volume;
        while (audioSource.volume > 0)
        {
            audioSource.volume -= startVolume * Time.deltaTime * 2f; // 0.5 segundos de fade
            yield return null;
        }
        audioSource.Stop();
        audioSource.volume = startVolume; // repor volume para uso futuro
    }
}
