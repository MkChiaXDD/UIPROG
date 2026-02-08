using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [System.Serializable]
    public class NamedClip
    {
        public string name;
        public AudioClip clip;
    }

    // PlayerPrefs Keys (you can keep these the same as your exposed params)
    private const string KEY_MASTER = "Master"; // float 0..1
    private const string KEY_BGM = "BGM";       // float 0..1
    private const string KEY_SFX = "SFX";       // float 0..1

    [Header("Audio Mixer")]
    [SerializeField] private AudioMixer mixer; // drag your AudioMixer asset here

    // Exposed parameter names (must match exactly)
    [SerializeField] private string masterParam = "Master";
    [SerializeField] private string bgmParam = "BGM";
    [SerializeField] private string sfxParam = "SFX";

    [Header("BGM")]
    [SerializeField] private AudioSource bgmSource;
    [SerializeField] private List<NamedClip> bgmClips = new List<NamedClip>();

    [Header("SFX")]
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private List<NamedClip> sfxClips = new List<NamedClip>();

    [Header("Default Volumes")]
    [Range(0f, 1f)][SerializeField] private float defaultMasterVolume = 1f;
    [Range(0f, 1f)][SerializeField] private float defaultBgmVolume = 1f;
    [Range(0f, 1f)][SerializeField] private float defaultSfxVolume = 1f;

    private Dictionary<string, AudioClip> bgmDict;
    private Dictionary<string, AudioClip> sfxDict;

    private float masterVolume = 1f;
    private float bgmVolume = 1f;
    private float sfxVolume = 1f;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        EnsureAudioSources();
        BuildDictionaries();

        LoadVolumes();
        ApplyMixerVolumes();
    }

    private void EnsureAudioSources()
    {
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
            if (!bgmDict.ContainsKey(nc.name)) bgmDict.Add(nc.name, nc.clip);
        }

        sfxDict = new Dictionary<string, AudioClip>();
        foreach (var nc in sfxClips)
        {
            if (nc == null || string.IsNullOrEmpty(nc.name) || nc.clip == null) continue;
            if (!sfxDict.ContainsKey(nc.name)) sfxDict.Add(nc.name, nc.clip);
        }
    }

    // Slider 0..1 -> dB for mixer (-80..0)
    private float ToDb(float v01)
    {
        v01 = Mathf.Clamp(v01, 0.0001f, 1f);
        return Mathf.Log10(v01) * 20f; // 1 => 0dB, 0.0001 => -80dB
    }

    private void LoadVolumes()
    {
        masterVolume = Mathf.Clamp01(PlayerPrefs.GetFloat(KEY_MASTER, defaultMasterVolume));
        bgmVolume = Mathf.Clamp01(PlayerPrefs.GetFloat(KEY_BGM, defaultBgmVolume));
        sfxVolume = Mathf.Clamp01(PlayerPrefs.GetFloat(KEY_SFX, defaultSfxVolume));
    }

    private void SaveVolumes()
    {
        PlayerPrefs.SetFloat(KEY_MASTER, masterVolume);
        PlayerPrefs.SetFloat(KEY_BGM, bgmVolume);
        PlayerPrefs.SetFloat(KEY_SFX, sfxVolume);
        PlayerPrefs.Save();
    }

    private void ApplyMixerVolumes()
    {
        if (!mixer)
        {
            Debug.LogWarning("[AudioManager] No AudioMixer assigned. Sliders won't affect mixer.");
            return;
        }

        mixer.SetFloat(masterParam, ToDb(masterVolume));
        mixer.SetFloat(bgmParam, ToDb(bgmVolume));
        mixer.SetFloat(sfxParam, ToDb(sfxVolume));
    }

    public float GetMasterVolume() => masterVolume;
    public float GetBgmVolume() => bgmVolume;
    public float GetSfxVolume() => sfxVolume;

    public void SetMasterVolume(float v, bool save = true)
    {
        masterVolume = Mathf.Clamp01(v);
        ApplyMixerVolumes();
        if (save) SaveVolumes();
    }

    public void SetBgmVolume(float v, bool save = true)
    {
        bgmVolume = Mathf.Clamp01(v);
        ApplyMixerVolumes();
        if (save) SaveVolumes();
    }

    public void SetSfxVolume(float v, bool save = true)
    {
        sfxVolume = Mathf.Clamp01(v);
        ApplyMixerVolumes();
        if (save) SaveVolumes();
    }

    // -------------------------
    // Audio playback
    // -------------------------
    public void PlayBGM(string bgmName, bool loop = true, bool restartIfSame = false)
    {
        if (string.IsNullOrEmpty(bgmName)) return;

        if (!bgmDict.TryGetValue(bgmName, out var clip))
        {
            Debug.LogWarning($"[AudioManager] BGM not found: {bgmName}");
            return;
        }

        if (!restartIfSame && bgmSource.isPlaying && bgmSource.clip == clip)
            return;

        bgmSource.loop = loop;
        bgmSource.clip = clip;
        bgmSource.Play();
    }

    public void PlaySFX(string sfxName, float volume01 = 1f)
    {
        if (string.IsNullOrEmpty(sfxName)) return;

        if (!sfxDict.TryGetValue(sfxName, out var clip))
        {
            Debug.LogWarning($"[AudioManager] SFX not found: {sfxName}");
            return;
        }

        sfxSource.PlayOneShot(clip, Mathf.Clamp01(volume01));
    }
}
