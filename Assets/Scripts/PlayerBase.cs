using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PlayerBase : MonoBehaviour, IComponent
{
    public bool IsComponentActive { get; set; } = true;
}
