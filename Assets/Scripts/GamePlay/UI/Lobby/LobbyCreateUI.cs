using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

public class LobbyCreateUI : MonoBehaviour
{
    [SerializeField] TMP_InputField m_LobbyNameInputField;
    [SerializeField] GameObject m_LoadingIndicatorObject;
    [SerializeField] Toggle m_IsPrivate;
    [SerializeField] CanvasGroup m_CanvasGroup;
    [Inject] LobbyUIMediator m_LobbyUIMediator;

    private void Awake()
    {
        EnableUnityRelayUI();
    }

    private void EnableUnityRelayUI()
    {
        m_LoadingIndicatorObject.SetActive(false);
    }
    public void OnCreateClick()
    {
        m_LobbyUIMediator.CreateLobbyRequest(m_LobbyNameInputField.text, m_IsPrivate);
    }
    public void Show()
    {
        m_CanvasGroup.alpha = 1;
        m_CanvasGroup.blocksRaycasts = true;
    }
    public void Hide()
    {
        m_CanvasGroup.alpha = 0;
        m_CanvasGroup.blocksRaycasts = false;
    }
}

