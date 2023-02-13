using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Events;
using System;

public class PlayerHud : NetworkBehaviour
{
    [SerializeField] TextMeshPro playerInfo;
    [SerializeField] Slider playerHealth;
    
    [SerializeField]
    private NetworkVariable<NetworkString> playerName = new NetworkVariable<NetworkString>();

    private const int PlayerMaxHealth = 10;
    [SerializeField]
    private NetworkVariable<int> networkPlayerHealth = new NetworkVariable<int>(PlayerMaxHealth);

    public void OnDamaged(int damged)
    {
        Debug.Log($"[{OwnerClientId}] OnDamaged {damged}");

        int newValue = networkPlayerHealth.Value - damged;

        playerHealth.value = (float)(newValue) / PlayerMaxHealth;

        RequestChangePlayerHealthValueServerRPC(newValue, new ServerRpcParams()
        {
            Receive = new ServerRpcReceiveParams()
            {
                SenderClientId = OwnerClientId
            }
        });
    }

    [ServerRpc]
    public void RequestChangePlayerHealthValueServerRPC(int newValue, ServerRpcParams @params = default)
    {
        if (!IsOwner) return;

        Debug.Log($"RequestChangePlayerHealthValueServerRPC {@params.Receive.SenderClientId}");
        //if (@params.Receive.SenderClientId == OwnerClientId)
        {
            Debug.Log($"[{OwnerClientId}] On Player Health Change {newValue}");
            networkPlayerHealth.Value = newValue;
        }
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (IsServer)
        {
            playerName.Value = $"Player {OwnerClientId}";
        }

        //Debug.Log($"PlayerHud::OnNetworkSpawn {OwnerClientId}");
        SetOverlay();
    }

    private void SetOverlay()
    {
        playerInfo.text = playerName.Value;
        playerHealth.value = 1.0f;
    }

    private void Update()
    {
        playerInfo.transform.rotation = Camera.main.transform.rotation;
        playerHealth.transform.rotation = Camera.main.transform.rotation;
    }
}
