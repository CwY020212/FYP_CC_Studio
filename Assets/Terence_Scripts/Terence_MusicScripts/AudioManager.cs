using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Audio Sources")]
    [SerializeField] private AudioSource musicSource; // Assign your AmbientMusicPlayer's AudioSource here
    //[SerializeField] private AudioSource sfxSource;   // Optional: for sound effects

    [Header("Audio Mixer")]
    [SerializeField] private AudioMixer masterMixer; // Assign your main Audio Mixer

    // Optional: Public variables for fading
    public float fadeDuration = 2f;

    private Coroutine fadeRoutine;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Persist across scenes
        }
        else
        {
            Destroy(gameObject);
        }

        // Ensure references are set
        if (musicSource == null) Debug.LogError("Music AudioSource not assigned in AudioManager!");
        //if (sfxSource == null) Debug.LogWarning("SFX AudioSource not assigned in AudioManager!"); // SFX is optional for this context
    }

    // --- Music Control ---

    public void PlayMusic(AudioClip clip, bool loop = true, float volume = 1.0f)
    {
        if (musicSource == null) return;

        musicSource.clip = clip;
        musicSource.loop = loop;
        musicSource.volume = volume;
        musicSource.Play();
    }

    public void StopMusic()
    {
        if (musicSource == null) return;
        musicSource.Stop();
    }

    public void FadeMusicIn(AudioClip clip, float targetVolume, float duration)
    {
        if (musicSource == null) return;
        if (fadeRoutine != null) StopCoroutine(fadeRoutine);
        fadeRoutine = StartCoroutine(FadeMusic(clip, targetVolume, duration, true));
    }

    public void FadeMusicOut(float duration)
    {
        if (musicSource == null) return;
        if (fadeRoutine != null) StopCoroutine(fadeRoutine);
        fadeRoutine = StartCoroutine(FadeMusic(null, 0f, duration, false)); // Pass null clip to indicate fade out existing
    }

    private System.Collections.IEnumerator FadeMusic(AudioClip clip, float targetVolume, float duration, bool fadeIn)
    {
        float startVolume = musicSource.volume;
        float timer = 0f;

        if (fadeIn && clip != null)
        {
            musicSource.clip = clip;
            musicSource.Play();
            startVolume = 0f; // Start from silent for fading in
        }

        while (timer < duration)
        {
            timer += Time.deltaTime;
            float newVolume = Mathf.Lerp(startVolume, targetVolume, timer / duration);
            musicSource.volume = newVolume;
            yield return null;
        }

        musicSource.volume = targetVolume; // Ensure it reaches target volume
        if (!fadeIn && targetVolume <= 0.01f) // If fading out to silent
        {
            musicSource.Stop();
        }
    }

    // --- Volume Control via Mixer (Recommended) ---
    public void SetMasterVolume(float volume)
    {
        if (masterMixer == null) return;
        masterMixer.SetFloat("MasterVolume", Mathf.Log10(volume) * 20); // Volume in dB
    }

    public void SetMusicVolume(float volume)
    {
        if (masterMixer == null) return;
        masterMixer.SetFloat("MusicVolume", Mathf.Log10(volume) * 20); // Volume in dB
    }

    public void SetSfxVolume(float volume)
    {
        if (masterMixer == null) return;
        masterMixer.SetFloat("SfxVolume", Mathf.Log10(volume) * 20); // Volume in dB
    }
}
