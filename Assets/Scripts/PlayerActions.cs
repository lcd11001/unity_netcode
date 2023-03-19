using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.InputSystem;

public class PlayerActions : MonoBehaviour
{
    private PlayerControls actions;
    private IPlayerController controller;

    private bool isTouching;
    private bool isKeyPress;

    private Camera mainCamera;
    private Vector3 touchInput;

    readonly object lockKey = new object();
    readonly object lockTouch = new object();

    private void Awake()
    {
        actions = new PlayerControls();

        controller = GetComponent<IPlayerController>();
        Assert.IsNotNull(controller, "PlayerController component not found");

        mainCamera = Camera.main;
    }

    private void OnEnable()
    {
        actions.Player.Movement.performed += Movement_performed;
        actions.Player.Movement.canceled += Movement_canceled;

        actions.Player.Touch.started += Touch_started;
        actions.Player.Touch.canceled += Touch_canceled;

        actions.Enable();
    }


    private void OnDisable()
    {
        actions.Disable();
        actions.Player.Movement.performed -= Movement_performed;
        actions.Player.Movement.canceled -= Movement_canceled;

        actions.Player.Touch.started -= Touch_started;
        actions.Player.Touch.canceled -= Touch_canceled;
    }

    #region Touchscreen

    private void Touch_started(InputAction.CallbackContext obj)
    {
        lock (lockTouch)
        {
            if (isTouching == false)
            {
                isTouching = true;
                StartCoroutine(DetectTouch());
            }
        }
    }

    private void Touch_canceled(InputAction.CallbackContext obj)
    {
        lock (lockTouch)
        {
            isTouching = false;
        }
    }

    private IEnumerator DetectTouch()
    {
        while (isTouching)
        {
            Vector2 screenPos = actions.Player.TouchPosition.ReadValue<Vector2>();
            touchInput.x = screenPos.x;
            touchInput.y = screenPos.y;
            touchInput.z = mainCamera.nearClipPlane - mainCamera.transform.position.z;
            Vector3 worldPos = mainCamera.ScreenToWorldPoint(touchInput);
            controller.OnMoveToward(worldPos);

            yield return null;
        }
    }

    #endregion

    #region Keyboard

    private void Movement_performed(InputAction.CallbackContext context)
    {
        lock (lockKey)
        {
            if (isKeyPress == false)
            {
                isKeyPress = true;
                StartCoroutine(DetectKeyboard());
            }
        }
    }

    private void Movement_canceled(InputAction.CallbackContext context)
    {
        lock (lockKey)
        {
            isKeyPress = false;
        }
    }

    private IEnumerator DetectKeyboard()
    {
        while (isKeyPress)
        {
            controller.OnMove(actions.Player.Movement.ReadValue<Vector2>());
            yield return null;
        }
    }

    #endregion

}
