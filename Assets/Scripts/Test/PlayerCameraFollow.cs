using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCameraFollow : MonoBehaviourSingleton<PlayerCameraFollow>
{
    private CinemachineVirtualCamera cinemachineVirtualCamera;
    private void Awake()
    {
        cinemachineVirtualCamera = GetComponent<CinemachineVirtualCamera>();
    }

    public void FollowPlayer(Transform transform)
    {
        cinemachineVirtualCamera.Follow = transform;
    }
}
