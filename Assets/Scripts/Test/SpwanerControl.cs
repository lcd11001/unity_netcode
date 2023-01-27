using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class SpwanerControl : NetworkBehaviourSingleton<SpwanerControl>
{
    [SerializeField] private GameObject objectPrefab;
    [SerializeField] private int maxObjectCount = 3;

    private void Awake()
    {
        // init pool
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
            GameObject go = Instantiate(objectPrefab, pos, Quaternion.identity);
            go.GetComponent<NetworkObject>().Spawn();

            // pool init
        }
    }
}
