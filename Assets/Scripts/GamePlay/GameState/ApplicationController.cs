using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using VContainer;
using VContainer.Unity;

public class ApplicationController : LifetimeScope
{
    //[SerializeField]
    //UpdateRunner m_UpdateRunner;
    [SerializeField]
    ConnectionManager m_ConnectionManager;
    [SerializeField]
    NetworkManager m_NetworkManager;


    protected override void Configure(IContainerBuilder builder)
    {
        base.Configure(builder);
        builder.RegisterComponent(m_ConnectionManager);
        builder.RegisterComponent(m_NetworkManager);

        builder.Register<PersistentGameState>(Lifetime.Singleton);

        builder.RegisterComponent(new NetworkedMessageChannel<LifeStateChangedEventMessage>()).AsImplementedInterfaces();
    }
    private void Start()
    {
        DontDestroyOnLoad(gameObject);

        SceneManager.LoadScene("MainMenu");
    }
    protected override void OnDestroy()
    {
        base.OnDestroy();
    }
}
