using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Demo
{
    public class GameManager : MonoBehaviourSingletonPersistent<GameManager>
    {
        [SerializeField] GameObject playerPrefab;
        //void Start()
        //{
        //    if (LobbyManager.Instance.IsGameHost)
        //    {
        //        NetworkManager.Singleton.StartHost();
        //    }
        //    else
        //    {
        //        NetworkManager.Singleton.StartClient();
        //    }
        //}

        public void ServerSceneInit(ulong clientId)
        {
            Vector3 spawnPos = new Vector3(
                Random.Range(-3.0f, 3.0f),
                0,
                Random.Range(-3.0f, 3.0f)
            );
            GameObject go = Instantiate(playerPrefab, spawnPos, Quaternion.identity);
            NetworkObject no = go.GetComponent<NetworkObject>();
            no.SpawnWithOwnership(clientId, true);
        }
    }
}
