using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public class MenuManager : MonoBehaviour
{
    [Header("Language Dropdown")]
    [SerializeField] private TMP_Dropdown languageDropdown;
    [SerializeField] private List<Locale> locales; // SAME ORDER as dropdown options

    IEnumerator Start()
    {
        yield return LocalizationSettings.InitializationOperation;

        LoadSavedLanguage();

        languageDropdown.onValueChanged.AddListener(OnLanguageDropdownChanged);
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

                // Sync dropdown UI
                int dropdownIndex = locales.IndexOf(savedLocale);
                if (dropdownIndex >= 0)
                    languageDropdown.SetValueWithoutNotify(dropdownIndex);

                return;
            }
        }

        // Fallback to system language
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

    public void LoadGameScene()
    {
        SceneManager.LoadScene("GameScene");
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
