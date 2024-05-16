using System;
using Unity.Services.Authentication;
using UnityEngine;
using UnityEngine.UI;
using VContainer;
using VContainer.Unity;

public class ClientMainMenuState : GameStateBehaviour
{
    public override GameState ActiveState => GameState.MainMenu;

    [SerializeField]
    NameGenerationData m_NameGenerationData;
    [SerializeField]
    LobbyUIMediator m_LobbyUIMediator;
    [SerializeField]
    Button m_LobbyButton;
    //[SerializeField]
    //GameObject m_Spinner;
    [SerializeField]
    CanvasGroup m_CanvasGroup;

    [Inject]
    LocalLobbyUser m_LocalUser;
    [Inject]
    LocalLobby m_LocalLobby;
    [Inject]
    AuthenticationServiceFacade m_AuthServiceFacade;
    protected override void Awake()
    {
        base.Awake();

        m_LobbyUIMediator.Hide();

        TrySignIn();
    }

    protected override void Configure(IContainerBuilder builder)
    {
        base.Configure(builder);
        builder.RegisterComponent(m_LobbyUIMediator);
        builder.RegisterComponent(m_NameGenerationData);
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
    }
    private async void TrySignIn()
    {
        try
        {
            //TODO
            var unityAuthenticationInitOptions = m_AuthServiceFacade.GenerateAuthenticationOptions("SomeOne");
            await m_AuthServiceFacade.InitializeAndSignInAsync(unityAuthenticationInitOptions);
            OnAuthSignIn();
        
        }
        catch (Exception)
        {
            OnSignInFailed();
        }
    }

    private void OnAuthSignIn()
    {
        m_LocalUser.ID = AuthenticationService.Instance.PlayerId;
        m_LocalLobby.AddUser(m_LocalUser);
        Debug.Log($"Signed in. Unity Player ID {AuthenticationService.Instance.PlayerId}");
    }

    private void OnSignInFailed()
    {
    }

    public void OnstartClicked()
    {
        m_LobbyUIMediator.ToggleJoinLobbyUI();
        m_LobbyUIMediator.Show();
        m_CanvasGroup.alpha = 0f;
        m_CanvasGroup.blocksRaycasts = false;
    }
}
