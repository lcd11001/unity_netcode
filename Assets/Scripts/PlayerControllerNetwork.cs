using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerControllerNetwork : NetworkBehaviour
{
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        IComponent component = GetComponent<IComponent>();
        if (component != null && !IsOwner)
        {
            component.IsComponentActive = false;
        }
    }
    
    
}
