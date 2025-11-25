/*
Adapted from: https://www.youtube.com/watch?v=JivuXdrIHK0
*/

using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class PauseMenu : NetworkBehaviour
{
    public static bool GameIsPaused = false;

    [SerializeField] private GameObject pauseMenuUI;
    [SerializeField] private InputActionReference pauseAction;
    [SerializeField] private string menuSceneName = "MainMenu";

    void OnEnable()
    {
        if (pauseAction != null)
        {
            pauseAction.action.performed += OnPausePerformed;
            pauseAction.action.Enable();
        }
    }

    void OnDisable()
    {
        if (pauseAction != null)
        {
            pauseAction.action.performed -= OnPausePerformed;
            pauseAction.action.Disable();
        }
    }

    void OnPausePerformed(InputAction.CallbackContext ctx)
    {
        if (GameIsPaused)
            Resume();
        else
            Pause();
    }

    public void Resume()
    {
        pauseMenuUI.SetActive(false);
        GameIsPaused = false;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void Pause()
    {
        pauseMenuUI.SetActive(true);
        GameIsPaused = true;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void LoadMenu()
    {
        if (IsHost)
        {
            SendClientsToMenuClientRpc();
            NetworkManager.Singleton.Shutdown();
            SceneManager.LoadScene(menuSceneName);
        }
        else
        {
            NetworkManager.Singleton.Shutdown();
            SceneManager.LoadScene(menuSceneName);
        }
    }

    public void QuitGame()
    {
        if (IsHost)
        {
            SendClientsToMenuClientRpc();
            NetworkManager.Singleton.Shutdown();
            Application.Quit();
            Debug.Log("Host quitting game");
        }
        else
        {
            NetworkManager.Singleton.Shutdown();
            Application.Quit();
            Debug.Log("Client quitting game");
        }
    }

    [ClientRpc]
    void SendClientsToMenuClientRpc()
    {
        if (IsHost) return;

        NetworkManager.Singleton.Shutdown();
        SceneManager.LoadScene(menuSceneName);
    }
}
