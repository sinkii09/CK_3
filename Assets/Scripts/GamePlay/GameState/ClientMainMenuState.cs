using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VContainer;

public class ClientMainMenuState : GameStateBehaviour
{
    public override GameState ActiveState => GameState.MainMenu;

    protected override void Awake()
    {
        base.Awake();
    }

    protected override void Configure(IContainerBuilder builder)
    {
        base.Configure(builder);
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
    }
}
