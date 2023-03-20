using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : PlayerBase, IPlayerController
{
    [SerializeField] float moveSpeed = 10f;
    [SerializeField] float rotateSpeed = 10f;
    [SerializeField] Transform rotateTarget;

    Vector3 moveDir;

    public bool IsMoving { get; set; } = false;

    public void OnMove(Vector2 value)
    {
        if (IsComponentActive)
        {
            //Debug.Log($"OnMove {value}");
            moveDir = transform.up * value.y + transform.right * value.x;
        }
    }

    public void OnMoveToward(Vector3 target)
    {
        if (IsComponentActive)
        {
            //Debug.Log($"OnMoveToward {target}");
            moveDir = (target - transform.position).normalized;
        }
    }

    private void LateUpdate()
    {
        if (IsMoving)
        {
            InputMove();
            InputRotate();
        }
    }

    private void InputRotate()
    {
        //transform.up = moveDir;
        Quaternion targetRotation = Quaternion.LookRotation(rotateTarget.forward, moveDir);
        Quaternion rotation = Quaternion.RotateTowards(rotateTarget.rotation, targetRotation, rotateSpeed * Time.deltaTime);

        rotateTarget.rotation = rotation;
    }

    private void InputMove()
    {
        transform.position += moveDir * moveSpeed * Time.deltaTime;
    }
}
