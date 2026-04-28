using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System;
//This script controls all the options in the Pause Menu
public class MenuManager : MonoBehaviour
{
    [SerializeField] private GameObject pauseMenu;
    private bool _showPauseMenu;

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

        Time.timeScale = _showPauseMenu ? 0 : 1;
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

        Time.timeScale = _showGameOverMenu ? 0 : 1;
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
                    winGameMenu.transform.GetChild(i).GetChild(0).GetComponentInChildren<TMP_Text>().text = $"Final Score: { PlayerProfileManager.GetScore(0) }";
                }
                winGameMenu.transform.GetChild(i).gameObject.SetActive(_showWinGameMenu);
            }  
        }

        if (_showWinGameMenu) {
            FreezeGame();
        } else {
            ResumeGame();
        }

        // Time.timeScale = _showWinGameMenu ? 0 : 1;
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
        Time.timeScale = 0;
    }

    private void ResumeGame()
    {
        OnToggleAll?.Invoke(true);
        Time.timeScale = 1;
    }
}
