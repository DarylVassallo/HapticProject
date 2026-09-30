using UnityEngine;

using System.Collections;

using UnityEngine.Localization;
using UnityEngine.Localization.Components;
using UnityEngine.Localization.Settings;

using UnityEngine.UI;

using Unity.Netcode;

public class LanguageManager : NetworkBehaviour
{
    [SerializeField] Locale engLocale;
    [SerializeField] Locale frLocale;
    
    [System.Serializable]
    private struct LanguageButton
    {
        public string languageName;
        public Button button;
        public Locale locale;
    }

    [System.Serializable]
    private struct LanguageParentObject
    {
        public string languageName;
        public GameObject[] parentObjects;
    }

    [SerializeField] private LanguageButton[] languageButtons;
    [SerializeField] private LanguageParentObject[] languageParentObject;

    IEnumerator Start()
    {
        yield return LocalizationSettings.InitializationOperation;

        foreach (var langBtn in languageButtons)
        {
            langBtn.button.onClick.AddListener(() => ChangeLanguage(langBtn.locale));
        }
    }

    private void OnEnable()
    {
        EventsManager.OnChangeLanguageToEnglish += ChangeLanguageToEnglish;
        EventsManager.OnChangeLanguageToFrench += ChangeLanguageToFrench;
    }

    private void OnDisable()
    {
        EventsManager.OnChangeLanguageToEnglish -= ChangeLanguageToEnglish;
        EventsManager.OnChangeLanguageToFrench -= ChangeLanguageToFrench;
    }

    public override void OnNetworkSpawn()
    {
        LoadSavedLanguage();
    }

    //This loads the chosen language, and applies it to the scene
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
                UpdatedSceneLanguage(savedLocale);
                return;
            }
        }

        Locale deviceLocale = LocalizationSettings.AvailableLocales.GetLocale(
            Application.systemLanguage
        );
        if (deviceLocale != null)
        {
            LocalizationSettings.SelectedLocale = deviceLocale;
            UpdatedSceneLanguage(deviceLocale);
        }
        else
        {
            LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[0];
            UpdatedSceneLanguage(LocalizationSettings.AvailableLocales.Locales[0]);
        }
    }

    private void ChangeLanguageToEnglish()
    {
        ChangeLanguage(engLocale);
    }

    private void ChangeLanguageToFrench()
    {
        ChangeLanguage(frLocale);
    }

    //Changes the language used for the scene
    private void ChangeLanguage(Locale targetLocale)
    {
        LocalizationSettings.SelectedLocale = targetLocale;
        UpdatedSceneLanguage(targetLocale);

        PlayerPrefs.SetString("SelectedLanguage", targetLocale.Identifier.Code);        
        PlayerPrefs.Save();
    }

    private void UpdatedSceneLanguage(Locale targetLocale)
    {
        foreach (var langBtn in languageButtons)
        {
            if (targetLocale == langBtn.locale)
            {
                if(langBtn.languageName == "English") EventsManager.UseEnglishNarrator();
                if(langBtn.languageName == "French") EventsManager.UseFrenchNarrator();

                foreach (var langParObj in languageParentObject)
                {
                    if(langBtn.languageName == langParObj.languageName)
                    {
                        foreach (var parObj in langParObj.parentObjects)
                        {
                            for (int i = 0; i < parObj.transform.childCount; i++)
                            {
                                parObj.transform.GetChild(i).gameObject.SetActive(true);
                                EventsManager.AddSpecificHiddenObject(parObj.transform.GetChild(i).gameObject);
                            }
                        }
                    }
                    else
                    {
                        foreach (var parObj in langParObj.parentObjects)
                        {
                            for (int i = 0; i < parObj.transform.childCount; i++)
                            {
                                EventsManager.RemoveHiddenObject(parObj.transform.GetChild(i).gameObject);
                                parObj.transform.GetChild(i).gameObject.SetActive(false);
                            }
                        }
                    }
                }
            }
        }
    }
}
