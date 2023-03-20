using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IComponent
{
    bool IsComponentActive { get; set; }
}

public interface IPlayerController
{
    void OnMove(Vector2 value);
    void OnMoveToward(Vector3 target);
}
