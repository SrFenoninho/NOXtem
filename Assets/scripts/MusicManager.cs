using UnityEngine;
using System.Collections;

public class MusicManager : MonoBehaviour
{





    // ---------------------------------------------
    //  INSPECTOR
    // ---------------------------------------------
    public AudioClip[] tracks;
    public float volume = 0.5f;





    // ---------------------------------------------
    //  PRIVATE STATE
    // ---------------------------------------------
    private AudioSource audioSource;
    private int currentTrack = 0;
    private bool isStopped = false;
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
        if (isStopped || isPaused) return;

        if (!audioSource.isPlaying)
            NextTrack();
    }




    // ---------------------------------------------
    //  PRIVATE METHODS
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
        if (tracks.Length == 0) return;
        currentTrack = (currentTrack + 1) % tracks.Length;
        PlayCurrentTrack();
    }




    // ---------------------------------------------
    //  PUBLIC METHODS
    // ---------------------------------------------
    public void PauseMusic()
    {
        if (audioSource.isPlaying)
        {
            savedTimeSamples = audioSource.timeSamples;
            audioSource.Pause();
        }
        isPaused = true;
    }

    public void ResumeMusic()
    {
        if (isStopped) return;
        if (audioSource.clip == null) return;
        isPaused = false;
        audioSource.timeSamples = savedTimeSamples;
        audioSource.UnPause();
    }

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
            audioSource.volume -= startVolume * Time.unscaledDeltaTime * 2f;
            yield return null;
        }
        audioSource.Stop();
        audioSource.volume = startVolume;
    }
}
