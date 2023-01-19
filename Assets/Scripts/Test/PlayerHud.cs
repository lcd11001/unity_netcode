using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using TMPro;

public class PlayerHud : NetworkBehaviour
{
    [SerializeField] TextMeshPro playerInfo;
    private NetworkVariable<NetworkString> playerName = new NetworkVariable<NetworkString>();

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
    }

    private void Update()
    {
        playerInfo.transform.rotation = Camera.main.transform.rotation;
    }
}