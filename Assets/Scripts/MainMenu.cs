/*
Menu system adapted from: https://www.youtube.com/watch?v=76WOa6IU_s8
*/

using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public string gameScene;

    public void StartHost()
    {
        SceneManager.LoadScene(gameScene);
    }

    public void StartClient()
    {
        SceneManager.LoadScene(gameScene);
    }

    public void OpenOptions()
    {
        
    }

    public void CloseOptions()
    {
        
    }

    public void QuitGame()
    {
        Application.Quit();
        Debug.Log("Quitting Game");
    }
}
