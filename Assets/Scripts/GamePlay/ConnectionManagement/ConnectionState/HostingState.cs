using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VContainer;

public class HostingState : OnlineState
{
    [Inject]
    LobbyServiceFacade m_LobbyServiceFacade;
    [Inject]
    IPublisher<ConnectionEventMessage> m_ConnectionEventPublisher;

    public override void Enter()
    {
        SceneLoaderWrapper.Instance.LoadScene("CharacterSelect", true);

        if(m_LobbyServiceFacade.CurrentUnityLobby != null)
        {
            m_LobbyServiceFacade.BeginTracking();
        }
    }
}
