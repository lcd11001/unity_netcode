using UnityEngine;
using Unity.Netcode;

public class PlayersManager: NetworkBehaviourSingleton<PlayersManager>
{
    // synchronize network variable
    private NetworkVariable<int> playersInGame = new NetworkVariable<int>();

    public int PlayersInGame
    {
        get
        {
            return playersInGame.Value;
        }
    }

    private void Start()
    {
        if (NetworkManager != null)
        {
            NetworkManager.OnClientConnectedCallback += Singleton_OnClientConnectedCallback;
            NetworkManager.OnClientDisconnectCallback += Singleton_OnClientDisconnectCallback;
        }
    }

    public override void OnDestroy()
    {
        if (NetworkManager != null)
        {
            NetworkManager.OnClientConnectedCallback -= Singleton_OnClientConnectedCallback;
            NetworkManager.OnClientDisconnectCallback -= Singleton_OnClientDisconnectCallback;
        }
     
        base.OnDestroy();
    }

    private void Singleton_OnClientDisconnectCallback(ulong id)
    {
        if (IsServer)
        {
            Debug.Log($"player id [{id}] just disconnected");
            playersInGame.Value--;
        }
    }

    private void Singleton_OnClientConnectedCallback(ulong id)
    {
        if (IsServer)
        {
            Debug.Log($"player id [{id}] just connected");
            playersInGame.Value++;
        }
    }
}
