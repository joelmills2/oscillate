using UnityEngine;
using Unity.Netcode;

public class NetworkManagerGuard : MonoBehaviour
{
    void Awake()
    {
        var thisManager = GetComponent<NetworkManager>();

        if (NetworkManager.Singleton != null && NetworkManager.Singleton != thisManager)
        {
            Destroy(gameObject);
        }
    }
}
