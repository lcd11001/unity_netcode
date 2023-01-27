using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Netcode;
using StarterAssets;

[RequireComponent(typeof(NetworkObject))]
public class PlayerController : NetworkBehaviour
{
    [SerializeField] private Vector2 defaultPositionRange = new Vector2(-4, 4);

    PlayerInput playerInput;

    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        playerInput.enabled = IsOwner;
    }

    private void Start()
    {
        if (IsOwner)
        {
            transform.position = new Vector3(
                Random.Range(defaultPositionRange.x, defaultPositionRange.y),
                0,
                Random.Range(defaultPositionRange.x, defaultPositionRange.y)
            );
        }
    }
}
