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
        if (isStopped) return;

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

    // ---------------------------------------------
    //  FADE OUT
    // ---------------------------------------------
    public void StopMusic()
    {
        isStopped = true;
        StartCoroutine(FadeOut());
    }

    IEnumerator FadeOut()
    {
        float startVolume = audioSource.volume;
        while (audioSource.volume > 0)
        {
            audioSource.volume -= startVolume * Time.deltaTime * 2f;
            yield return null;
        }
        audioSource.Stop();
        audioSource.volume = startVolume;
    }
}
