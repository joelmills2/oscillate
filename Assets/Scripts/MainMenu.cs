/*
Menu system adapted from: https://www.youtube.com/watch?v=76WOa6IU_s8
*/

using System.Net;
using System.Net.Sockets;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using TMPro;

public class MainMenu : MonoBehaviour
{
    [Header("Scenes")]
    [SerializeField] private string gameScene = "PathfindingScene";

    [SerializeField] private TMP_InputField playerNameInput;
    [SerializeField] private TMP_InputField ipInputField;
    [SerializeField] private TMP_Text hostIpLabel;

    public GameObject optionsScreen;

    private NetworkManager Net => NetworkManager.Singleton;

    void Start()
    {
        if (hostIpLabel != null)
        {
            hostIpLabel.text = "Host IP: " + GetLocalIPv4();
        }
    }

    public void StartHost()
    {
        if (Net == null)
        {
            Debug.LogError("No NetworkManager in the Main Menu scene.");
            return;
        }

        CachePlayerName();
        ConfigureHostTransport();

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
        ConfigureClientTransport();

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

    private void ConfigureHostTransport()
    {
        var utp = Net.NetworkConfig.NetworkTransport as UnityTransport;
        if (utp == null) return;

        utp.ConnectionData.Address = "0.0.0.0";
        utp.ConnectionData.Port = 7777;
    }

    private void ConfigureClientTransport()
    {
        var utp = Net.NetworkConfig.NetworkTransport as UnityTransport;
        if (utp == null) return;

        string ip = "127.0.0.1";
        if (ipInputField != null && !string.IsNullOrWhiteSpace(ipInputField.text))
        {
            ip = ipInputField.text.Trim();
        }

        utp.ConnectionData.Address = ip;
        utp.ConnectionData.Port = 7777;
    }

    private string GetLocalIPv4()
    {
        try
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork && !IPAddress.IsLoopback(ip))
                    return ip.ToString();
            }
        }
        catch
        {
        }

        return "127.0.0.1";
    }
}
