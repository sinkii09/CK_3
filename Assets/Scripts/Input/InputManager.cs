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

    public Vector2 AimPosition { get; private set; }

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

    public void OnFire(InputAction.CallbackContext context)
    {
        if(context.performed)
        {
            FireEvent?.Invoke(true);
        }
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        if(context.performed)
        {
            MoveEvent?.Invoke(context.ReadValue<Vector2>());
        }
        else if(context.canceled)
        {
            MoveEvent?.Invoke(Vector2.zero);
        }
    }

    public void OnAim(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            AimPosition = context.ReadValue<Vector2>();
        }
        else if (context.canceled)
        {
            AimPosition = Vector2.zero;
        }
    }
}
