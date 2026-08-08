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

    [SerializeField] private GameObject pcPlayerUI;
    [SerializeField] private Transform pcHealthBar;
    private float _maxHealthBarLength;
    [SerializeField] private Transform pcChargeBar;
    private float _maxChargeBarLength;
    private TMP_Text _scoreUI;

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

        _scoreUI = pcPlayerUI.GetComponentInChildren<TMP_Text>();

        _maxHealthBarLength = pcHealthBar.localScale.y;
        _maxChargeBarLength = pcChargeBar.localScale.y;
    }

    private void OnEnable()
    {
        EventsManager.OnCancel += TogglePauseMenu;
        
        EventsManager.OnGameOver += ToggleGameOverMenu;
        EventsManager.OnChangeHealthBar += ChangeHealthBar;
        EventsManager.OnChangeChargeBar += ChangeChargeBar;
    }

    private void OnDisable()
    {
        EventsManager.OnCancel -= TogglePauseMenu;

        EventsManager.OnGameOver -= ToggleGameOverMenu;
        EventsManager.OnChangeHealthBar -= ChangeHealthBar;
        EventsManager.OnChangeChargeBar -= ChangeChargeBar;
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

        foreach (var langBtn in languageButtons)
        {
            langBtn.button.onClick.AddListener(() => ChangeLanguage(langBtn.locale));
        }
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

    //Changes the language used for the scene
    private void ChangeLanguage(Locale targetLocale)
    {
        LocalizationSettings.SelectedLocale = targetLocale;
        PlayerPrefs.SetString("SelectedLanguage", targetLocale.Identifier.Code);        
        PlayerPrefs.Save();
    }

    //This toggles the pause menu, and pausing the scene when the menu is shown
    private void TogglePauseMenu()
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

    //This toggles the settings menu
    private void ToggleSettingsMenu()
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

    //This changes the length of the  PC Player health bar, to represent the total health
    private void ChangeHealthBar(float _currentHealth)
    {
        ChangeBar(pcHealthBar, _maxHealthBarLength, _currentHealth / 100f);
    }

    //This changes the length of the  PC Player charge bar, to represent the total charge
    private void ChangeChargeBar(float _currentCharge)
    {
       ChangeBar(pcChargeBar, _maxChargeBarLength, _currentCharge / 100f);
    }
    
    //Changes the size of the specified bar
    private void ChangeBar(Transform _bar, float _maxBarLength, float _newValue)
    {
        _bar.localScale = new Vector3(_maxBarLength * _newValue, _bar.localScale.y, _bar.localScale.z);
    }
    
    //Toggles the game over screen
    [Rpc(SendTo.Everyone, InvokePermission = RpcInvokePermission.Everyone)]
    public void ToggleGameOverMenuRpc()
    {
        ToggleGameOverMenu();
    }

    //Toggles the game over screen, stopping the game when the menu is active
    private void ToggleGameOverMenu()
    {
        Debug.Log("ToggleGameOverMenu _showGameOverMenu: " + _showGameOverMenu);
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

    //This can stop/resume the game when required
    private void CheckTimeScale()
    {
        if (_showPauseMenu || _showSettingsMenu || _showGameOverMenu)
        {
            FreezeGame();
        }
        else
        {
            ResumeGame();
        }
    }

    //This stops the game
    private void FreezeGame()
    {
        EventsManager.ToggleAll(false);
        AudioListener.volume = 0;
        Time.timeScale = 0;
    }

    //This resumes the game
    private void ResumeGame()
    {
        EventsManager.ToggleAll(true);
        AudioListener.volume = 1;
        Time.timeScale = 1;
    }
}
