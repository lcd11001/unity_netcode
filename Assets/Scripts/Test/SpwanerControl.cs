using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Netcode.Extensions;

public class SpwanerControl : NetworkBehaviourSingleton<SpwanerControl>
{
    [SerializeField] private NetworkObjectPool networkObjectPool;
    
    [Space(10)]

    [SerializeField] private GameObject objectPrefab;
    [SerializeField] private int maxObjectCount = 3;

    private void Awake()
    {
        // init pool
        NetworkManager.Singleton.OnServerStarted += Singleton_OnServerStarted;
    }

    private void Singleton_OnServerStarted()
    {
        networkObjectPool.InitializePool();
    }

    public void SpawnObjects()
    {
        if (!IsServer)
        {
            return;
        }

        for(int i=0; i<maxObjectCount; i++)
        {
            Vector3 pos = new Vector3(
                Random.Range(-4.0f, 4.0f),
                Random.Range(10.0f, 15.0f),
                Random.Range(-4.0f, 4.0f)
            );

            /*
            GameObject go = Instantiate(objectPrefab, pos, Quaternion.identity);
            go.GetComponent<NetworkObject>().Spawn();
            */

            // pool init
            GameObject go = networkObjectPool.GetNetworkObject(objectPrefab, pos, Quaternion.identity).gameObject;
            go.GetComponent<NetworkObject>().Spawn();

        }
    }
}
