using UnityEngine;
using Unity.Netcode;
using TMPro;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class ChatManager : NetworkBehaviour
{
    public static ChatManager Singleton;

    [SerializeField] ChatMessage chatMessagePrefab;
    [SerializeField] RectTransform chatContent;
    [SerializeField] ScrollRect chatScrollRect;
    [SerializeField] TMP_InputField chatInput;

    public string playerName;

    void Awake()
    {
        Singleton = this;
    }

    void Update()
    {
        if (!IsClient) return;

        var kb = Keyboard.current;
        if (kb == null) return;

        if (kb.tKey.wasPressedThisFrame)
            chatInput.ActivateInputField();

        bool pressedEnter =
            (kb.enterKey != null && kb.enterKey.wasPressedThisFrame) ||
            (kb.numpadEnterKey != null && kb.numpadEnterKey.wasPressedThisFrame);

        if (!pressedEnter) return;

        if (!string.IsNullOrWhiteSpace(chatInput.text))
        {
            SendChatMessage(chatInput.text);
            chatInput.text = "";
        }

        chatInput.DeactivateInputField();
    }

    public void SendChatMessage(string message, string fromWho = null)
    {
        if (string.IsNullOrWhiteSpace(message)) return;

        string name = !string.IsNullOrWhiteSpace(fromWho) ? fromWho : playerName;

        if (string.IsNullOrWhiteSpace(name))
            name = "Player " + NetworkManager.Singleton.LocalClientId;

        string s = name + " > " + message;
        SendChatMessageServerRpc(s);
    }

    void AddMessage(string msg)
    {
        ChatMessage cm = Instantiate(chatMessagePrefab, chatContent, false);
        cm.SetText(msg);

        Canvas.ForceUpdateCanvases();
        chatScrollRect.verticalNormalizedPosition = 0f;
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    void SendChatMessageServerRpc(string message)
    {
        ReceiveChatMessageClientRpc(message);
    }

    [ClientRpc]
    void ReceiveChatMessageClientRpc(string message)
    {
        Singleton.AddMessage(message);
    }

    public override void OnNetworkSpawn()
    {
        if (!IsClient) return;

        string name = GameSession.LocalPlayerName;
        if (string.IsNullOrWhiteSpace(name))
            name = "Player " + NetworkManager.Singleton.LocalClientId;

        playerName = name;
        Debug.Log($"ChatManager name set to {playerName} for client {NetworkManager.Singleton.LocalClientId}");
    }

    public bool IsTyping
    {
        get { return chatInput != null && chatInput.isFocused; }
    }
}
