using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class DestroyPhysicObject : MonoBehaviour
{
    [SerializeField] float destroyTimer;
    NetworkObject networkObject;

    private void Awake()
    {
        networkObject = GetComponent<NetworkObject>();
    }

    private void OnEnable()
    {
        StartCoroutine(AutoDestroy(destroyTimer));
    }

    private IEnumerator AutoDestroy(float destroyTimer)
    {
        yield return new WaitForSeconds(destroyTimer);
        if (networkObject != null)
        {
            if (NetworkManager.Singleton.IsServer)
            {
                networkObject.Despawn();
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
