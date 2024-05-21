using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIAvatarSelectSeat : MonoBehaviour
{
    private int m_SeatIndex;

    private int m_PlayerNumber;

    private NetworkCharSelection.SeatState m_State;

    private bool m_IsDisabled;

    public void Initialize(int seatIndex)
    {
        m_SeatIndex = seatIndex;
        m_PlayerNumber = -1;
        m_State = NetworkCharSelection.SeatState.Inactive;
    }

    public bool IsLocked()
    {
        return m_State == NetworkCharSelection.SeatState.LockedIn;
    }

    public void OnClicked()
    {
        ClientCharSelectState.Instance.OnPlayerClickedSeat(m_SeatIndex);
    }
}
