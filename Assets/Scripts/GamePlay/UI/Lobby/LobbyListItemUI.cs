using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using VContainer;

public class LobbyListItemUI : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI m_lobbyNameText;
    [SerializeField] TextMeshProUGUI m_lobbyCountText;

    [Inject] LobbyUIMediator m_LobbyUIMediator;

    LocalLobby m_Data;

    public void SetData(LocalLobby lobby)
    {
        m_Data = lobby;
        m_lobbyNameText.SetText(lobby.LobbyName);
        m_lobbyCountText.SetText($"{lobby.PlayerCount}/{lobby.MaxPlayerCount}");
    }
    public void OnClick()
    {
        m_LobbyUIMediator.JoinLobbyRequest(m_Data);
    }
    
}
