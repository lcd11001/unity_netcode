using StarterAssets;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

public class PlayerRaycastController : NetworkBehaviour
{
    private const int PlayerMaxHealth = 10;

    [SerializeField]
    private NetworkVariable<int> networkPlayerHealth = new NetworkVariable<int>(PlayerMaxHealth);

    [SerializeField]
    private GameObject leftHand;

    [SerializeField]
    private GameObject rightHand;

    [SerializeField]
    private float minPunchDistance = 1.0f;

    [SerializeField]
    private LayerMask playerMask;

    public static UnityEvent<int, int, int> OnDamged = new UnityEvent<int, int, int>();

    NetworkThirdPersonController networkThirdPersonController;
    private void Awake()
    {
        networkThirdPersonController = GetComponent<NetworkThirdPersonController>();
    }

    private void FixedUpdate()
    {
        if (IsOwner)
        {
            if (networkThirdPersonController.Punched)
            {
                CheckPunch(leftHand.transform, Vector3.up);
                CheckPunch(rightHand.transform, Vector3.down);
            }
        }
    }

    private void CheckPunch(Transform hand, Vector3 aimDirection)
    {
        RaycastHit hit;
        int layerMask = playerMask.value;

        if (Physics.Raycast(hand.position, hand.TransformDirection(aimDirection), out hit, minPunchDistance, layerMask))
        {
            Debug.DrawRay(hand.position, hand.TransformDirection(aimDirection) * minPunchDistance, Color.yellow);

            var playerHit = hit.transform.GetComponent<NetworkObject>();
            if (playerHit != null)
            {
                UpdateHealthServerRPC(1, playerHit.OwnerClientId);
            }
        }
        else
        {
            Debug.DrawRay(hand.position, hand.TransformDirection(aimDirection) * minPunchDistance, Color.red);
        }
    }

    [ServerRpc]
    private void UpdateHealthServerRPC(int damage, ulong clientId)
    {
        var client = NetworkManager.Singleton.ConnectedClients[clientId]
            .PlayerObject.GetComponent<PlayerRaycastController>();

        if (client != null && client.networkPlayerHealth.Value > 0)
        {
            client.networkPlayerHealth.Value = Mathf.Max(client.networkPlayerHealth.Value - damage, 0);
            // execute method on client getting punch
        }

        NotifyHealthChangedClientRPC(damage, new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = new ulong[] { clientId }
            }
        });
    }

    [ClientRpc]
    public void NotifyHealthChangedClientRPC(int damage, ClientRpcParams @params = default)
    {
        //if (IsOwner) return;

        Debug.Log($"Client got punch {damage}");
        OnDamged?.Invoke(damage, networkPlayerHealth.Value, PlayerMaxHealth);
    }
}
