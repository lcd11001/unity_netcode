using StarterAssets;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

public class PlayerRaycastController : NetworkBehaviour
{
    [SerializeField]
    private GameObject leftHand;

    [SerializeField]
    private GameObject rightHand;

    [SerializeField]
    private float minPunchDistance = 1.0f;

    [SerializeField]
    private LayerMask playerMask;

    public UnityEvent<int> OnDamged;

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
        Debug.Log($"Client {clientId} got punch {damage}");
        if (NetworkManager.Singleton.ConnectedClients.ContainsKey(clientId))
        {
            var client = NetworkManager.Singleton.ConnectedClients[clientId];
            var hud = client.PlayerObject.GetComponent<PlayerHud>();
            if (hud != null)
            {
                hud.OnDamageServerRpc(damage);
            }
        }
    }
}
