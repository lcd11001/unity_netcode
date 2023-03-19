using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour, IPlayerController
{
    [SerializeField] float moveSpeed = 10f;
    public void OnMove(Vector2 value)
    {
        //Debug.Log($"OnMove {value}");
        Vector3 moveDir = transform.up * value.y + transform.right * value.x;
        transform.position += moveDir * moveSpeed * Time.deltaTime;
    }

    public void OnMoveToward(Vector3 target)
    {
        //Debug.Log($"OnMoveToward {target}");
        this.transform.position = Vector3.MoveTowards(this.transform.position, target, Time.deltaTime * moveSpeed);
    }

    private void Update()
    {
        //Vector2 mousePosition
    }
}
