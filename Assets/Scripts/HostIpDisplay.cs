using System.Collections.Generic;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using Unity.Netcode;
using UnityEngine;
using TMPro;

public class HostIpDisplay : MonoBehaviour
{
    [SerializeField] TMP_Text hostIpLabel;

    void Start()
    {
        if (hostIpLabel == null) return;

        if (NetworkManager.Singleton == null || !NetworkManager.Singleton.IsHost)
        {
            hostIpLabel.text = "";
            return;
        }

        hostIpLabel.text = "Host IP: " + GetLocalIPv4();
    }

    string GetLocalIPv4()
    {
        var candidates = new List<string>();

        try
        {
            foreach (var ni in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (ni.OperationalStatus != OperationalStatus.Up)
                    continue;

                if (ni.NetworkInterfaceType == NetworkInterfaceType.Loopback ||
                    ni.NetworkInterfaceType == NetworkInterfaceType.Tunnel)
                    continue;

                var props = ni.GetIPProperties();
                foreach (var unicast in props.UnicastAddresses)
                {
                    if (unicast.Address.AddressFamily == AddressFamily.InterNetwork &&
                        !IPAddress.IsLoopback(unicast.Address))
                    {
                        candidates.Add(unicast.Address.ToString());
                    }
                }
            }
        }
        catch
        {
        }

        if (candidates.Count == 0)
            return "127.0.0.1";

        string preferred = candidates.Find(a => a.StartsWith("192."));
        if (!string.IsNullOrEmpty(preferred)) return preferred;

        preferred = candidates.Find(a => a.StartsWith("10."));
        if (!string.IsNullOrEmpty(preferred)) return preferred;

        preferred = candidates.Find(a => a.StartsWith("172."));
        if (!string.IsNullOrEmpty(preferred)) return preferred;

        return candidates[0];
    }
}
