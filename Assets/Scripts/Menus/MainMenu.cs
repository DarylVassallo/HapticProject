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
    // [SerializeField] private GameObject temple;
    [SerializeField] private Material cameraCover;
    [SerializeField] private Material handMaterial;
    [SerializeField] private Transform vrCoverParent;
    private Renderer[] vrCovers;
    
    [SerializeField] private Camera vrCamera;
    [SerializeField] private Camera pcCamera;

    [SerializeField] private Locale engLocale;
    [SerializeField] private Locale frenchLocale;
    
    [SerializeField] private GameObject[] menuList;
    [SerializeField] private GameObject[] vrMenuList;

    [SerializeField] private Transform pcCreditsPage;
    [SerializeField] private Transform vrCreditsPage;

    // [SerializeField] private string creditsPath = "Assets/Credits.txt";
    [SerializeField] private TextAsset creditsFile;

    private bool isCreditsScrolling;

    private NetworkVariable<int> nextScene = new (-99);
    private NetworkVariable<int> menuValue = new (-99);

    private bool _chosePlayer;
    private bool _prevIsDeviceActive;
    private bool _currIsDeviceActive;
    
    private bool _changingScene;

    [SerializeField] private AudioSource _backgroundMusicSource;
    [SerializeField] private float _maxBackgroundVolume;

    private void Awake()
    {
        _changingScene = false;
        _chosePlayer = false;
        _currIsDeviceActive = UnityEngine.XR.XRSettings.isDeviceActive;
        if(_currIsDeviceActive)
        {
            vrCamera.depth = 1;
            pcCamera.depth = -1;
        }
        else
        {
            vrCamera.depth = -1;
            pcCamera.depth = 1;
        }
        _prevIsDeviceActive = _currIsDeviceActive;

        cameraCover.color = new Color(0, 0, 0, 0);
        handMaterial.color = new Color(0, 0, 0, 0);

        vrCovers = new Renderer[vrCoverParent.childCount];
        for (int i = 0; i < vrCoverParent.childCount; i++)
        {
            vrCovers[i] = vrCoverParent.GetChild(i).GetComponent<Renderer>();
            vrCovers[i].material.color = new Color(0, 0, 0, 1f * ((i + 1f) / vrCoverParent.childCount));
        } 

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

    void OnEnable()
    {
        EventsManager.OnChosePlayer += ChosePlayer;

        EventsManager.OnSetMenuWithoutNetwork += SetMenuNumberWithoutNetwork;
        EventsManager.OnSetMenu += SetMenuNumber;

        EventsManager.OnCreatedPCPlayer += RevealTempleServerRpc;
        EventsManager.OnRevealTemple += RevealTempleServerRpc;

        EventsManager.OnPlayLevel += PlayLevelClient;
        EventsManager.OnQuit += Quit;

        EventsManager.OnChangeLanguage += ChangeLanguageNum;

        EventsManager.OnReduceBackgroundMusic += ReduceBackgroundMusicRpc;
    }

    void OnDisable()
    {
        EventsManager.OnChosePlayer -= ChosePlayer;

        EventsManager.OnSetMenuWithoutNetwork -= SetMenuNumberWithoutNetwork;
        EventsManager.OnSetMenu -= SetMenuNumber;

        EventsManager.OnCreatedPCPlayer -= RevealTempleServerRpc;
        EventsManager.OnRevealTemple -= RevealTempleServerRpc;

        EventsManager.OnPlayLevel -= PlayLevelClient;
        EventsManager.OnQuit -= Quit;

        EventsManager.OnChangeLanguage -= ChangeLanguageNum;

        EventsManager.OnReduceBackgroundMusic -= ReduceBackgroundMusicRpc;
    }

    public void ChosePlayer()
    {
        _chosePlayer = true;
        SetupCamera();
    }

    private void SetupCamera()
    {       
        _currIsDeviceActive = UnityEngine.XR.XRSettings.isDeviceActive;

        if(_prevIsDeviceActive == _currIsDeviceActive) return;

        if(_currIsDeviceActive)
        {
            vrCamera.depth = 1;
            pcCamera.depth = -1;
        }
        else
        {
            vrCamera.depth = -1;
            pcCamera.depth = 1;
        }

        _prevIsDeviceActive = _currIsDeviceActive;
    }

    [Rpc(SendTo.Everyone, InvokePermission = RpcInvokePermission.Everyone)]
    public void RevealTempleServerRpc()
    {
        SetMenuNumberValueServerRpc(1);
        StartCoroutine(ShiftRevealTemple(4f));
    }

    IEnumerator ShiftRevealTemple(float _delay)
    {
        float elapsed = 0f;
        while(elapsed < _delay)
        {
            elapsed += Time.deltaTime;
            for (int i = 0; i < vrCoverParent.childCount; i++)
            {
                vrCovers[i].material.color = new Color(0, 0, 0, (1f - (elapsed / _delay)) * ((i + 1f) / vrCoverParent.childCount));
            } 

            yield return null;
        }

        _backgroundMusicSource.Stop();
        StartCoroutine(ChangeBackgroundMusicVolume(4f, 0f, _maxBackgroundVolume));
    }

    [Rpc(SendTo.Everyone, InvokePermission = RpcInvokePermission.Everyone)]
    private void ReduceBackgroundMusicRpc(float _reducedAmount)
    {
        StartCoroutine(ChangeBackgroundMusicVolume(2f, _backgroundMusicSource.volume, _backgroundMusicSource.volume * _reducedAmount));
    }

    IEnumerator ChangeBackgroundMusicVolume(float _delay, float _currentVolume, float _requiredVolume)
    {
        _backgroundMusicSource.volume = _currentVolume;
        _backgroundMusicSource.Play();

        float _elapsed = 0f;

        while(_elapsed < _delay)
        {
            _elapsed += Time.deltaTime;
            _backgroundMusicSource.volume = Mathf.Lerp(_currentVolume, _requiredVolume, (_elapsed / _delay)); 
            yield return null;
        }

        _backgroundMusicSource.volume = _requiredVolume;
    }

    public override void OnNetworkSpawn()
    {
        nextScene.OnValueChanged += OnNextSceneChanged;
        menuValue.OnValueChanged += OnMenuValueChanged;
    }

    public override void OnNetworkDespawn()
    {
        nextScene.OnValueChanged -= OnNextSceneChanged;
        menuValue.OnValueChanged -= OnMenuValueChanged;
    }

    private void OnNextSceneChanged(int previousValue, int newValue)
    {
        PlayLevelServerRpc(Path.GetFileNameWithoutExtension(SceneUtility.GetScenePathByBuildIndex(newValue)));
    }

    private void OnMenuValueChanged(int previousValue, int newValue)
    {
        SetCurrentMenuNumber(newValue);
    }

    [System.Serializable]
    private struct LanguageButton
    {
        public Button button;
        public Locale locale;
    }
    private LanguageButton[] languageButtons;

    IEnumerator Start()
    {
        yield return LocalizationSettings.InitializationOperation;

        LoadSavedLanguage();
    }

    private void FixedUpdate()
    {
        if(!_chosePlayer) SetupCamera();

        //If required, this causes the credits page for both players to slowly move upwards
        if (isCreditsScrolling)
        {
            pcCreditsPage.position = new Vector2(   pcCreditsPage.position.x,
                                                    pcCreditsPage.position.y + 0.3f); 
                                                
            vrCreditsPage.position = new Vector3(   vrCreditsPage.position.x,
                                                    vrCreditsPage.position.y + 0.01f,
                                                    vrCreditsPage.position.z);  
        }
    }

    //Loads the chosen language, to be used in the game
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
                return;
            }
        }

        Locale deviceLocale = LocalizationSettings.AvailableLocales.GetLocale(
            Application.systemLanguage
        );
        if (deviceLocale != null)
        {
            LocalizationSettings.SelectedLocale = deviceLocale;
        }
        else
        {
            LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[0];
        }
    }


    private void ChangeLanguageNum(int languageNum)
    {
        switch(languageNum)
        {
            case 0:
                ChangeLanguage(engLocale);
                break;
            case 1:
                ChangeLanguage(frenchLocale);
                break;
        }
    }

    //Used by UI button to change the language used
    public void ChangeLanguage(Locale targetLocale)
    {
        LocalizationSettings.SelectedLocale = targetLocale;
        PlayerPrefs.SetString("SelectedLanguage", targetLocale.Identifier.Code);
        PlayerPrefs.Save();
    }

    //Gets the correct scene number
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
    
    //Used by UI Button to change the scene
    public void PlayLevelClient(string _sceneName)
    {
        if(_changingScene) return;

        _changingScene = true;    
        StartCoroutine(MoveToScene(_sceneName, 1f));
    }

    IEnumerator MoveToScene(string _sceneName, float _delay)
    {
        float elapsed = 0f;
        while(elapsed < _delay)
        {
            elapsed += Time.deltaTime;
            cameraCover.color = new Color(0, 0, 0, elapsed / _delay);

            yield return null;
        }

        cameraCover.color = new Color(0, 0, 0, 1);
        SetNextSceneServerRpc(GetSceneIndex(_sceneName));
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    private void SetNextSceneServerRpc(int _newScene)
    {
        nextScene.Value = _newScene;
    }

    //Loads the correct scene
    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    private void PlayLevelServerRpc(string _sceneName)
    {
        if (!NetworkManager.Singleton.IsServer) return;
        
        cameraCover.color = new Color(0, 0, 0, 1);
        NetworkManager.Singleton.SceneManager.LoadScene(_sceneName, LoadSceneMode.Single);
    }

    //Shows the credits page for both players
    private void ShowCredits()
    {
        CreateCreditEntries(pcCreditsPage);
        CreateVRCreditEntries(vrCreditsPage);
    }

    //Creates a credit page for the PC Player, which includes titles and entries with specific formatting
    private void CreateCreditEntries(Transform currentCreditsPage)
    {
        currentCreditsPage.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 0);

        StringReader reader = new StringReader(creditsFile.text);
        // StreamReader reader = new StreamReader(creditsPath); 
        int rowNum = 0;

        string line;
        string entry;
        GameObject newEntry;
        TextMeshProUGUI newEntryText;
        LocalizeStringEvent newEntryLocalizeEvent;

        int fontSize;
        Color entryColor;

        while((line = reader.ReadLine()) != null)
        {
            // line = reader.ReadLine();

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

    //Creates a credit page for the VR Player, which includes titles and entries with specific formatting
    private void CreateVRCreditEntries(Transform currentCreditsPage)
    {
        currentCreditsPage.position = currentCreditsPage.parent.position;

        StringReader reader = new StringReader(creditsFile.text);
        // StreamReader reader = new StreamReader(creditsPath); 
        int rowNum = 0;

        string line;
        string entry;
        GameObject newEntry;
        TextMeshPro newEntryText;
        LocalizeStringEvent newEntryLocalizeEvent;

        int fontSize;
        Color entryColor;
        while((line = reader.ReadLine()) != null)
        {
            // line = reader.ReadLine();

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

    //Used by UI button to end the game
    public void Quit()
    {
        Application.Quit();
    }

    //Used by UI button to change the visible menu
    public void SetMenuNumber(int _newMenuValue)
    {
        SetMenuNumberValueServerRpc(_newMenuValue);
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    private void SetMenuNumberValueServerRpc(int _newMenuValue)
    {
        menuValue.Value = _newMenuValue;
    }

    //Shows the chosen menu for both players
    private void SetCurrentMenuNumber(int _menuNum)
    {     
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

    //Used by UI button to change the visible menu
    public void SetMenuNumberWithoutNetwork(int _menuNum)
    {
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

    //Activate the cursor after a delay
    private IEnumerator ReturnCursor()
    {
        yield return new WaitForSeconds(1f);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true; 
    }
}
