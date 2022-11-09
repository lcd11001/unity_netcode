using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class NetworkManagerUI : MonoBehaviour
{
    [SerializeField] private Button btnServer, btnHost, btnClient;

    private void Awake()
    {
        btnServer.onClick.AddListener(() =>
        {
            NetworkManager.Singleton.StartServer();
        });

        btnHost.onClick.AddListener(() =>
        {
            NetworkManager.Singleton.StartHost();
        });

        btnClient.onClick.AddListener(() =>
        {
            NetworkManager.Singleton.StartClient();
        });
    }
}
