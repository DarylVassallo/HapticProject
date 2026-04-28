using UnityEngine;
using UnityEngine.SceneManagement;

using System.Collections;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;

//This script controls all the options in the Main Menu
public class MainMenu : MonoBehaviour
{
    [SerializeField] private GameObject mainMenu;
    [SerializeField] private GameObject settingsMenu;

    private void Awake()
    {
        mainMenu.SetActive(true);
        settingsMenu.SetActive(false);
    }

    [System.Serializable]
    public struct LanguageButton
    {
        public Button button;
        public Locale locale;
    }
    public LanguageButton[] languageButtons;

    IEnumerator Start()
    {
        yield return LocalizationSettings.InitializationOperation;

        LoadSavedLanguage();

        foreach (var langBtn in languageButtons)
        {
            langBtn.button.onClick.AddListener(() => ChangeLanguage(langBtn.locale));
        }
    }

    private void LoadSavedLanguage()
    {
        string savedLangCode = PlayerPrefs.GetString("SelectedLanguage", "");

        if (!string.IsNullOrEmpty(savedLangCode))
        {
            Locale savedLocale = LocalizationSettings.AvailableLocales.GetLocale(
                new LocaleIdentifier(savedLangCode)
            );
            if (savedLocale != null)
            {
                LocalizationSettings.SelectedLocale = savedLocale;
                Debug.Log("Loaded saved language: " + savedLangCode);
                return;
            }
        }

        Locale deviceLocale = LocalizationSettings.AvailableLocales.GetLocale(
            Application.systemLanguage
        );
        if (deviceLocale != null)
        {
            LocalizationSettings.SelectedLocale = deviceLocale;
            Debug.Log("Using device language: " + Application.systemLanguage);
        }
        else
        {
            LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[0];
            Debug.LogWarning("Using default language");
        }
    }

    void ChangeLanguage(Locale targetLocale)
    {
        LocalizationSettings.SelectedLocale = targetLocale;
        PlayerPrefs.SetString("SelectedLanguage", targetLocale.Identifier.Code);
        PlayerPrefs.Save();
        Debug.Log("Language saved: " + targetLocale.Identifier.Code);
    }

    public void PlayLevel(string _sceneName)
    {
        SceneManager.LoadScene(_sceneName);
    }

    public void Quit()
    {
        Application.Quit();
    }

    public void ChangeMenu(bool showMainMenu)
    {
        if (showMainMenu)
        {
            mainMenu.SetActive(true);
            settingsMenu.SetActive(false);
        }
        else
        {
            mainMenu.SetActive(false);
            settingsMenu.SetActive(true);
        }
    }
}
