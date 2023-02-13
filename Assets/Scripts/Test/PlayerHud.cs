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
    private NetworkVariable<NetworkString> networkPlayerName = new NetworkVariable<NetworkString>();

    private const int PlayerMaxHealth = 1000;
    [SerializeField]
    private NetworkVariable<int> networkPlayerHealth = new NetworkVariable<int>(PlayerMaxHealth);

    private void Start()
    {
        networkPlayerHealth.OnValueChanged += OnHealthChange;
    }

    [ServerRpc(RequireOwnership = false)]
    public void OnDamageServerRpc(int damage, ServerRpcParams rpcParams = default)
    {
        Debug.Log($"OnDamageServerRpc {rpcParams.Receive.SenderClientId}");
        networkPlayerHealth.Value = Mathf.Max(0, networkPlayerHealth.Value - damage);
    }

    private void OnHealthChange(int previousValue, int newValue)
    {
        Debug.Log($"OnHealthChange {previousValue} => {newValue}");
        playerHealth.value = newValue;
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (IsServer)
        {
            networkPlayerName.Value = $"Player {OwnerClientId}";
            networkPlayerHealth.Value = PlayerMaxHealth;
        }

        //Debug.Log($"PlayerHud::OnNetworkSpawn {OwnerClientId}");
        SetOverlay();
    }

    private void SetOverlay()
    {
        playerInfo.text = networkPlayerName.Value;
        playerHealth.value = playerHealth.maxValue = networkPlayerHealth.Value;
    }

    private void Update()
    {
        //playerInfo.transform.rotation = Camera.main.transform.rotation;
        //playerHealth.transform.rotation = Camera.main.transform.rotation;

        playerHealth.transform.parent.rotation = Camera.main.transform.rotation;
    }
}
