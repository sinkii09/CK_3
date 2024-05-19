using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using static PlayerInputActions;

[CreateAssetMenu(fileName = "New Input Manager", menuName = "Input/InputManager")]
public class InputManager : ScriptableObject, IPlayerActions
{
    public event Action<Vector2> MoveEvent;
    public event Action<bool> FireEvent;
    public event Action<bool> JumpEvent;



    public Vector2 MousePosition { get; private set; }

    private PlayerInputActions _controller;

    private void OnEnable()
    {
        if(_controller == null)
        {
            _controller = new PlayerInputActions();
            _controller.Player.SetCallbacks(this);
        }

        _controller.Player.Enable();
    }

    private void OnDisable()
    {
        _controller.Player.Disable();
    }
    public Vector2 GetMoveInputDirection()
    {
        return _controller.Player.MoveDirection.ReadValue<Vector2>();
    }
    public Vector2 GetMouseScreenPosition()
    {
        return Mouse.current.position.ReadValue();
    }
    public bool IsRightMouseButtonDownThisFrame()
    {
        return _controller.Player.RightMouseClick.WasPressedThisFrame();
    }
    public bool IsLeftMouseButtonDownThisFrame()
    {
        return _controller.Player.LeftMouseClick.WasPressedThisFrame();
    }
    public bool IsLeftMouseButtonUpThisFrame()
    {
        return _controller.Player.LeftMouseClick.WasReleasedThisFrame();
    }
    public void OnMousePosition(InputAction.CallbackContext context)
    {
        MousePosition = context.ReadValue<Vector2>();
    }

    public void OnRightMouseClick(InputAction.CallbackContext context)
    {

    }

    public void OnLeftMouseClick(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            FireEvent?.Invoke(true);
        }
    }

    public void OnMoveDirection(InputAction.CallbackContext context)
    {
        MoveEvent?.Invoke(context.ReadValue<Vector2>());
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if(context.performed)
        {
            JumpEvent?.Invoke(true);
        }
    }
}
