using UnityEngine;
using UnityEngine.SceneManagement;
//This script controls all the options in the Main Menu
public class MainMenu : MonoBehaviour
{
    public void PlayLevel(string _sceneName)
    {
        SceneManager.LoadScene(_sceneName);
    }

    public void Quit()
    {
        Application.Quit();
    }
}
