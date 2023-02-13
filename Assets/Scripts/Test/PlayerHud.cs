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
    private NetworkVariable<NetworkString> playerName = new NetworkVariable<NetworkString>();

    private void Start()
    {
        PlayerRaycastController.OnDamged.AddListener(OnDamaged);
    }

    private void OnDamaged(int damged, int currentHealth, int maxHealth)
    {
        playerHealth.value = (float)currentHealth / maxHealth;
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
    }

    private void Update()
    {
        playerInfo.transform.rotation = Camera.main.transform.rotation;
        playerHealth.transform.rotation = Camera.main.transform.rotation;
    }
}
