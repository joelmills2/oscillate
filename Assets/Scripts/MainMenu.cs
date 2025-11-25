/*
Menu system adapted from: https://www.youtube.com/watch?v=76WOa6IU_s8
*/

using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Netcode;
using TMPro;

public class MainMenu : MonoBehaviour
{
    [Header("Scenes")]
    [SerializeField] private string gameScene = "PathfindingScene";

    [SerializeField] private TMP_InputField playerNameInput;

    private NetworkManager Net => NetworkManager.Singleton;

    public GameObject optionsScreen;

    public void StartHost()
    {
        if (Net == null)
        {
            Debug.LogError("No NetworkManager in the Main Menu scene.");
            return;
        }

        CachePlayerName();

        bool started = Net.StartHost();
        if (!started)
        {
            Debug.LogError("Failed to start host.");
            return;
        }

        Net.SceneManager.LoadScene(gameScene, LoadSceneMode.Single);
        Debug.Log("Host started, loading scene: " + gameScene);
    }

    public void StartClient()
    {
        if (Net == null)
        {
            Debug.LogError("No NetworkManager in the Main Menu scene.");
            return;
        }

        CachePlayerName();

        bool started = Net.StartClient();
        if (!started)
        {
            Debug.LogError("Failed to start client.");
            return;
        }

        Debug.Log("Client starting and connecting to host.");
    }

    public void OpenOptions()
    {
        optionsScreen.SetActive(true);
    }

    public void CloseOptions()
    {
        optionsScreen.SetActive(false);
    }

    public void QuitGame()
    {
        Application.Quit();
        Debug.Log("Quitting Game");
    }

    private void CachePlayerName()
    {
        string name = "Player";

        if (playerNameInput != null && !string.IsNullOrWhiteSpace(playerNameInput.text))
        {
            name = playerNameInput.text.Trim();
        }

        GameSession.LocalPlayerName = name;
        Debug.Log("Local player name set to: " + name);
    }
}
