using UnityEngine;
using UnityEngine.SceneManagement;

using System.Collections;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;

using System.IO;
using TMPro;

using UnityEngine.Localization;
using UnityEngine.Localization.Components;
using UnityEngine.Localization.Settings;

//This script controls all the options in the Main Menu
public class MainMenu : MonoBehaviour
{
    [SerializeField] private GameObject mainMenu;
    [SerializeField] private GameObject settingsMenu;
    [SerializeField] private GameObject creditsMenu;
    private Transform creditsPage;

    [SerializeField] private string creditsPath = "Assets/Credits.txt";

    private bool isCreditsScrolling;

    private void Awake()
    {
        mainMenu.SetActive(true);
        settingsMenu.SetActive(false);

        foreach (Transform child in creditsMenu.transform)
        {
            if (child.GetComponent<Button>() == null)
            {
                creditsPage = child;
                break;
            }
        }
        creditsMenu.SetActive(false);

        isCreditsScrolling = false;
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

    private void FixedUpdate()
    {
        if (isCreditsScrolling)
        {
            creditsPage.position = new Vector2( creditsPage.position.x,
                                                creditsPage.position.y + 0.3f);  
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

    public void ShowCredits()
    {
        creditsPage.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 0);

        StreamReader reader = new StreamReader(creditsPath); 
        int rowNum = 0;

        string line;
        string entry;
        GameObject newEntry;
        TextMeshProUGUI newEntryText;
        LocalizeStringEvent newEntryLocalizeEvent;

        int fontSize;
        Color entryColor;
        while(!reader.EndOfStream)
        {
            line = reader.ReadLine();

            if(line[0] == '!')
            {
                entry = line.Substring(1);
                fontSize = 30;   
                entryColor = Color.red;   
            }
            else
            {
                entry = line;
                fontSize = 20;  
                entryColor = Color.white;  
            }

            newEntry = new GameObject("Entry_" + entry);
            newEntry.transform.SetParent(creditsPage);

            newEntryText = newEntry.AddComponent<TextMeshProUGUI>();
            newEntryText.text = entry;
            newEntryText.fontSize = fontSize; 
            newEntryText.alignment = TextAlignmentOptions.Top;
            newEntryText.color = entryColor; 

            newEntryLocalizeEvent = newEntry.AddComponent<LocalizeStringEvent>();
            newEntryLocalizeEvent.OnUpdateString.AddListener(newText =>
            {
                if(!newText.Contains("No translation"))
                {
                    newEntryText.text = newText;
                }
            });
            newEntryLocalizeEvent.StringReference.SetReference("Languages", entry);

            RectTransform rect = newEntryText.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(600, 100);
            rect.anchoredPosition = new Vector2(0, 100 - (140 * rowNum)); 
            rowNum++; 
        }
        reader.Close();
    }

    public void Quit()
    {
        Application.Quit();
    }

    public void ChangeMenu(int _menuNum)
    {
        if (_menuNum == 0)
        {
            mainMenu.SetActive(true);
            settingsMenu.SetActive(false);
            creditsMenu.SetActive(false);
            isCreditsScrolling = false;
        }
        else if (_menuNum == 1)
        {
            mainMenu.SetActive(false);
            settingsMenu.SetActive(true);
            creditsMenu.SetActive(false);
            isCreditsScrolling = false;
        }else if (_menuNum == 2)
        {
            mainMenu.SetActive(false);
            settingsMenu.SetActive(false);
            creditsMenu.SetActive(true);
            isCreditsScrolling = true;
        }
    }
}
