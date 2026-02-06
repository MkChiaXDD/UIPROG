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
    [Header("Language Dropdown")]
    [SerializeField] private TMP_Dropdown languageDropdown;
    [SerializeField] private List<Locale> locales;

    [Header("Settings Tabs")]
    [SerializeField] private Button AudioBtn;
    [SerializeField] private Button VideoBtn;

    [Header("Map Rotation Buttons")]
    [SerializeField] private Button fixedBtn;
    [SerializeField] private Button rotateBtn;

    [Header("Crosshair Buttons")]
    [SerializeField] private Button crosshairBtn1;
    [SerializeField] private Button crosshairBtn2;

    [Header("Crosshair Visual")]
    [SerializeField] private Image crosshairImage;
    [SerializeField] private Sprite crosshairSprite1;
    [SerializeField] private Sprite crosshairSprite2;

    [Header("Button Colors")]
    [SerializeField] private Color selectedColor = new Color(0.6f, 0.6f, 0.6f, 1f);
    [SerializeField] private Color unselectedColor = Color.white;

    [Header("Settings")]
    [SerializeField] private GameObject settingsCanvas;
    private bool isSettingsActive;

    IEnumerator Start()
    {
        yield return LocalizationSettings.InitializationOperation;

        LoadSavedLanguage();
        languageDropdown.onValueChanged.AddListener(OnLanguageDropdownChanged);
    }

    private void Awake()
    {
        // Map rotation pair
        if (fixedBtn) fixedBtn.onClick.AddListener(OnFixedClicked);
        if (rotateBtn) rotateBtn.onClick.AddListener(OnRotateClicked);
        SetMapRotationSelected(isFixed: true);

        // Audio/Video pair
        if (AudioBtn) AudioBtn.onClick.AddListener(OnAudioClicked);
        if (VideoBtn) VideoBtn.onClick.AddListener(OnVideoClicked);
        SetSettingsTabSelected(isAudio: true);

        // Crosshair pair
        if (crosshairBtn1) crosshairBtn1.onClick.AddListener(OnCrosshair1Clicked);
        if (crosshairBtn2) crosshairBtn2.onClick.AddListener(OnCrosshair2Clicked);

        // default crosshair
        SetCrosshairSelected(selectFirst: true);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            ToggleSettings();
    }

    void OnLanguageDropdownChanged(int index)
    {
        if (index < 0 || index >= locales.Count) return;
        ChangeLanguage(locales[index]);
    }

    void LoadSavedLanguage()
    {
        string savedLangCode = PlayerPrefs.GetString("SelectedLanguage", "");

        if (!string.IsNullOrEmpty(savedLangCode))
        {
            Locale savedLocale = LocalizationSettings.AvailableLocales
                .GetLocale(new LocaleIdentifier(savedLangCode));

            if (savedLocale != null)
            {
                LocalizationSettings.SelectedLocale = savedLocale;

                int dropdownIndex = locales.IndexOf(savedLocale);
                if (dropdownIndex >= 0)
                    languageDropdown.SetValueWithoutNotify(dropdownIndex);

                return;
            }
        }

        Locale deviceLocale = LocalizationSettings.AvailableLocales
            .GetLocale(Application.systemLanguage);

        if (deviceLocale != null)
        {
            LocalizationSettings.SelectedLocale = deviceLocale;

            int dropdownIndex = locales.IndexOf(deviceLocale);
            if (dropdownIndex >= 0)
                languageDropdown.SetValueWithoutNotify(dropdownIndex);
        }
        else
        {
            LocalizationSettings.SelectedLocale =
                LocalizationSettings.AvailableLocales.Locales[0];
        }
    }

    void ChangeLanguage(Locale targetLocale)
    {
        LocalizationSettings.SelectedLocale = targetLocale;
        PlayerPrefs.SetString("SelectedLanguage", targetLocale.Identifier.Code);
        PlayerPrefs.Save();

        Debug.Log("Language Saved: " + targetLocale.Identifier.Code);
    }

    // -------------------------
    // Map Rotation Buttons
    // -------------------------
    private void OnFixedClicked()
    {
        SetMapRotationSelected(isFixed: true);
        // your fixed mode logic here
    }

    private void OnRotateClicked()
    {
        SetMapRotationSelected(isFixed: false);
        // your rotate mode logic here
    }

    private void SetMapRotationSelected(bool isFixed)
    {
        ApplyTogglePairVisual(fixedBtn, rotateBtn, selectFirst: isFixed);
    }

    // -------------------------
    // Audio / Video Buttons
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
    }

    // -------------------------
    // Crosshair Buttons
    // -------------------------
    private void OnCrosshair1Clicked()
    {
        SetCrosshairSelected(selectFirst: true);
    }

    private void OnCrosshair2Clicked()
    {
        SetCrosshairSelected(selectFirst: false);
    }

    private void SetCrosshairSelected(bool selectFirst)
    {
        ApplyTogglePairVisual(crosshairBtn1, crosshairBtn2, selectFirst);

        if (!crosshairImage) return;

        crosshairImage.sprite = selectFirst ? crosshairSprite1 : crosshairSprite2;
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
