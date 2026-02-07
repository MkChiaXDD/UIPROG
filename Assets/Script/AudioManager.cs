using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [System.Serializable]
    public class NamedClip
    {
        public string name;   // e.g. "MainTheme", "Menu", "Jump", "Explosion"
        public AudioClip clip;
    }

    [Header("BGM")]
    [SerializeField] private AudioSource bgmSource;
    [SerializeField] private List<NamedClip> bgmClips = new List<NamedClip>();

    [Header("SFX")]
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private List<NamedClip> sfxClips = new List<NamedClip>();

    private Dictionary<string, AudioClip> bgmDict;
    private Dictionary<string, AudioClip> sfxDict;

    private void Awake()
    {
        // Singleton
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Make sure audio sources exist
        EnsureAudioSources();

        // Build lookup tables
        BuildDictionaries();
    }

    private void EnsureAudioSources()
    {
        // If you didn’t assign them, create them.
        if (!bgmSource)
        {
            var bgmObj = new GameObject("BGM_Source");
            bgmObj.transform.SetParent(transform);
            bgmSource = bgmObj.AddComponent<AudioSource>();
            bgmSource.playOnAwake = false;
            bgmSource.loop = true;
        }

        if (!sfxSource)
        {
            var sfxObj = new GameObject("SFX_Source");
            sfxObj.transform.SetParent(transform);
            sfxSource = sfxObj.AddComponent<AudioSource>();
            sfxSource.playOnAwake = false;
            sfxSource.loop = false;
        }
    }

    private void BuildDictionaries()
    {
        bgmDict = new Dictionary<string, AudioClip>();
        foreach (var nc in bgmClips)
        {
            if (nc == null || string.IsNullOrEmpty(nc.name) || nc.clip == null) continue;
            if (!bgmDict.ContainsKey(nc.name))
                bgmDict.Add(nc.name, nc.clip);
            else
                Debug.LogWarning($"[AudioManager] Duplicate BGM name: {nc.name}");
        }

        sfxDict = new Dictionary<string, AudioClip>();
        foreach (var nc in sfxClips)
        {
            if (nc == null || string.IsNullOrEmpty(nc.name) || nc.clip == null) continue;
            if (!sfxDict.ContainsKey(nc.name))
                sfxDict.Add(nc.name, nc.clip);
            else
                Debug.LogWarning($"[AudioManager] Duplicate SFX name: {nc.name}");
        }
    }

    // -------------------------
    // Public API
    // -------------------------
    public void PlayBGM(string bgmName, bool loop = true, bool restartIfSame = false, float volume01 = 1f)
    {
        if (string.IsNullOrEmpty(bgmName)) return;

        if (!bgmDict.TryGetValue(bgmName, out var clip))
        {
            Debug.LogWarning($"[AudioManager] BGM not found: {bgmName}");
            return;
        }

        // If same clip is already playing, don’t restart unless you want to
        if (!restartIfSame && bgmSource.isPlaying && bgmSource.clip == clip)
            return;

        bgmSource.loop = loop;
        bgmSource.volume = Mathf.Clamp01(volume01);
        bgmSource.clip = clip;
        bgmSource.Play();
    }

    public void StopBGM()
    {
        if (bgmSource) bgmSource.Stop();
    }

    public void PauseBGM(bool pause)
    {
        if (!bgmSource) return;
        if (pause) bgmSource.Pause();
        else bgmSource.UnPause();
    }

    public void PlaySFX(string sfxName, float volume01 = 1f)
    {
        if (string.IsNullOrEmpty(sfxName)) return;

        if (!sfxDict.TryGetValue(sfxName, out var clip))
        {
            Debug.LogWarning($"[AudioManager] SFX not found: {sfxName}");
            return;
        }

        // volume01 is a scale (0..1 typically)
        sfxSource.PlayOneShot(clip, Mathf.Clamp01(volume01));
    }

    public void SetBgmVolume(float volume01)
    {
        if (!bgmSource) return;
        bgmSource.volume = Mathf.Clamp01(volume01);
    }

    public void SetSfxVolume(float volume01)
    {
        if (!sfxSource) return;
        sfxSource.volume = Mathf.Clamp01(volume01);
    }
}
