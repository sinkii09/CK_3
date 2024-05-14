using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

public class LobbyJoinUI : MonoBehaviour
{
    [SerializeField] LobbyListItemUI m_LobbyListItemPrototype;
    [SerializeField] TMP_InputField m_JoinCodeField;
    [SerializeField] CanvasGroup m_CanvasGroup;
    [SerializeField] Button m_JoinLobbyButton;
    [SerializeField] Graphic m_EmptyLobbyListLabel;

    IObjectResolver m_Container;
    LobbyUIMediator m_LobbyUIMediator;
    UpdateRunner m_UpdateRunner;
    ISubscriber<LobbyListFetchedMessage> m_LocalLobbiesRefreshedSub;
    List<LobbyListItemUI> m_LobbyListItems = new List<LobbyListItemUI>();

    [Inject]
    void InjectDependenciesAndInitialize(
        IObjectResolver container,LobbyUIMediator lobbyUIMediator,UpdateRunner updateRunner, ISubscriber<LobbyListFetchedMessage> localLobbiesRefreshedSub)
    {
        m_Container = container;
        m_LobbyUIMediator = lobbyUIMediator;
        m_UpdateRunner = updateRunner;
        m_LocalLobbiesRefreshedSub = localLobbiesRefreshedSub;
        m_LocalLobbiesRefreshedSub.Subscribe(UpdateUI);
    }
    private void Awake()
    {
        m_LobbyListItemPrototype.gameObject.SetActive(false);
    }

    private void OnDisable()
    {
        if (m_UpdateRunner != null)
        {
            m_UpdateRunner.Unsubscribe(PeriodicRefresh);
        }
    }

    private void OnDestroy()
    {
        if (m_LocalLobbiesRefreshedSub != null)
        {
            m_LocalLobbiesRefreshedSub.Unsubscribe(UpdateUI);
        }
    }

    public void OnJoinCodeInputTextChanged()
    {
        m_JoinCodeField.text = SanitizeJoinCode(m_JoinCodeField.text);
        m_JoinLobbyButton.interactable = m_JoinCodeField.text.Length > 0;
    }
    string SanitizeJoinCode(string dirtyString)
    {
        return Regex.Replace(dirtyString.ToUpper(), "[^A-Z0-9]", "");
    }
    public void OnJoinButtonPressed()
    {
        m_LobbyUIMediator.JoinLobbyWithCodeRequest(SanitizeJoinCode(m_JoinCodeField.text));
    }
    void PeriodicRefresh(float _)
    {
        //this is a soft refresh without needing to lock the UI and such
        m_LobbyUIMediator.QueryLobbiesRequest(false);
    }

    public void OnRefresh()
    {
        m_LobbyUIMediator.QueryLobbiesRequest(true);
    }

    void UpdateUI(LobbyListFetchedMessage message)
    {
        EnsureNumberOfActiveUISlots(message.LocalLobbies.Count);

        for (var i = 0; i < message.LocalLobbies.Count; i++)
        {
            var localLobby = message.LocalLobbies[i];
            m_LobbyListItems[i].SetData(localLobby);
        }

        if (message.LocalLobbies.Count == 0)
        {
            m_EmptyLobbyListLabel.enabled = true;
        }
        else
        {
            m_EmptyLobbyListLabel.enabled = false;
        }
    }

    void EnsureNumberOfActiveUISlots(int requiredNumber)
    {
        int delta = requiredNumber - m_LobbyListItems.Count;

        for (int i = 0; i < delta; i++)
        {
            m_LobbyListItems.Add(CreateLobbyListItem());
        }

        for (int i = 0; i < m_LobbyListItems.Count; i++)
        {
            m_LobbyListItems[i].gameObject.SetActive(i < requiredNumber);
        }
    }

    LobbyListItemUI CreateLobbyListItem()
    {
        var listItem = Instantiate(m_LobbyListItemPrototype.gameObject, m_LobbyListItemPrototype.transform.parent)
            .GetComponent<LobbyListItemUI>();
        listItem.gameObject.SetActive(true);

        m_Container.Inject(listItem);

        return listItem;
    }

    public void OnQuickJoinClicked()
    {
        m_LobbyUIMediator.QuickJoinRequest();
    }

    public void Show()
    {
        m_CanvasGroup.alpha = 1f;
        m_CanvasGroup.blocksRaycasts = true;
        m_JoinCodeField.text = "";
        m_UpdateRunner.Subscribe(PeriodicRefresh, 10f);
    }

    public void Hide()
    {
        m_CanvasGroup.alpha = 0f;
        m_CanvasGroup.blocksRaycasts = false;
        m_UpdateRunner.Unsubscribe(PeriodicRefresh);
    }
}
