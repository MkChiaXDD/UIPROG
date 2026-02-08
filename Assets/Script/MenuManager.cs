using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    // -------------------------
    // PlayerPrefs Keys
    // -------------------------
    private const string KEY_LANG = "SelectedLanguage";     // string (locale code)
    private const string KEY_MAP_ROT = "MapRotationFixed";  // int (1 fixed, 0 rotate)
    private const string KEY_CROSSHAIR = "CrosshairIndex";  // int (0 = crosshair1, 1 = crosshair2)

    [Header("Language Dropdown")]
    [SerializeField] private TMP_Dropdown languageDropdown;
    [SerializeField] private List<Locale> locales;

    [Header("Settings Tabs")]
    [SerializeField] private Button AudioBtn;
    [SerializeField] private Button VideoBtn;

    [Header("Map Rotation Buttons")]
    [SerializeField] private Button fixedBtn;
    [SerializeField] private Button rotateBtn;

    [Header("Crosshair Buttons (Optional)")]
    [SerializeField] private Button crosshairBtn1;
    [SerializeField] private Button crosshairBtn2;

    [Header("Crosshair Visual (Optional)")]
    [SerializeField] private Image crosshairImage;
    [SerializeField] private Sprite crosshairSprite1;
    [SerializeField] private Sprite crosshairSprite2;

    [Header("Button Colors")]
    [SerializeField] private Color selectedColor = new Color(0.6f, 0.6f, 0.6f, 1f);
    [SerializeField] private Color unselectedColor = Color.white;

    [Header("Volume Sliders")]
    [SerializeField] private Slider masterSlider;
    [SerializeField] private Slider bgmSlider;
    [SerializeField] private Slider sfxSlider;

    [Header("Settings")]
    [SerializeField] private GameObject settingsCanvas;
    private bool isSettingsActive;

    // Cached current settings
    private bool isMapFixed = true;
    private int crosshairIndex = 0; // 0 = first, 1 = second

    private bool HasCrosshairUI =>
        crosshairBtn1 != null && crosshairBtn2 != null;

    private bool HasCrosshairVisual =>
        crosshairImage != null && crosshairSprite1 != null && crosshairSprite2 != null;

    private IEnumerator Start()
    {
        yield return LocalizationSettings.InitializationOperation;

        if (languageDropdown)
            languageDropdown.onValueChanged.AddListener(OnLanguageDropdownChanged);

        LoadAndApplyLanguage();
        LoadAndApplyMapRotation();
        LoadAndApplyCrosshair();

        // Audio/Video tab: no PlayerPrefs, just set a default visual state
        SetSettingsTabSelected(isAudio: true);

        InitSliders();
    }

    private void Awake()
    {
        if (fixedBtn) fixedBtn.onClick.AddListener(OnFixedClicked);
        if (rotateBtn) rotateBtn.onClick.AddListener(OnRotateClicked);

        if (AudioBtn) AudioBtn.onClick.AddListener(OnAudioClicked);
        if (VideoBtn) VideoBtn.onClick.AddListener(OnVideoClicked);

        if (crosshairBtn1) crosshairBtn1.onClick.AddListener(OnCrosshair1Clicked);
        if (crosshairBtn2) crosshairBtn2.onClick.AddListener(OnCrosshair2Clicked);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            ToggleSettings();
    }

    // -------------------------
    // Language
    // -------------------------
    private void OnLanguageDropdownChanged(int index)
    {
        if (!languageDropdown) return;
        if (index < 0 || index >= locales.Count) return;

        ChangeLanguage(locales[index]);
    }

    private void LoadAndApplyLanguage()
    {
        string savedLangCode = PlayerPrefs.GetString(KEY_LANG, "");
        if (!string.IsNullOrEmpty(savedLangCode))
        {
            var savedLocale = LocalizationSettings.AvailableLocales
                .GetLocale(new LocaleIdentifier(savedLangCode));

            if (savedLocale != null)
            {
                ApplyLocale(savedLocale);
                return;
            }
        }

        var deviceLocale = LocalizationSettings.AvailableLocales
            .GetLocale(Application.systemLanguage);

        if (deviceLocale != null)
        {
            ApplyLocale(deviceLocale);
            return;
        }

        if (LocalizationSettings.AvailableLocales.Locales.Count > 0)
            ApplyLocale(LocalizationSettings.AvailableLocales.Locales[0]);
    }

    private void ApplyLocale(Locale locale)
    {
        LocalizationSettings.SelectedLocale = locale;

        if (languageDropdown)
        {
            int dropdownIndex = locales.IndexOf(locale);
            if (dropdownIndex >= 0)
                languageDropdown.SetValueWithoutNotify(dropdownIndex);
        }
    }

    private void ChangeLanguage(Locale targetLocale)
    {
        ApplyLocale(targetLocale);
        PlayerPrefs.SetString(KEY_LANG, targetLocale.Identifier.Code);
        PlayerPrefs.Save();
    }

    // -------------------------
    // Map Rotation
    // -------------------------
    private void OnFixedClicked()
    {
        SetMapRotation(isFixed: true, save: true);
        // your fixed mode logic here
    }

    private void OnRotateClicked()
    {
        SetMapRotation(isFixed: false, save: true);
        // your rotate mode logic here
    }

    private void LoadAndApplyMapRotation()
    {
        int saved = PlayerPrefs.GetInt(KEY_MAP_ROT, 1); // default fixed
        SetMapRotation(isFixed: saved == 1, save: false);
    }

    private void SetMapRotation(bool isFixed, bool save)
    {
        isMapFixed = isFixed;
        ApplyTogglePairVisual(fixedBtn, rotateBtn, selectFirst: isFixed);
        AudioManager.Instance.PlaySFX("Toggle");

        if (save)
        {
            PlayerPrefs.SetInt(KEY_MAP_ROT, isFixed ? 1 : 0);
            PlayerPrefs.Save();
        }
    }

    // -------------------------
    // Audio / Video Tabs (NO PlayerPrefs)
    // -------------------------
    private void OnAudioClicked()
    {
        SetSettingsTabSelected(isAudio: true);
        // show audio panel here if you have it
    }

    private void OnVideoClicked()
    {
        SetSettingsTabSelected(isAudio: false);
        // show video panel here if you have it
    }

    private void SetSettingsTabSelected(bool isAudio)
    {
        ApplyTogglePairVisual(AudioBtn, VideoBtn, selectFirst: isAudio);
        AudioManager.Instance.PlaySFX("Toggle");
    }

    // -------------------------
    // Crosshair (Optional)
    // -------------------------
    private void OnCrosshair1Clicked()
    {
        SetCrosshair(index: 0, save: true);
    }

    private void OnCrosshair2Clicked()
    {
        SetCrosshair(index: 1, save: true);
    }

    private void LoadAndApplyCrosshair()
    {
        if (!HasCrosshairUI && !HasCrosshairVisual) return;

        int saved = PlayerPrefs.GetInt(KEY_CROSSHAIR, 0);
        saved = Mathf.Clamp(saved, 0, 1);
        SetCrosshair(saved, save: false);
    }

    private void SetCrosshair(int index, bool save)
    {
        crosshairIndex = Mathf.Clamp(index, 0, 1);

        if (HasCrosshairUI)
            ApplyTogglePairVisual(crosshairBtn1, crosshairBtn2, selectFirst: crosshairIndex == 0);

        if (HasCrosshairVisual)
            crosshairImage.sprite = (crosshairIndex == 0) ? crosshairSprite1 : crosshairSprite2;

        AudioManager.Instance.PlaySFX("Toggle");
        if (save)
        {
            PlayerPrefs.SetInt(KEY_CROSSHAIR, crosshairIndex);
            PlayerPrefs.Save();
        }
    }

    // -------------------------
    // Shared UI helpers
    // -------------------------
    private void ApplyTogglePairVisual(Button first, Button second, bool selectFirst)
    {
        if (!first || !second) return;

        SetButtonColor(first, selectFirst ? selectedColor : unselectedColor);
        SetButtonColor(second, !selectFirst ? selectedColor : unselectedColor);

        first.interactable = !selectFirst;
        second.interactable = selectFirst;
    }

    private void SetButtonColor(Button btn, Color c)
    {
        if (!btn) return;
        var img = btn.GetComponent<Image>();
        if (img) img.color = c;
    }

    private void InitSliders()
    {
        if (AudioManager.Instance == null) return;

        float m = AudioManager.Instance.GetMasterVolume();
        float b = AudioManager.Instance.GetBgmVolume();
        float s = AudioManager.Instance.GetSfxVolume();

        if (masterSlider) masterSlider.SetValueWithoutNotify(m);
        if (bgmSlider) bgmSlider.SetValueWithoutNotify(b);
        if (sfxSlider) sfxSlider.SetValueWithoutNotify(s);

        // IMPORTANT: apply to mixer immediately (no saving)
        AudioManager.Instance.SetMasterVolume(m, save: false);
        AudioManager.Instance.SetBgmVolume(b, save: false);
        AudioManager.Instance.SetSfxVolume(s, save: false);

        // listeners
        if (masterSlider) masterSlider.onValueChanged.AddListener(OnMasterSlider);
        if (bgmSlider) bgmSlider.onValueChanged.AddListener(OnBgmSlider);
        if (sfxSlider) sfxSlider.onValueChanged.AddListener(OnSfxSlider);
    }

    private void OnDisable()
    {
        if (masterSlider) masterSlider.onValueChanged.RemoveListener(OnMasterSlider);
        if (bgmSlider) bgmSlider.onValueChanged.RemoveListener(OnBgmSlider);
        if (sfxSlider) sfxSlider.onValueChanged.RemoveListener(OnSfxSlider);
    }

    private void OnMasterSlider(float v) => AudioManager.Instance?.SetMasterVolume(v, true);
    private void OnBgmSlider(float v) => AudioManager.Instance?.SetBgmVolume(v, true);
    private void OnSfxSlider(float v) => AudioManager.Instance?.SetSfxVolume(v, true);



    // -------------------------
    // Settings / Scene
    // -------------------------
    public void ToggleSettings()
    {
        isSettingsActive = !isSettingsActive;
        if (settingsCanvas) settingsCanvas.SetActive(isSettingsActive);
    }

    public void LoadGameScene()
    {
        SceneManager.LoadScene("GameScene");
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
