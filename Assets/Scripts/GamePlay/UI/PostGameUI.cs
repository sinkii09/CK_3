using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using VContainer;

public class PostGameUI : MonoBehaviour
{
    [SerializeField]
    private GameObject m_ReplayButton;


    ServerPostGameState m_PostGameState;

    [Inject]
    void Inject(ServerPostGameState postGameState)
    {
        m_PostGameState = postGameState;

        if(NetworkManager.Singleton.IsHost)
        {
            m_ReplayButton.SetActive(true);
        }
        else
        {
            m_ReplayButton.SetActive(false);
        }
    }
    public void OnPlayAgainClicked()
    {
        m_PostGameState.PlayAgain();
    }
    public void OnMainMenuClicked()
    {
        m_PostGameState.GoToMainMenu();
    }
}
