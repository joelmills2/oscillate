using UnityEngine;
using Unity.Netcode;
using Unity.Collections;
using TMPro;

public class PlayerInfo : NetworkBehaviour
{
    public NetworkVariable<FixedString64Bytes> PlayerName =
        new NetworkVariable<FixedString64Bytes>(
            "Player",
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Owner);

    [SerializeField] private TMP_Text nameLabel;

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            string localName = GameSession.LocalPlayerName;
            if (string.IsNullOrWhiteSpace(localName))
                localName = "Player";

            PlayerName.Value = localName;
            Debug.Log($"[PlayerInfo] Set name for client {OwnerClientId} to '{localName}'");
        }

        UpdateNameLabel(PlayerName.Value.ToString());
        PlayerName.OnValueChanged += OnNameChanged;
    }

    public override void OnNetworkDespawn()
    {
        PlayerName.OnValueChanged -= OnNameChanged;
    }

    private void OnNameChanged(FixedString64Bytes oldValue, FixedString64Bytes newValue)
    {
        UpdateNameLabel(newValue.ToString());
    }

    private void UpdateNameLabel(string text)
    {
        if (nameLabel != null)
        {
            nameLabel.text = text;
        }
    }
}
