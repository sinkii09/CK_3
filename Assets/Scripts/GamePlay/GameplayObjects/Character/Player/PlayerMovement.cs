using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerMovement : NetworkBehaviour
{
    [Header("Settings")]
    [SerializeField] private float movementSpeed = 5f;
    [SerializeField] private float turningSpeed = 360f;
    [SerializeField] private LayerMask aimLayerMask;


    [Header("References")]
    [SerializeField] private InputManager inputManager;
    [SerializeField] private Rigidbody rb;


    private Vector3 moveDirection;
    private Vector3 lookingDirection;
    public override void OnNetworkSpawn()
    {
        if (!IsOwner) return;
        inputManager.MoveEvent += InputManager_MoveEvent;
    }



    public override void OnNetworkDespawn()
    {
        if(!IsOwner) return;
        inputManager.MoveEvent -= InputManager_MoveEvent;
    }
    private void Update()
    {
        if (!IsOwner) return;


    }
    private void FixedUpdate()
    {
        if (!IsOwner) return;
        HandleMove();
    }
    private void LateUpdate()
    {
        if (!IsOwner) return;
        HandleAim();
    }
    private void InputManager_MoveEvent(Vector2 moveInput)
    {
        moveDirection =  new Vector3(moveInput.x,0, moveInput.y);

    }
    private void HandleMove()
    {
        transform.position += moveDirection * movementSpeed * Time.deltaTime;
    }
    private void HandleAim() 
    {
        Ray ray = Camera.main.ScreenPointToRay(inputManager.AimPosition);
        if (Physics.Raycast(ray, out var hitInfo, Mathf.Infinity, aimLayerMask))
        {
            lookingDirection = hitInfo.point - transform.position;
            lookingDirection.y = 0f;
            lookingDirection.Normalize();

            transform.forward = lookingDirection;
        }
    }
}
