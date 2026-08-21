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
    private Color pcHealthBarOriginalColour;

    private float _maxHealthBarLength;

    [SerializeField] private GameObject pcChargeBar;
    private Transform pcChargeBarTransform;
    private Image pcChargeBarImage;
    private Color pcChargeBarOriginalColour;

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

        DisableAllMenusRpc();

        _scoreUI = pcPlayerUI.GetComponentInChildren<TMP_Text>();

        pcHealthBarTransform = pcHealthBar.transform;
        _maxHealthBarLength = pcHealthBarTransform.localScale.y;
        pcHealthBarImage = pcHealthBar.GetComponent<Image>();
        pcHealthBarOriginalColour = pcChargeBarImage.color;

        pcChargeBarTransform = pcChargeBar.transform;
        _maxChargeBarLength = pcChargeBarTransform.localScale.y;
        pcChargeBarImage = pcChargeBar.GetComponent<Image>();
        pcChargeBarOriginalColour = pcChargeBarImage.color;

        previousHealth = 1f;

        _audioSource = this.gameObject.GetComponent<AudioSource>();
    }

    private void OnEnable()
    {
        EventsManager.OnCancel += TogglePauseMenuRpc;
        
        EventsManager.OnGameOver += ToggleGameOverMenuRpc;
        EventsManager.OnEnteredTemple += ToggleWinMenuRpc;
        EventsManager.OnChangeHealthBar += ChangeHealthBarRpc;
        EventsManager.OnChangeChargeBar += ChangeChargeBarRpc;
    }

    private void OnDisable()
    {
        EventsManager.OnCancel -= TogglePauseMenuRpc;

        EventsManager.OnGameOver -= ToggleGameOverMenuRpc;
        EventsManager.OnEnteredTemple -= ToggleWinMenuRpc;
        EventsManager.OnChangeHealthBar -= ChangeHealthBarRpc;
        EventsManager.OnChangeChargeBar -= ChangeChargeBarRpc;
    }

    //This changes the length of the  PC Player health bar, to represent the total health
    [Rpc(SendTo.Everyone, InvokePermission = RpcInvokePermission.Everyone)]
    private void ChangeHealthBarRpc(float _currentHealth)
    {
        StartCoroutine(ChangeBar(pcHealthBarTransform, pcHealthBarImage, pcHealthBarOriginalColour, _maxHealthBarLength, _currentHealth / 100f, 1f));
    }

    //This changes the length of the  PC Player charge bar, to represent the total charge
    [Rpc(SendTo.Everyone, InvokePermission = RpcInvokePermission.Everyone)]
    private void ChangeChargeBarRpc(float _currentCharge)
    {
        ConstantChangeBar(pcChargeBarTransform, pcChargeBarImage, pcChargeBarOriginalColour, _maxChargeBarLength, _currentCharge / 100f);
    }
    
    //Changes the size of the specified bar
    private void ConstantChangeBar(Transform _barTransform, Image _barImage, Color originalColour, float _maxBarLength, float _newValue)
    {
        _barTransform.localScale = new Vector3(_maxBarLength * _newValue, _barTransform.localScale.y, _barTransform.localScale.z);
        _barImage.color = originalColour;
    }

    IEnumerator ChangeBar(Transform _barTransform, Image _barImage, Color originalColour, float _maxBarLength, float _newValue, float _delay)
    {
        float elapsed = 0f;
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

        previousHealth = _newValue;
        ConstantChangeBar(_barTransform, _barImage, originalColour, _maxBarLength, _newValue);
    }

    //This toggles the pause menu, and pausing the scene when the menu is shown
    [Rpc(SendTo.Everyone, InvokePermission = RpcInvokePermission.Everyone)]
    public void DisableAllMenusRpc()
    {
        _showPauseMenu = false;
        _showSettingsMenu = false;
        _showGameOverMenu = false;
        _showWinMenu = !_showWinMenu;

        for (int i = 0; i < pauseMenu.transform.childCount; i++)
        {
            if (pauseMenu.transform.GetChild(i).gameObject != null)  pauseMenu.transform.GetChild(i).gameObject.SetActive(_showPauseMenu);
        }

        for (int i = 0; i < settingsMenu.transform.childCount; i++)
        {
            if (settingsMenu.transform.GetChild(i).gameObject != null)  settingsMenu.transform.GetChild(i).gameObject.SetActive(_showSettingsMenu);
        }

        for (int i = 0; i < gameOverMenu.transform.childCount; i++)
        {
            if (gameOverMenu.transform.GetChild(i).gameObject != null)  gameOverMenu.transform.GetChild(i).gameObject.SetActive(_showGameOverMenu);
        }

        for (int i = 0; i < winMenu.transform.childCount; i++)
        {
            if (winMenu.transform.GetChild(i).gameObject != null)  winMenu.transform.GetChild(i).gameObject.SetActive(_showWinMenu);
        }

        CheckCursorTimeScale();
        CheckTimeScale();
    }

    //This toggles the pause menu, and pausing the scene when the menu is shown
    [Rpc(SendTo.Everyone, InvokePermission = RpcInvokePermission.Everyone)]
    public void TogglePauseMenuRpc(bool _toggle)
    {
        _showPauseMenu = _toggle;

        for (int i = 0; i < pauseMenu.transform.childCount; i++)
        {
            if (pauseMenu.transform.GetChild(i).gameObject != null)  pauseMenu.transform.GetChild(i).gameObject.SetActive(_showPauseMenu);
        }

        CheckCursorTimeScale();
        CheckTimeScale();
    }

    //This toggles the settings menu (with UI Buttons too)
    [Rpc(SendTo.Everyone, InvokePermission = RpcInvokePermission.Everyone)]
    public void ToggleSettingsMenuRpc(bool _toggle)
    {
        _showSettingsMenu = _toggle;

        for (int i = 0; i < settingsMenu.transform.childCount; i++)
        {
            if (settingsMenu.transform.GetChild(i).gameObject != null)  settingsMenu.transform.GetChild(i).gameObject.SetActive(_showSettingsMenu);
        }

        CheckCursorTimeScale();
        CheckTimeScale();
    }
    
    //Toggles the game over screen (using UI Buttons), stopping the game when the menu is active
    [Rpc(SendTo.Everyone, InvokePermission = RpcInvokePermission.Everyone)]
    public void ToggleGameOverMenuRpc(bool _toggle)
    {
        _showGameOverMenu = _toggle;

        for (int i = 0; i < gameOverMenu.transform.childCount; i++)
        {
            if (gameOverMenu.transform.GetChild(i).gameObject != null)  gameOverMenu.transform.GetChild(i).gameObject.SetActive(_showGameOverMenu);
        }

        CheckCursorTimeScale();
        CheckTimeScale();
    }

    //Toggles the win screen, stopping the game when the menu is active
    [Rpc(SendTo.Everyone, InvokePermission = RpcInvokePermission.Everyone)]
    private void ToggleWinMenuRpc(bool _toggle)
    {
        _showWinMenu = _toggle;

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
