using UnityEngine;
using System.Collections;

public class MusicManager : MonoBehaviour
{
    public AudioClip[] tracks;
    public float volume = 0.5f;

    private AudioSource audioSource;
    private int currentTrack = 0;

    void Start()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.volume = volume;
        PlayCurrentTrack();
    }

    void Update()
    {
        if (!audioSource.isPlaying)
            NextTrack();
    }

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

    public void StopMusic()
    {
        StartCoroutine(FadeOut());
    }

    IEnumerator FadeOut()
    {
        float startVolume = audioSource.volume;
        while (audioSource.volume > 0)
        {
            audioSource.volume -= startVolume * Time.deltaTime * 2f; // 2f = velocidade do fade
            yield return null;
        }
        audioSource.Stop();
        audioSource.volume = startVolume;
    }
}