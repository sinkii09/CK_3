using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Action : ScriptableObject
{
    [NonSerialized]
    public ActionID ActionID;

    public const string k_DefaultHitReact = "HitReact1";

    protected ActionRequestData m_Data;

    public float TimeStarted { get; set; }

    public float TimeRunning { get { return (Time.time - TimeStarted); } }

    public ref ActionRequestData Data => ref m_Data;

    public ActionConfig Config;

    public void Initialize(ref ActionRequestData data)
    {
        m_Data = data;
        ActionID = data.ActionID;
    }
    public virtual void Reset()
    {
        m_Data = default;
        ActionID = default;
        TimeStarted = 0;
    }

    public abstract bool OnStart(ServerCharacter serverCharacter);
    public abstract bool OnUpdate(ServerCharacter clientCharacter);
    public virtual void End(ServerCharacter serverCharacter)
    {
        Cancel(serverCharacter);
    }
    public virtual void Cancel(ServerCharacter serverCharacter)
    {

    }
    public virtual bool ShouldBecomeNonBlocking()
    {
        return true;
    }
    public virtual bool ChainIntoNewAction(ref ActionRequestData newAction) { return false; }
    
    public virtual void CollisionEntered(ServerCharacter serverCharacter, Collision collision) { }
    
    public enum BuffableValue
    {
        PercentHealingReceived, // unbuffed value is 1.0. Reducing to 0 would mean "no healing". 2 would mean "double healing"
        PercentDamageReceived,  // unbuffed value is 1.0. Reducing to 0 would mean "no damage". 2 would mean "double damage"
        ChanceToStunTramplers,  // unbuffed value is 0. If > 0, is the chance that someone trampling this character becomes stunned
    }
    public virtual void BuffValue(BuffableValue buffType, ref float buffedValue) { }
    public enum GameplayActivity
    {
        AttackedByEnemy,
        Healed,
        StoppedChargingUp,
        UsingAttackAction, 
    }

    public virtual void OnGameplayActivity(ServerCharacter serverCharacter, GameplayActivity activityType) { }
    public bool AnticipatedClient { get; protected set; }

    public virtual bool OnStartClient(ClientCharacter clientCharacter)
    {
        AnticipatedClient = false; //once you start for real you are no longer an anticipated action.
        TimeStarted = UnityEngine.Time.time;
        return true;
    }

    //public virtual bool OnUpdateClient(ClientCharacter clientCharacter)
    //{
    //    return ActionConclusion.Continue;
    //}
    public virtual void EndClient(ClientCharacter clientCharacter)
    {
        CancelClient(clientCharacter);
    }
    public virtual void CancelClient(ClientCharacter clientCharacter) { }
}
