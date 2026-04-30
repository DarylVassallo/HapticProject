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

//This script controls all the options in the Pause Menu
public class MenuManager : MonoBehaviour
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
    private TMP_Text _scoreUI;

    public static event Action<bool> OnToggleAll;

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
    }

    private void OnEnable()
    {
        PCPlayerInputManager.OnCancel += TogglePauseMenu;
        Health.OnGameOver += ToggleGameOverMenu;
        WinPlatform.OnWinGame += ToggleWinGameMenu;
        PlayerProfileManager.OnUpdateScore += UpdateUIScore;
    }

    private void OnDisable()
    {
        PCPlayerInputManager.OnCancel -= TogglePauseMenu;
        Health.OnGameOver -= ToggleGameOverMenu;
        WinPlatform.OnWinGame -= ToggleWinGameMenu;
        PlayerProfileManager.OnUpdateScore -= UpdateUIScore;
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
            Debug.LogWarning("Using default language");
        }
    }

    void ChangeLanguage(Locale targetLocale)
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

    public void ToggleGameOverMenu()
    {
        _showGameOverMenu = !_showGameOverMenu;

        if(_showGameOverMenu)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
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
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

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
    
    public void PlayLevel(string _sceneName)
    {
        SceneManager.LoadScene(_sceneName);
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
