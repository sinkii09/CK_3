using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VContainer;
using Unity.Services.Core;
using UnityEngine.UI;
using Unity.Services.Lobbies.Models;

public class LobbyUIMediator : MonoBehaviour
{
    [SerializeField] CanvasGroup m_CanvasGroup;
    [SerializeField] CanvasGroup m_MainMenuCanvasGroup;
    [SerializeField] GameObject m_LoadingSpinner;
    [SerializeField] LobbyJoinUI m_LobbyJoiningUI;
    [SerializeField] LobbyCreateUI m_LobbyCreationUI;
    [SerializeField] GameObject m_CreateButtonGO;
    [SerializeField] GameObject m_JoinButtonGO;

    AuthenticationServiceFacade m_AuthenticationServiceFacade;
    LobbyServiceFacade m_LobbyServiceFacade;
    LocalLobbyUser m_LocalUser;
    LocalLobby m_LocalLobby;
    ConnectionManager m_ConnectionManager;
    NameGenerationData m_NameGenerationData;

    ISubscriber<ConnectStatus> m_ConnectStatusSubscriber;

    const string k_DefaultLobbyName = "no-name";

    [Inject]
    void InjectDependenciesAndInitialize(
        AuthenticationServiceFacade authenticationServiceFacade,
        LobbyServiceFacade lobbyServiceFacade,
        LocalLobbyUser localLobbyUser,
        LocalLobby localLobby,
        ConnectionManager connectionManager,
        NameGenerationData nameGenerationData,
        ISubscriber<ConnectStatus> connectStatusSubscriber
        )
    {
        m_AuthenticationServiceFacade = authenticationServiceFacade;
        m_LobbyServiceFacade = lobbyServiceFacade;
        m_LocalUser = localLobbyUser;
        m_LocalLobby = localLobby;
        m_ConnectionManager = connectionManager;
        m_ConnectStatusSubscriber = connectStatusSubscriber;
        m_NameGenerationData = nameGenerationData;
        RegerateName();

        m_ConnectStatusSubscriber.Subscribe(OnConnectStatus);
    }
    private void OnDestroy()
    {
        if(m_ConnectStatusSubscriber != null)
        {
            m_ConnectStatusSubscriber.Unsubscribe(OnConnectStatus);
        }
    }

    private void OnConnectStatus(ConnectStatus status)
    {
        if (status is ConnectStatus.GenericDisconnect or ConnectStatus.StartClientFailed)
        {
            UnblockUIAfterLoadingIsComplete();
        }
    }

    private void UnblockUIAfterLoadingIsComplete()
    {
        if (m_CanvasGroup != null)
        {
            m_CanvasGroup.interactable = true;
            m_LoadingSpinner.SetActive(false);
        }
    }
    private void BlockUIWhileLoadingIsInProgress()
    {
        m_CanvasGroup.interactable = false;
        m_LoadingSpinner.SetActive(true);
    }
    public async void CreateLobbyRequest(string lobbyName, bool isPrivate)
    {
        if(string.IsNullOrEmpty(lobbyName))
        {
            lobbyName = k_DefaultLobbyName;
        }

        BlockUIWhileLoadingIsInProgress();

        bool playerIsAuthorized = await m_AuthenticationServiceFacade.EnsurePlayerIsAuthorized();

        if(!playerIsAuthorized)
        {
            UnblockUIAfterLoadingIsComplete();
            return;
        }

        var lobbyCreationAttempt = await m_LobbyServiceFacade.TryCreateLobbyAsync(lobbyName, m_ConnectionManager.MaxConnectedPlayers, isPrivate);

        if(lobbyCreationAttempt.Success)
        {
            m_LocalUser.IsHost = true;
            m_LobbyServiceFacade.SetRemoteLobby(lobbyCreationAttempt.Lobby);

            Debug.Log($"Created lobby with ID: {m_LocalLobby.LobbyID} and code {m_LocalLobby.LobbyCode}");

            m_ConnectionManager.StartHostLobby(m_LocalUser.DisplayName);
        }
        else
        {
            UnblockUIAfterLoadingIsComplete();
        }

    }

