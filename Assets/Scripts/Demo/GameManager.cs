using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace Demo
{
    public class GameManager : MonoBehaviourSingletonPersistent<GameManager>
    {
        void Start()
        {
            if (LobbyManager.Instance.IsGameHost)
            {
                NetworkManager.Singleton.StartHost();
            }
            else
            {
                NetworkManager.Singleton.StartClient();
            }
        }

        
    }
}
