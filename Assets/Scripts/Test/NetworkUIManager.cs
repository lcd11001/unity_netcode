using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Netcode;

public class NetworkUIManager : MonoBehaviour
{
    [SerializeField] private Button startServerButton;
    [SerializeField] private Button startHostButton;
    [SerializeField] private Button startClientButton;
    [SerializeField] private TextMeshProUGUI playersInGameText;

    private void Awake()
    {
        Cursor.visible = true;
    }

    private void Start()
    {
        startHostButton?.onClick.AddListener(() =>
        {
            if (NetworkManager.Singleton.StartHost())
            {
                Debug.Log("Host started");
            }
            else
            {
                Debug.Log("Host could not started");
            }
        });

        startServerButton?.onClick.AddListener(() =>
        {
            if (NetworkManager.Singleton.StartServer())
            {
                Debug.Log("Server started");
            }
            else
            {
                Debug.Log("Server could not started");
            }
        });

        startClientButton?.onClick.AddListener(() =>
        {
            if (NetworkManager.Singleton.StartClient())
            {
                Debug.Log("Client started");
            }
            else
            {
                Debug.Log("Client could not started");
            }
        });
    }

    private void Update()
    {
        if (playersInGameText != null)
        {
            playersInGameText.text = $"Players in game: {PlayersManager.Instance.PlayersInGame}";
        }
    }

}
