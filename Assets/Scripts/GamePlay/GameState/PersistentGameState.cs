using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum WinState
{
    Invalid,
    Win,
    Loss
}
public class PersistentGameState 
{
    public WinState WinState { get; private set; }

    public void SetWinState(WinState winState)
    {
        WinState = winState;
    }

    public void Reset()
    {
        WinState = WinState.Invalid;
    }
}
