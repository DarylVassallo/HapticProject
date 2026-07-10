using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System;

using System.Collections;
using UnityEngine.Localization;
using UnityEngine.Localization.Components;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;

using UnityEngine.InputSystem;

using Unity.Netcode;

using System.IO;

//This script controls all the options in the Pause Menu
public class MenuManager : NetworkBehaviour
{
    [SerializeField] private GameObject pauseMenu;
    private bool _showPauseMenu;

    [SerializeField] private GameObject settingsMenu;
    private bool _showSettingsMenu;

    [SerializeField] private GameObject gameOverMenu;
    private bool _showGameOverMenu;

    [SerializeField] private GameObject winGameMenu;
    private bool _showWinGameMenu;

    [SerializeField] private GameObject pcPlayerUI;
    [SerializeField] private Transform pcHealthBar;
    private float _maxHealthBarLength;
    [SerializeField] private Transform pcChargeBar;
    private float _maxChargeBarLength;
    private TMP_Text _scoreUI;

    public static event Action<bool> OnToggleAll;

    private NetworkVariable<int> nextScene = new (-1);

    private void Awake()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        _showPauseMenu = true;
        TogglePauseMenu();
        
        _showSettingsMenu = true;
        ToggleSettingsMenu();

        _showGameOverMenu = true;
        ToggleGameOverMenu();

        _showWinGameMenu = true;
        ToggleWinGameMenu();

        _scoreUI = pcPlayerUI.GetComponentInChildren<TMP_Text>();

        _maxHealthBarLength = pcHealthBar.localScale.y;
        _maxChargeBarLength = pcChargeBar.localScale.y;
    }

    private void OnEnable()
    {
        PCPlayerInputManager.OnCancel += TogglePauseMenu;
        
        Health.OnGameOver += ToggleGameOverMenu;
        Health.OnChangeHealthBar += ChangeHealthBar;
        FlashlightCharge.OnChangeChargeBar += ChangeChargeBar;

        WinPlatform.OnWinGame += ToggleWinGameMenu;
        PlayerProfileManager.OnUpdateScore += UpdateUIScore;
    }

    private void OnDisable()
    {
        PCPlayerInputManager.OnCancel -= TogglePauseMenu;

        Health.OnGameOver -= ToggleGameOverMenu;
        Health.OnChangeHealthBar -= ChangeHealthBar;
        FlashlightCharge.OnChangeChargeBar -= ChangeChargeBar;

        WinPlatform.OnWinGame -= ToggleWinGameMenu;
        PlayerProfileManager.OnUpdateScore -= UpdateUIScore;
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

    private void ChangeLanguage(Locale targetLocale)
    {
        LocalizationSettings.SelectedLocale = targetLocale;
        PlayerPrefs.SetString("SelectedLanguage", targetLocale.Identifier.Code);        
        PlayerPrefs.Save();
    }

    public void TogglePauseMenu()
    {
        _showPauseMenu = !_showPauseMenu;

        if (_showPauseMenu)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        for (int i = 0; i < pauseMenu.transform.childCount; i++)
        {
            if (pauseMenu.transform.GetChild(i).gameObject != null)  pauseMenu.transform.GetChild(i).gameObject.SetActive(_showPauseMenu);
        }

        CheckTimeScale();
    }

    public void ToggleSettingsMenu()
    {
        _showSettingsMenu = !_showSettingsMenu;

        if (_showSettingsMenu)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        for (int i = 0; i < settingsMenu.transform.childCount; i++)
        {
            if (settingsMenu.transform.GetChild(i).gameObject != null)  settingsMenu.transform.GetChild(i).gameObject.SetActive(_showSettingsMenu);
        }

        CheckTimeScale();
    }

    private void ChangeHealthBar(float _currentHealth)
    {
        ChangeBar(pcHealthBar, _maxHealthBarLength, _currentHealth / 100f);
    }

    private void ChangeChargeBar(float _currentCharge)
    {
       ChangeBar(pcChargeBar, _maxChargeBarLength, _currentCharge / 100f);
    }
    
    private void ChangeBar(Transform _bar, float _maxBarLength, float _newValue)
    {
        _bar.localScale = new Vector3(_maxBarLength * _newValue, _bar.localScale.y, _bar.localScale.z);
    }
    
    [Rpc(SendTo.Everyone, RequireOwnership = false)]
    public void ToggleGameOverMenuRpc()
    {
        ToggleGameOverMenu();
    }

    public void ToggleGameOverMenu()
    {
        _showGameOverMenu = !_showGameOverMenu;

        if(_showGameOverMenu)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            // PlayLevelServerRpc("GameOverScene");
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        for (int i = 0; i < gameOverMenu.transform.childCount; i++)
        {
            if (gameOverMenu.transform.GetChild(i).gameObject != null)  gameOverMenu.transform.GetChild(i).gameObject.SetActive(_showGameOverMenu);
        }

        CheckTimeScale();
    }

    public void ToggleWinGameMenu()
    {
        _showWinGameMenu = !_showWinGameMenu;

        if(_showWinGameMenu)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            for (int i = 0; i < winGameMenu.transform.childCount; i++)
            {
                if (winGameMenu.transform.GetChild(i).gameObject != null)
                {
                    if (winGameMenu.transform.GetChild(i).GetComponentInChildren<TMP_Text>() != null && winGameMenu.transform.GetChild(i).GetComponentInChildren<TMP_Text>().text == $"You Win")
                    {
                        var localized = winGameMenu.transform.GetChild(i)
                                .GetChild(0)
                                .GetComponentInChildren<LocalizeStringEvent>()
                                .StringReference;

                        localized.Arguments = new object[] 
                        { 
                            new { score = PlayerProfileManager.GetScore(0) } 
                        };

                        localized.RefreshString();
                    }
                    winGameMenu.transform.GetChild(i).gameObject.SetActive(_showWinGameMenu);
                }  
            }
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        CheckTimeScale();
    }

    private void CheckTimeScale()
    {
        if (_showPauseMenu || _showSettingsMenu || _showGameOverMenu || _showWinGameMenu)
        {
            FreezeGame();
        }
        else
        {
            ResumeGame();
        }
    }
    public void UpdateUIScore()
    {
        _scoreUI.text = $"{PlayerProfileManager.GetScore(0)}";
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

    private void FreezeGame()
    {
        OnToggleAll?.Invoke(false);
        AudioListener.volume = 0;
        Time.timeScale = 0;
    }

    private void ResumeGame()
    {
        OnToggleAll?.Invoke(true);
        AudioListener.volume = 1;
        Time.timeScale = 1;
    }
}
