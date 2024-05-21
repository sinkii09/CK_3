using System.Collections;
using System.Collections.Generic;
using Avatar = Unity.CK.GamePlay.Configuration.Avatar;
using Unity.Multiplayer.Samples.Utilities;
using Unity.Netcode;
using UnityEngine;
using System;
using VContainer;

public class ClientCharSelectState : GameStateBehaviour
{
    public static ClientCharSelectState Instance { get; private set; }
    public override GameState ActiveState { get { return GameState.CharSelect; } }

    Dictionary<Guid, GameObject> m_SpawnedCharacterGraphics = new Dictionary<Guid, GameObject>();
    
    [SerializeField]
    List<UIAvatarSelectSeat> m_PlayerSeats;
    #region Components

    [SerializeField]
    NetcodeHooks m_NetcodeHooks;


    [SerializeField]
    NetworkCharSelection m_NetworkCharSelection;

    [Inject]
    ConnectionManager m_ConnectionManager;
    #endregion

    int m_LastSeatSelected = -1;
    bool m_HasLocalPlayerLockedIn = false;
    protected override void Awake()
    {
        base.Awake();

        Instance = this;

        m_NetcodeHooks.OnNetworkSpawnHook += OnNetworkSpawn;
        m_NetcodeHooks.OnNetworkDespawnHook += OnNetworkDespawn;
    }
    protected override void Start()
    {
        base.Start();
        for (int i = 0; i < m_PlayerSeats.Count; ++i)
        {
            m_PlayerSeats[i].Initialize(i);
        }
        UpdateCharacterSelection(NetworkCharSelection.SeatState.Inactive);
    }
    protected override void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }

        base.OnDestroy();
    }
    #region NetworkCallBack
    void OnNetworkSpawn()
    {
        if(!NetworkManager.Singleton.IsClient)
        {
            enabled = false;
        }
        else
        {
            m_NetworkCharSelection.IsLobbyClosed.OnValueChanged += OnLobbyClosedChanged;
            m_NetworkCharSelection.LobbyPlayers.OnListChanged += OnLobbyPlayerStateChanged;
        }
    }
    void OnNetworkDespawn()
    {
        if(m_NetworkCharSelection)
        {
            m_NetworkCharSelection.IsLobbyClosed.OnValueChanged -= OnLobbyClosedChanged;
            m_NetworkCharSelection.LobbyPlayers.OnListChanged -= OnLobbyPlayerStateChanged;
        }
    }
    #endregion
    private void OnLobbyPlayerStateChanged(NetworkListEvent<NetworkCharSelection.PlayerStatus> changeEvent)
    {

    }

    private void OnLobbyClosedChanged(bool previousValue, bool newValue)
    {

    }
    void UpdateCharacterSelection(NetworkCharSelection.SeatState state,int seatIdx = -1)
    {
        bool isNewSeat = m_LastSeatSelected != seatIdx;

        m_LastSeatSelected = seatIdx;
        if (state == NetworkCharSelection.SeatState.Inactive)
        {

        }
        else            // thay doi seat
        {
            if(seatIdx != -1)
            {
                // khac voi seat truoc
                if (isNewSeat)
                {

                }
            }
            if(state == NetworkCharSelection.SeatState.LockedIn && !m_HasLocalPlayerLockedIn)
            {
                m_HasLocalPlayerLockedIn = true;
            }
            else if(m_HasLocalPlayerLockedIn && state == NetworkCharSelection.SeatState.Active)
            {
                if(m_HasLocalPlayerLockedIn)
                {
                    m_HasLocalPlayerLockedIn = false;
                }
            }
            else if(state == NetworkCharSelection.SeatState.Active && isNewSeat)
            {

            }
        }
    }
    public void OnPlayerClickedSeat(int seatIdx)
    {
        if (m_NetworkCharSelection.IsSpawned)
        {
            m_NetworkCharSelection.ChangeSeatServerRPC(NetworkManager.Singleton.LocalClientId, seatIdx, false);
        }
    }
    public void OnPlayerClickedReady()
    {
        if (m_NetworkCharSelection.IsSpawned)
        {
            m_NetworkCharSelection.ChangeSeatServerRPC(NetworkManager.Singleton.LocalClientId, m_LastSeatSelected, true);
        }
    }

    GameObject GetCharacterGraphics(Avatar avatar)
    {
        if(!m_SpawnedCharacterGraphics.TryGetValue(avatar.Guid, out GameObject CharacterGraphics ))
        {
            CharacterGraphics = Instantiate(avatar.GraphicsCharacterSelect);
        }    
        return CharacterGraphics;
    }
}
