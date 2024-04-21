using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using static PlayerInputActions;

[CreateAssetMenu(fileName = "New Input Manager", menuName = "Input/InputManager")]
public class InputManager : ScriptableObject,IPlayerActions
{
    public event Action<Vector2> MoveEvent;
    public event Action<bool> FireEvent;
    public event Action<Vector2> AimEvent;



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
        
    }
    public Vector2 GetMouseScreenPosition()
    {
        return Mouse.current.position.ReadValue();
    }
    public bool IsRightMouseButtonDownThisFrame()
    {
        return _controller.Player.RightMouseClick.WasPressedThisFrame();
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
}
