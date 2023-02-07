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
    [SerializeField] private Button physicButton;
    [SerializeField] private TextMeshProUGUI playersInGameText;
    [SerializeField] private TMP_InputField inputJoinCode;

    [Space(10)]

    [SerializeField] RelayManager relayManager;

    private void Awake()
    {
        physicButton?.gameObject.SetActive(false);
    }

    private void Start()
    {
        // Set this to true to reveal the cursor. Set it to false to hide the cursor.
        // Note that in CursorLockMode.Locked mode, the cursor is invisible regardless of the value of this property.
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        startHostButton?.onClick.AddListener(async () =>
        {
            if (relayManager.IsRelayEnabled)
            {
                RelayHostData relayHostData = await relayManager.SetupRelay();
                inputJoinCode.text = relayHostData.JoinCode;
            }
            else
            {
                Debug.Log("Relay server not enable");
            }

            if (NetworkManager.Singleton.StartHost())
            {
                Debug.Log("Host started");
            }
            else
            {
                Debug.Log("Host could not started");
            }
        });

        startServerButton?.onClick.AddListener(async () =>
        {
            if (relayManager.IsRelayEnabled)
            {
                RelayHostData relayHostData = await relayManager.SetupRelay();
                inputJoinCode.text = relayHostData.JoinCode;
            }
            else
            {
                Debug.Log("Relay server not enable");
            }

            if (NetworkManager.Singleton.StartServer())
            {
                Debug.Log("Server started");
            }
            else
            {
                Debug.Log("Server could not started");
            }
        });

        startClientButton?.onClick.AddListener(async () =>
        {
            if (relayManager.IsRelayEnabled && !string.IsNullOrEmpty(inputJoinCode.text))
            {
                await relayManager.JoinRelay(inputJoinCode.text);
            }
            else
            {
                Debug.Log("Relay server not enable");
            }

            if (NetworkManager.Singleton.StartClient())
            {
                Debug.Log("Client started");
            }
            else
            {
                Debug.Log("Client could not started");
            }
        });

        physicButton?.onClick.AddListener(() =>
        {
            Debug.Log("Spawn Physic Objects");
            SpwanerControl.Instance.SpawnObjects();
        });

        NetworkManager.Singleton.OnServerStarted += OnServerStarted;
    }

    private void OnServerStarted()
    {
        Debug.Log("Server started");
        physicButton?.gameObject.SetActive(true);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            Debug.Log("Press key P to Spawn Physic Objects");
            SpwanerControl.Instance.SpawnObjects();
        }

        if (playersInGameText != null)
        {
            playersInGameText.text = $"Players in game: {PlayersManager.Instance.PlayersInGame}";
        }
    }

}
