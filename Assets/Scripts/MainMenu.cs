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
    [SerializeField] private TextMeshProUGUI ipText;
    [SerializeField] private GameObject[] menuList;
    [SerializeField] private GameObject[] vrMenuList;

    [SerializeField] private Transform pcCreditsPage;
    [SerializeField] private Transform vrCreditsPage;

    [SerializeField] private string creditsPath = "Assets/Credits.txt";

    private bool isCreditsScrolling;

    private NetworkVariable<int> nextScene = new (-1);
    private NetworkVariable<int> menuValue = new (-1);

    private void Awake()
    {
        Debug.Log("MainMenu Awake");

        for (int i = 0; i < menuList.Length; i++)
        {
            if (i == 0)
            {
                menuList[i].SetActive(true);
                vrMenuList[i].SetActive(true);
            }
            else
            {
                menuList[i].SetActive(false);
                vrMenuList[i].SetActive(false);
            }
        }
    }

    public override void OnNetworkSpawn()
    {
        Debug.Log("MainMenu OnNetworkSpawn");

        nextScene.OnValueChanged += OnNextSceneChanged;
        menuValue.OnValueChanged += OnMenuValueChanged;
    }

    private void OnNetworkDespawn()
    {
        Debug.Log("MainMenu OnNetworkDespawn");

        nextScene.OnValueChanged -= OnNextSceneChanged;
        menuValue.OnValueChanged -= OnMenuValueChanged;
    }

    private void OnNextSceneChanged(int previousValue, int newValue)
    {
        Debug.Log("OnNextSceneChanged: " + newValue);

        PlayLevelServerRpc(Path.GetFileNameWithoutExtension(SceneUtility.GetScenePathByBuildIndex(newValue)));
    }

    private void OnMenuValueChanged(int previousValue, int newValue)
    {
        Debug.Log("OnMenuValueChanged: " + newValue);

        SetCurrentMenuNumber(newValue);
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
        Debug.Log("MainMenu Start");

        yield return LocalizationSettings.InitializationOperation;

        LoadSavedLanguage();

        // foreach (var langBtn in languageButtons)
        // {
        //     langBtn.button.onClick.AddListener(() => ChangeLanguage(langBtn.locale));
        // }
    }

    private void FixedUpdate()
    {
        if (isCreditsScrolling)
        {
            Debug.Log("MainMenu FixedUpdate");

            pcCreditsPage.position = new Vector2(   pcCreditsPage.position.x,
                                                    pcCreditsPage.position.y + 0.3f); 
                                                
            vrCreditsPage.position = new Vector3(   vrCreditsPage.position.x,
                                                    vrCreditsPage.position.y + 0.01f,
                                                    vrCreditsPage.position.z);  
        }
    }
    private void LoadSavedLanguage()
    {
        Debug.Log("MainMenu LoadSavedLanguage");

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

    public void ChangeLanguage(Locale targetLocale)
    {
        Debug.Log("MainMenu ChangeLanguage: " + targetLocale);

        LocalizationSettings.SelectedLocale = targetLocale;
        PlayerPrefs.SetString("SelectedLanguage", targetLocale.Identifier.Code);
        PlayerPrefs.Save();
        Debug.Log("Language saved: " + targetLocale.Identifier.Code);
    }

    private int GetSceneIndex(string sceneName)
    {
        Debug.Log("MainMenu GetSceneIndex: " + sceneName);

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
        Debug.Log("PlayLevelClient: " + _sceneName);

        SetNextSceneServerRpc(GetSceneIndex(_sceneName));
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetNextSceneServerRpc(int _newScene)
    {
        Debug.Log("SetNextSceneServerRpc: " + _newScene);

        nextScene.Value = _newScene;
    }

    [ServerRpc(RequireOwnership = false)]
    private void PlayLevelServerRpc(string _sceneName)
    {
        Debug.Log("PlayLevelServerRpc: " + _sceneName);

        if (!NetworkManager.Singleton.IsServer) return;

        // foreach (var netObj in FindObjectsOfType<NetworkObject>())
        // {
        //     if (netObj.IsSpawned)
        //     {
        //         netObj.Despawn(true);
        //     }
        // }

        NetworkManager.Singleton.SceneManager.LoadScene(_sceneName, LoadSceneMode.Single);
    }

    private void ShowCredits()
    {
        Debug.Log("MainMenu ShowCredits");

        CreateCreditEntries(pcCreditsPage);
        CreateVRCreditEntries(vrCreditsPage);
    }

    public void CreateCreditEntries(Transform currentCreditsPage)
    {
        Debug.Log("MainMenu CreateCreditEntries: " + currentCreditsPage);

        currentCreditsPage.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 0);

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
                fontSize = 20;   
                entryColor = Color.red;   
            }
            else
            {
                entry = line;
                fontSize = 15;  
                entryColor = Color.white;  
            }

            newEntry = new GameObject("Entry_" + entry);
            newEntry.transform.SetParent(currentCreditsPage);

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

    public void CreateVRCreditEntries(Transform currentCreditsPage)
    {
        Debug.Log("MainMenu CreateVRCreditEntries: " + currentCreditsPage);

        currentCreditsPage.position = currentCreditsPage.parent.position;

        StreamReader reader = new StreamReader(creditsPath); 
        int rowNum = 0;

        string line;
        string entry;
        GameObject newEntry;
        TextMeshPro newEntryText;
        LocalizeStringEvent newEntryLocalizeEvent;

        int fontSize;
        Color entryColor;
        while(!reader.EndOfStream)
        {
            line = reader.ReadLine();

            if(line[0] == '!')
            {
                entry = line.Substring(1);
                fontSize = 20;   
                entryColor = Color.red;   
            }
            else
            {
                entry = line;
                fontSize = 15;  
                entryColor = Color.white;  
            }

            newEntry = new GameObject("Entry_" + entry);
            newEntry.transform.SetParent(currentCreditsPage);

            newEntryText = newEntry.AddComponent<TextMeshPro>();
            newEntryText.text = entry;
            newEntryText.fontSize = fontSize; 
            newEntryText.alignment = TextAlignmentOptions.Top;
            newEntryText.color = entryColor; 

            newEntryText.rectTransform.sizeDelta = new Vector3(100f, 5f);

            newEntryLocalizeEvent = newEntry.AddComponent<LocalizeStringEvent>();
            newEntryLocalizeEvent.OnUpdateString.AddListener(newText =>
            {
                if(!newText.Contains("No translation"))
                {
                    newEntryText.text = newText;
                }
            });
            newEntryLocalizeEvent.StringReference.SetReference("Languages", entry);

            newEntry.transform.localScale = Vector3.one;
            newEntry.transform.localPosition = new Vector3(0f, -9 * rowNum, 0f);

            // RectTransform rect = newEntryText.GetComponent<RectTransform>();
            // rect.sizeDelta = new Vector2(600, 100);
            // rect.position = new Vector2(0, 100 - (140 * rowNum)); 

            rowNum++; 
        }
        reader.Close();
    }

    public void Quit()
    {
        Debug.Log("MainMenu Quit");

        Application.Quit();
    }

// public void ActivateCreditsMenu()
// {
//     SetNextSceneServerRpc(GetSceneIndex(_sceneName));
// }

    public void SetMenuNumber(int _newMenuValue)
    {
        Debug.Log("MainMenu SetMenuNumber: " + _newMenuValue);

        SetMenuNumberValueServerRpc(_newMenuValue);
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetMenuNumberValueServerRpc(int _newMenuValue)
    {
        Debug.Log("MainMenu SetMenuNumberValueServerRpc: " + _newMenuValue);

        menuValue.Value = _newMenuValue;
    }

    private void SetCurrentMenuNumber(int _menuNum)
    {
        Debug.Log("MainMenu SetCurrentMenuNumber: " + _menuNum);
        
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        for (int i = 0; i < menuList.Length; i++)
        {
            if (_menuNum == i)
            {
                menuList[i].SetActive(true);
                vrMenuList[i].SetActive(true);

                if(_menuNum == menuList.Length - 1)
                {
                    isCreditsScrolling = true;
                    ShowCredits();
                }
            }
            else
            {
                menuList[i].SetActive(false);
                vrMenuList[i].SetActive(false);

                if(_menuNum == menuList.Length - 1) isCreditsScrolling = false;
            }
        }

        StartCoroutine(ReturnCursor());
    }

    public void SetMenuNumberWithoutNetwork(int _menuNum)
    {
        Debug.Log("MainMenu SetMenuNumberWithoutNetwork: " + _menuNum);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        for (int i = 0; i < menuList.Length; i++)
        {
            if (_menuNum == i)
            {
                menuList[i].SetActive(true);
                vrMenuList[i].SetActive(true);

                if(_menuNum == menuList.Length - 1) isCreditsScrolling = true;
            }
            else
            {
                menuList[i].SetActive(false);
                vrMenuList[i].SetActive(false);

                if(_menuNum == menuList.Length - 1) isCreditsScrolling = false;
            }
        }

        StartCoroutine(ReturnCursor());
    }

    private IEnumerator ReturnCursor()
    {
        Debug.Log("MainMenu ReturnCursor");

        yield return new WaitForSeconds(0.5f);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true; 
    }
}
