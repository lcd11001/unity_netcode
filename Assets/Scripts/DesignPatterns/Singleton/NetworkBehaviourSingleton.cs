using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(NetworkObject))]
public class NetworkBehaviourSingleton<T> : NetworkBehaviour
    where T : Component
{
    private static T _instance;
    public static T Instance
    {
        get
        {
            if (_instance == null)
            {
                var objs = FindObjectsOfType<T>() as T[];
                if (objs.Length > 0)
                {
                    _instance = objs[0];
                }
                if (objs.Length > 1)
                {
                    Debug.LogError($"There is more than one {typeof(T).Name} in the scene");
                }
                if (_instance == null)
                {
                    GameObject obj = new GameObject();
                    // obj.hideFlags = HideFlags.HideAndDontSave;
                    
                    obj.name = $"[Singleton] {typeof(T).Name}";
                    obj.hideFlags = HideFlags.DontSave;

                    var networkObject = obj.GetComponent<NetworkObject>();
                    if (networkObject == null || !networkObject.IsSpawned)
                    {
                        Debug.LogError($"This NetworkObject component should be spawned from Unity inspector");
                    }
                    
                    _instance = obj.AddComponent<T>();
                }
            }
            return _instance;
        }
    }
}

[RequireComponent(typeof(NetworkObject))]
public class NetworkBehaviourSingletonPersistent<T> : NetworkBehaviour
    where T : Component
{
    public static T Instance { get; private set; }
    public virtual void Awake()
    {
        if (Instance == null)
        {
            Instance = this as T;
            DontDestroyOnLoad(this);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}

