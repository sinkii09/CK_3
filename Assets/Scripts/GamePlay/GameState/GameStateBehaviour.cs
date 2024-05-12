using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VContainer.Unity;

public enum GameState
{
    MainMenu,
    CharSelect,
    Game01,
    Game02,
    Game03,
    PostGame,
}
public abstract class GameStateBehaviour : LifetimeScope
{
    public virtual bool Persists
    {
        get { return false; }
    }

    public abstract GameState ActiveState { get; }

    private static GameObject s_ActiveStateGO;

    protected override void Awake()
    {
        base.Awake();
        if (Parent != null)
        {
            Parent.Container.Inject(this);
        }
    }
    private void Start()
    {
        if (s_ActiveStateGO != null)
        {
            if (s_ActiveStateGO == gameObject)
            {
                return;
            }
            var previousState = s_ActiveStateGO.GetComponent<GameStateBehaviour>();
            if (previousState.Persists && previousState.ActiveState == ActiveState)
            {
                Destroy(gameObject);
                return;
            }
            Destroy(s_ActiveStateGO);
        }
        s_ActiveStateGO = gameObject;
        if (Persists)
        {
            DontDestroyOnLoad(gameObject);
        }
    }

    protected override void OnDestroy()
    {
        if (!Persists)
        {
            s_ActiveStateGO = null;
        }
    }
}
