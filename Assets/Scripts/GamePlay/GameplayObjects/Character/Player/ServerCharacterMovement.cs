using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Assertions;

public enum MovementState
{
    Idle = 0,
    PathFollowing = 1,
    Charging = 2,
    Knockback = 3,
}
[Serializable]
public enum MovementStatus
{
    Idle,         // not trying to move
    Normal,       // character is moving (normally)
    Uncontrolled, // character is being moved by e.g. a knockback -- they are not in control!
    Slowed,       // character's movement is magically hindered
    Hasted,       // character's movement is magically enhanced
    Walking,      // character should appear to be "walking" rather than normal running (e.g. for cut-scenes)
}
public class ServerCharacterMovement : NetworkBehaviour
{
    [SerializeField]
    NavMeshAgent m_NavMeshAgent;

    [SerializeField]
    Rigidbody m_Rigidbody;

    [SerializeField]
    private ServerCharacter m_ServerCharacter;

    private NavigationSystem m_NavigationSystem;

    private DynamicNavPath m_NavPath;

    private MovementState m_MovementState;

    MovementStatus m_PreviousState;

    private float m_ForcedSpeed;
    private float m_SpecialModeDurationRemaining;
    private Vector3 m_KnockbackVector;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
    public bool TeleportModeActivated { get; set; }

    const float k_CheatSpeed = 20;

    public bool SpeedCheatActivated { get; set; }
#endif
    void Awake()
    {
        enabled = false;
    }

    public override void OnNetworkSpawn()
    {
        if(IsServer)
        {
            enabled = true;
            m_NavMeshAgent.enabled = true;
            m_NavigationSystem = GameObject.FindGameObjectWithTag(NavigationSystem.NavigationSystemTag).GetComponent<NavigationSystem>();
            m_NavPath = new DynamicNavPath(m_NavMeshAgent, m_NavigationSystem);
        }
    }

    public override void OnNetworkDespawn()
    {
        if (m_NavPath != null)
        {
            m_NavPath.Dispose();
        }
        if (IsServer)
        {
            // Disable server components when despawning
            enabled = false;
            m_NavMeshAgent.enabled = false;
        }
    }
    public void SetMovementTarget(Vector3 position)
    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        if (TeleportModeActivated)
        {
            Teleport(position);
            return;
        }
#endif
        m_MovementState = MovementState.PathFollowing;
        m_NavPath.SetTargetPosition(position);
    }
    public void FollowTransform(Transform followTransform)
    {
        m_MovementState = MovementState.PathFollowing;
        m_NavPath.FollowTransform(followTransform);
    }
    public bool IsMoving()
    {
        return m_MovementState != MovementState.Idle;
    }
    public void CancelMove()
    {
        if (m_NavPath != null)
        {
            m_NavPath.Clear();
        }
        m_MovementState = MovementState.Idle;
    }
    public void Teleport(Vector3 newPosition)
    {
        CancelMove();
        if (!m_NavMeshAgent.Warp(newPosition))
        {
            // warping failed! We're off the navmesh somehow. Weird... but we can still teleport
            Debug.LogWarning($"NavMeshAgent.Warp({newPosition}) failed!", gameObject);
            transform.position = newPosition;
        }

        m_Rigidbody.position = transform.position;
        m_Rigidbody.rotation = transform.rotation;
    }
    private void FixedUpdate()
    {
        PerformMovement();

        var currentState = GetMovementStatus(m_MovementState);
        if (m_PreviousState != currentState)
        {
            m_ServerCharacter.MovementStatus.Value = currentState;
            m_PreviousState = currentState;
        }
    }
    private void PerformMovement()
    {
        if (m_MovementState == MovementState.Idle)
            return;

        Vector3 movementVector;

        if (m_MovementState == MovementState.Charging)
        {
            // if we're done charging, stop moving
            m_SpecialModeDurationRemaining -= Time.fixedDeltaTime;
            if (m_SpecialModeDurationRemaining <= 0)
            {
                m_MovementState = MovementState.Idle;
                return;
            }

            var desiredMovementAmount = m_ForcedSpeed * Time.fixedDeltaTime;
            movementVector = transform.forward * desiredMovementAmount;
        }
        else if (m_MovementState == MovementState.Knockback)
        {
            m_SpecialModeDurationRemaining -= Time.fixedDeltaTime;
            if (m_SpecialModeDurationRemaining <= 0)
            {
                m_MovementState = MovementState.Idle;
                return;
            }

            var desiredMovementAmount = m_ForcedSpeed * Time.fixedDeltaTime;
            movementVector = m_KnockbackVector * desiredMovementAmount;
        }
        else
        {
            var desiredMovementAmount = GetBaseMovementSpeed() * Time.fixedDeltaTime;
            movementVector = m_NavPath.MoveAlongPath(desiredMovementAmount);
            // If we didn't move stop moving.
            if (movementVector == Vector3.zero)
            {
                m_MovementState = MovementState.Idle;
                return;
            }
        }

        m_NavMeshAgent.Move(movementVector);
        transform.rotation = Quaternion.LookRotation(movementVector);

        // After moving adjust the position of the dynamic rigidbody.
        m_Rigidbody.position = transform.position;
        m_Rigidbody.rotation = transform.rotation;
    }
    private float GetBaseMovementSpeed()
    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        if (SpeedCheatActivated)
        {
            return k_CheatSpeed;
        }
#endif
        CharacterClass characterClass = GameDataSource.Instance.CharacterDataByType[m_ServerCharacter.CharacterType];
        Assert.IsNotNull(characterClass, $"No CharacterClass data for character type {m_ServerCharacter.CharacterType}");
        return characterClass.Speed;

    }
    private MovementStatus GetMovementStatus(MovementState movementState)
    {
        switch (movementState)
        {
            case MovementState.Idle:
                return MovementStatus.Idle;
            case MovementState.Knockback:
                return MovementStatus.Uncontrolled;
            default:
                return MovementStatus.Normal;
        }
    }
}
