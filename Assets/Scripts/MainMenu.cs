using UnityEngine;
using UnityEngine.SceneManagement;

using System.Collections;
using UnityEngine.Localization;
using UnityEngine.Localization.Components;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;

using System.IO;
using TMPro;

using Unity.Netcode;

//This script controls all the options in the Main Menu
public class MainMenu : NetworkBehaviour
{
    [SerializeField] private GameObject hostingOptionsMenu;
    [SerializeField] private GameObject multiplayerHostingMenu;
    [SerializeField] private GameObject mainMenu;
    [SerializeField] private GameObject settingsMenu;
    [SerializeField] private GameObject creditsMenu;
    private Transform creditsPage;

    [SerializeField] private string creditsPath = "Assets/Credits.txt";

    private bool isCreditsScrolling;

    private NetworkVariable<int> nextScene = new (-1);

    private void Awake()
    {
        hostingOptionsMenu.SetActive(true);
        multiplayerHostingMenu.SetActive(false);
        mainMenu.SetActive(false);
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

    public override void OnNetworkSpawn()
    {
        nextScene.OnValueChanged += OnNextSceneChanged;
    }

    private void OnNetworkDespawn()
    {
        nextScene.OnValueChanged -= OnNextSceneChanged;
    }

    private void OnNextSceneChanged(int previousValue, int newValue)
    {
        PlayLevelServerRpc(Path.GetFileNameWithoutExtension(SceneUtility.GetScenePathByBuildIndex(newValue)));
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

    private int GetSceneIndex(string sceneName)
    {
        for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
        {
            string path = SceneUtility.GetScenePathByBuildIndex(i);
            string name = Path.GetFileNameWithoutExtension(path);

            if (name == sceneName) return i;
        }

        return -1;
    }
    
    public void PlayLevelClient(string _sceneName)
    {
        SetNextSceneServerRpc(GetSceneIndex(_sceneName));
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetNextSceneServerRpc(int _newScene)
    {
        nextScene.Value = _newScene;
    }

    [ServerRpc(RequireOwnership = false)]
    public void PlayLevelServerRpc(string _sceneName)
    {
        if (!NetworkManager.Singleton.IsServer) return;

        foreach (var netObj in FindObjectsOfType<NetworkObject>())
        {
            if (netObj.IsSpawned)
            {
                netObj.Despawn(true);
            }
        }

        NetworkManager.Singleton.SceneManager.LoadScene(_sceneName, LoadSceneMode.Single);
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
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (_menuNum == 0)
        {
            hostingOptionsMenu.SetActive(true);
            multiplayerHostingMenu.SetActive(false);

            mainMenu.SetActive(false);

            settingsMenu.SetActive(false);

            creditsMenu.SetActive(false);
            isCreditsScrolling = false;
        }else if (_menuNum == 1)
        {
            hostingOptionsMenu.SetActive(false);
            multiplayerHostingMenu.SetActive(true);

            mainMenu.SetActive(false);

            settingsMenu.SetActive(false);

            creditsMenu.SetActive(false);
            isCreditsScrolling = false;
        }else if (_menuNum == 2)
        {
            hostingOptionsMenu.SetActive(false);
            multiplayerHostingMenu.SetActive(false);

            mainMenu.SetActive(true);

            settingsMenu.SetActive(false);

            creditsMenu.SetActive(false);
            isCreditsScrolling = false;
        }
        else if (_menuNum == 3)
        {
            hostingOptionsMenu.SetActive(false);
            multiplayerHostingMenu.SetActive(false);

            mainMenu.SetActive(false);

            settingsMenu.SetActive(true);

            creditsMenu.SetActive(false);
            isCreditsScrolling = false;
        }else if (_menuNum == 4)
        {
            hostingOptionsMenu.SetActive(false);
            multiplayerHostingMenu.SetActive(false);

            mainMenu.SetActive(false);

            settingsMenu.SetActive(false);

            creditsMenu.SetActive(true);
            isCreditsScrolling = true;
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}
