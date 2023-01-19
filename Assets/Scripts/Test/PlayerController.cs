using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

[RequireComponent(typeof(NetworkObject))]
public class PlayerController : NetworkBehaviour
{
    [SerializeField] private Vector2 defaultPositionRange = new Vector2(-4, 4);
    [SerializeField] private NetworkVariable<Vector3> position = new NetworkVariable<Vector3>();
    Vector3 prevPosition = new Vector3();
    private void Start()
    {
        transform.position = new Vector3(
            Random.Range(defaultPositionRange.x, defaultPositionRange.y),
            0,
            Random.Range(defaultPositionRange.x, defaultPositionRange.y)
        );
    }

    private void Update()
    {
        if (IsLocalPlayer)
        {
            UpdateClient();
        }
        
        if (IsServer)
        {
            UpdateServer();
        }
    }

    private void UpdateServer()
    {
        transform.position = position.Value;
    }

    private void UpdateClient()
    {
        if (transform.position != prevPosition)
        {
            prevPosition = transform.position;
            SyncPositionServerRpc(prevPosition);
        }
    }

    [ServerRpc]
    private void SyncPositionServerRpc(Vector3 pos)
    {
        position.Value = pos;
    }
}
