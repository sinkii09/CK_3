using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using VContainer;
using VContainer.Unity;

public class ApplicationController : LifetimeScope
{
    [SerializeField]
    UpdateRunner m_UpdateRunner;
    [SerializeField]
    ConnectionManager m_ConnectionManager;
    [SerializeField]
    NetworkManager m_NetworkManager;

    LocalLobby m_LocalLobby;
    LobbyServiceFacade m_LobbyServiceFacade;

    IDisposable m_Subscriptions;
    protected override void Configure(IContainerBuilder builder)
    {
        base.Configure(builder);
        builder.RegisterComponent(m_ConnectionManager);
        builder.RegisterComponent(m_NetworkManager);
        builder.RegisterComponent(m_UpdateRunner);

        builder.Register<LocalLobbyUser>(Lifetime.Singleton);
        builder.Register<LocalLobby>(Lifetime.Singleton);

        builder.Register<PersistentGameState>(Lifetime.Singleton);

        builder.RegisterInstance(new MessageChannel<UnityServiceErrorMessage>()).AsImplementedInterfaces();

        builder.RegisterComponent(new NetworkedMessageChannel<LifeStateChangedEventMessage>()).AsImplementedInterfaces();
        builder.RegisterComponent(new NetworkedMessageChannel<ConnectionEventMessage>()).AsImplementedInterfaces();

        builder.RegisterInstance(new MessageChannel<ConnectStatus>()).AsImplementedInterfaces();

        builder.RegisterInstance(new BufferedMessageChannel<LobbyListFetchedMessage>()).AsImplementedInterfaces();

        builder.Register<AuthenticationServiceFacade>(Lifetime.Singleton);
        builder.RegisterEntryPoint<LobbyServiceFacade>(Lifetime.Singleton).AsSelf();
    }
    private void Start()
    {
        m_LocalLobby = Container.Resolve<LocalLobby>();
        m_LobbyServiceFacade = Container.Resolve<LobbyServiceFacade>();

        DontDestroyOnLoad(gameObject);
        DontDestroyOnLoad(m_UpdateRunner.gameObject);

        SceneManager.LoadScene("MainMenu");
    }
    protected override void OnDestroy()
    {
        if (m_Subscriptions != null)
        {
            m_Subscriptions.Dispose();
        }
        if (m_LobbyServiceFacade != null)
        {
            m_LobbyServiceFacade.EndTracking();
        }
        base.OnDestroy();
    }
}
