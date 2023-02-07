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
        if (NetworkManager.Singleton.IsServer)
        {
            if (networkObject != null)
            {
                networkObject.Despawn();
            }
            else
            {
                Destroy(gameObject);
            }
        }
    }
}
