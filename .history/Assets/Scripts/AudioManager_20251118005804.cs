using System.Collections;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public AudioSource musicSource;
    public AudioSource ambientSource;
    public AudioClip defaultMusic;
    public AudioClip defaultAmbient;
    public float musicVolume = 1f;
    public float ambientVolume = 1f;

    private static AudioManager _instance;

    void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
        DontDestroyOnLoad(gameObject);
        if (musicSource == null)
        {
            musicSource = gameObject.AddComponent<AudioSource>();
            musicSource.loop = true;
        }
        if (ambientSource == null)
        {
            ambientSource = gameObject.AddComponent<AudioSource>();
            ambientSource.loop = true;
        }
    }

    void Start()
    {
        if (defaultMusic != null) PlayMusic(defaultMusic, musicVolume, true);
        if (defaultAmbient != null) PlayAmbient(defaultAmbient, ambientVolume, true);
    }

    public void PlayMusic(AudioClip clip, float volume, bool loop)
    {
        if (clip == null) return;
        musicSource.clip = clip;
        musicSource.volume = Mathf.Clamp01(volume);
        musicSource.loop = loop;
        musicSource.Play();
    }

    public void PlayAmbient(AudioClip clip, float volume, bool loop)
    {
        if (clip == null) return;
        ambientSource.clip = clip;
        ambientSource.volume = Mathf.Clamp01(volume);
        ambientSource.loop = loop;
        ambientSource.Play();
    }

    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
        musicSource.volume = musicVolume;
    }

    public void SetAmbientVolume(float volume)
    {
        ambientVolume = Mathf.Clamp01(volume);
        ambientSource.volume = ambientVolume;
    }

    public void FadeMusic(float targetVolume, float duration)
    {
        StartCoroutine(FadeSource(musicSource, targetVolume, duration));
    }

    public void FadeAmbient(float targetVolume, float duration)
    {
        StartCoroutine(FadeSource(ambientSource, targetVolume, duration));
    }

    private IEnumerator FadeSource(AudioSource source, float targetVolume, float duration)
    {
        if (source == null || duration <= 0f) yield break;
        float start = source.volume;
        float t = 0f;
        targetVolume = Mathf.Clamp01(targetVolume);
        while (t < duration)
        {
            t += Time.deltaTime;
            source.volume = Mathf.Lerp(start, targetVolume, t / duration);
            yield return null;
        }
        source.volume = targetVolume;
    }
}