    public async void QueryLobbiesRequest(bool blockUI)
    {
        if(Unity.Services.Core.UnityServices.State != ServicesInitializationState.Initialized)
        {
            return;
        }

        if (blockUI)
        {
            BlockUIWhileLoadingIsInProgress();
        }
        bool playerIsAuthorized = await m_AuthenticationServiceFacade.EnsurePlayerIsAuthorized();
        if (blockUI && !playerIsAuthorized)
        {
            UnblockUIAfterLoadingIsComplete();
            return;
        }

        await m_LobbyServiceFacade.RetrieveAndPublishLobbyListAsync();

        if(blockUI && !playerIsAuthorized)
        {
            UnblockUIAfterLoadingIsComplete();
        }
    }

    public async void JoinLobbyWithCodeRequest(string lobbyCode)
    {
        BlockUIWhileLoadingIsInProgress();

        bool playerIsAuthorized = await m_AuthenticationServiceFacade.EnsurePlayerIsAuthorized();
        
        if(!playerIsAuthorized)
        {
            UnblockUIAfterLoadingIsComplete();
            return;
        }

        var result = await m_LobbyServiceFacade.TryJoinLobbyAsync(null, lobbyCode);

        if(result.Success)
        {
            OnJoinedLobby(result.Lobby);
        }
        else
        {
            UnblockUIAfterLoadingIsComplete();
        }
    }

    public async void JoinLobbyRequest(LocalLobby lobby)
    {
        BlockUIWhileLoadingIsInProgress();
        bool playerIsAuthorized = await m_AuthenticationServiceFacade.EnsurePlayerIsAuthorized();

        if (!playerIsAuthorized)
        {
            UnblockUIAfterLoadingIsComplete();
            return;
        }
        var result = await m_LobbyServiceFacade.TryJoinLobbyAsync(lobby.LobbyID, lobby.LobbyCode);
        if (result.Success)
        {
            OnJoinedLobby(result.Lobby);
        }
        else
        {
            UnblockUIAfterLoadingIsComplete();
        }
    }
    public async void QuickJoinRequest()
    {
        BlockUIWhileLoadingIsInProgress();
        bool playerIsAuthorized = await m_AuthenticationServiceFacade.EnsurePlayerIsAuthorized();

        if (!playerIsAuthorized)
        {
            UnblockUIAfterLoadingIsComplete();
            return;
        }

        var result = await m_LobbyServiceFacade.TryQuickJoinLobbyAsync();

        if (result.Success)
        {
            Debug.Log("Joined");
            OnJoinedLobby(result.Lobby);
        }
        else
        {
            Debug.Log("JoinError");
            UnblockUIAfterLoadingIsComplete();
        }
    }
    void OnJoinedLobby(Lobby remoteLobby)
    {
        m_LobbyServiceFacade.SetRemoteLobby(remoteLobby);
        Debug.Log($"Joined lobby with code: {m_LocalLobby.LobbyCode}, Internal Relay Join Code{m_LocalLobby.RelayJoinCode}");

        m_ConnectionManager.StartClientLobby(m_LocalUser.DisplayName);
    }
    public void Show()
    {
        m_CanvasGroup.alpha = 1f;
        m_CanvasGroup.blocksRaycasts = true;
    }
    public void Hide()
    {
        m_CanvasGroup.alpha = 0f;
        m_CanvasGroup.blocksRaycasts = false;
        m_LobbyCreationUI.Hide();
        m_LobbyJoiningUI.Hide();
    }
    public void ToggleJoinLobbyUI()
    {
        m_LobbyJoiningUI.Show();
        m_LobbyCreationUI.Hide();
        m_JoinButtonGO.GetComponent<Button>().Select();
        m_CreateButtonGO.GetComponent<Image>().color = Color.HSVToRGB(0, 0, 38);
    }
    public void ToggleCreateLobbyUI()
    {
        m_LobbyJoiningUI.Hide();
        m_LobbyCreationUI.Show();
        m_JoinButtonGO.GetComponent<Image>().color = Color.HSVToRGB(0, 0, 38);
        m_CreateButtonGO.GetComponent<Image>().color = Color.HSVToRGB(0, 0, 10);
    }
    public void RegerateName()
    {
        m_LocalUser.DisplayName = m_NameGenerationData.GenerateName();
    }
    public void BackToMainMenu()
    {
        Hide();
        m_MainMenuCanvasGroup.alpha = 1f;
        m_MainMenuCanvasGroup.blocksRaycasts = true;
    }
}
