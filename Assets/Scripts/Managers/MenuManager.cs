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

    [SerializeField] private GameObject winMenu;
    private bool _showWinMenu;

    [SerializeField] private GameObject pcPlayerUI;

    [SerializeField] private GameObject pcHealthBar;
    private Transform pcHealthBarTransform;
    private Image pcHealthBarImage;

    private float _maxHealthBarLength;

    [SerializeField] private GameObject pcChargeBar;
    private Transform pcChargeBarTransform;
    private Image pcChargeBarImage;

    private float _maxChargeBarLength;
    private TMP_Text _scoreUI;

    [SerializeField] private Material increaseMaterial;
    [SerializeField] private Material decreaseMaterial;

    private float previousHealth;

    private AudioSource _audioSource;

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

        _showWinMenu = true;
        ToggleWinMenu();

        _scoreUI = pcPlayerUI.GetComponentInChildren<TMP_Text>();

        pcHealthBarTransform = pcHealthBar.transform;
        pcHealthBarImage = pcHealthBar.GetComponent<Image>();
        _maxHealthBarLength = pcHealthBarTransform.localScale.y;

        pcChargeBarTransform = pcChargeBar.transform;
        pcChargeBarImage = pcChargeBar.GetComponent<Image>();
        _maxChargeBarLength = pcChargeBarTransform.localScale.y;

        previousHealth = 1f;

        _audioSource = this.gameObject.GetComponent<AudioSource>();
    }

    private void OnEnable()
    {
        EventsManager.OnCancel += TogglePauseMenu;
        
        EventsManager.OnGameOver += ToggleGameOverMenu;
        EventsManager.OnEnteredTemple += ToggleWinMenu;
        EventsManager.OnChangeHealthBar += ChangeHealthBar;
        EventsManager.OnChangeChargeBar += ChangeChargeBar;
    }

    private void OnDisable()
    {
        EventsManager.OnCancel -= TogglePauseMenu;

        EventsManager.OnGameOver -= ToggleGameOverMenu;
        EventsManager.OnEnteredTemple -= ToggleWinMenu;
        EventsManager.OnChangeHealthBar -= ChangeHealthBar;
        EventsManager.OnChangeChargeBar -= ChangeChargeBar;
    }

    //This toggles the pause menu, and pausing the scene when the menu is shown
    public void TogglePauseMenu()
    {
        _showPauseMenu = !_showPauseMenu;

        for (int i = 0; i < pauseMenu.transform.childCount; i++)
        {
            if (pauseMenu.transform.GetChild(i).gameObject != null)  pauseMenu.transform.GetChild(i).gameObject.SetActive(_showPauseMenu);
        }

        CheckCursorTimeScale();
        CheckTimeScale();
    }

    //This toggles the settings menu (with UI Buttons too)
    public void ToggleSettingsMenu()
    {
        _showSettingsMenu = !_showSettingsMenu;

        for (int i = 0; i < settingsMenu.transform.childCount; i++)
        {
            if (settingsMenu.transform.GetChild(i).gameObject != null)  settingsMenu.transform.GetChild(i).gameObject.SetActive(_showSettingsMenu);
        }

        CheckCursorTimeScale();
        CheckTimeScale();
    }

    //This changes the length of the  PC Player health bar, to represent the total health
    private void ChangeHealthBar(float _currentHealth)
    {
        StartCoroutine(ChangeBar(pcHealthBarTransform, pcHealthBarImage, _maxHealthBarLength, _currentHealth / 100f, 1f));
    }

    //This changes the length of the  PC Player charge bar, to represent the total charge
    private void ChangeChargeBar(float _currentCharge)
    {
        ConstantChangeBar(pcChargeBarTransform, _maxChargeBarLength, _currentCharge / 100f);
    }
    
    //Changes the size of the specified bar
    private void ConstantChangeBar(Transform _bar, float _maxBarLength, float _newValue)
    {
        _bar.localScale = new Vector3(_maxBarLength * _newValue, _bar.localScale.y, _bar.localScale.z);
    }

    IEnumerator ChangeBar(Transform _barTransform, Image _barImage, float _maxBarLength, float _newValue, float _delay)
    {
        float elapsed = 0f;
        Color originalColour = _barImage.color;
        Material chargingMaterial;

        if(previousHealth > _newValue)
        {
            chargingMaterial = decreaseMaterial;
        }
        else
        {
            chargingMaterial = increaseMaterial;
        }


        // _barTransform.localScale = new Vector3(_maxBarLength * _newValue, _barTransform.localScale.y, _barTransform.localScale.z);
        while(elapsed < _delay)
        {
            elapsed += Time.deltaTime;
            _barTransform.localScale = new Vector3(_maxBarLength * (previousHealth + ((_newValue - previousHealth)  * (elapsed / _delay))), _barTransform.localScale.y, _barTransform.localScale.z);
            
            if(elapsed <= _delay/4)
            {
                _barImage.color =   Color.Lerp(
                                        originalColour,
                                        chargingMaterial.color,
                                        (elapsed / _delay) * 4
                                    );
            }
            
            if(elapsed >= _delay/4)
            {
                _barImage.color = Color.Lerp(
                                        chargingMaterial.color,
                                        originalColour,
                                        ((elapsed - (_delay * 0.75f)) / _delay) * 4
                                    );
            }

            yield return null;
        }

        _barTransform.localScale = new Vector3(_maxBarLength * _newValue, _barTransform.localScale.y, _barTransform.localScale.z);
        _barImage.color = originalColour;
        previousHealth = _newValue;
    }
    
    //Toggles the game over screen (using UI Buttons)
    [Rpc(SendTo.Everyone, InvokePermission = RpcInvokePermission.Everyone)]
    public void ToggleGameOverMenuRpc()
    {
        ToggleGameOverMenu();
    }

    //Toggles the game over screen, stopping the game when the menu is active
    private void ToggleGameOverMenu()
    {
        _showGameOverMenu = !_showGameOverMenu;

        for (int i = 0; i < gameOverMenu.transform.childCount; i++)
        {
            if (gameOverMenu.transform.GetChild(i).gameObject != null)  gameOverMenu.transform.GetChild(i).gameObject.SetActive(_showGameOverMenu);
        }

        CheckCursorTimeScale();
        CheckTimeScale();
    }

    //Toggles the win screen, stopping the game when the menu is active
    private void ToggleWinMenu()
    {
        _showWinMenu = !_showWinMenu;

        for (int i = 0; i < winMenu.transform.childCount; i++)
        {
            if (winMenu.transform.GetChild(i).gameObject != null)  winMenu.transform.GetChild(i).gameObject.SetActive(_showWinMenu);
        }

        CheckCursorTimeScale();
        CheckTimeScale();
    }

    //This can disable/enable the cursor when required
    private void CheckCursorTimeScale()
    {
        if (_showPauseMenu || _showSettingsMenu || _showGameOverMenu || _showWinMenu)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
    
    //This can stop/resume the game when required
    private void CheckTimeScale()
    {
        if (_showPauseMenu || _showSettingsMenu || _showGameOverMenu || _showWinMenu)
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
        EventsManager.TogglePauseManagerAudio(true);
        EventsManager.ToggleAll(false);
        AudioListener.volume = 0;
        Time.timeScale = 0;

        _audioSource.pitch = 1f;
        _audioSource.Stop();
        EventsManager.FixTeleportEffect(0, true);
    }

    //This resumes the game
    private void ResumeGame()
    {
        EventsManager.TogglePauseManagerAudio(false);
        EventsManager.ToggleAll(true);
        AudioListener.volume = 1;
        Time.timeScale = 1;
    }
}
