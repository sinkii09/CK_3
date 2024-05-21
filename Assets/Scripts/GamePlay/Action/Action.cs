using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public enum BlockingModeType
{
    EntireDuration,
    OnlyDuringExecTime,
}
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

    public int m_CurrentAmount;

    public bool IsGeneralTargetAction => ActionID == GameDataSource.Instance.GeneralTargetActionPrototype.ActionID;
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
    public abstract bool OnUpdate(ServerCharacter serverCharacter);
    public virtual void End(ServerCharacter serverCharacter)
    {
        Cancel(serverCharacter);
    }
    public virtual void Cancel(ServerCharacter serverCharacter)
    {

    }
    public virtual bool ShouldBecomeNonBlocking()
    {
        return Config.BlockingMode == BlockingModeType.OnlyDuringExecTime ? TimeRunning >= Config.ExecTimeSeconds : false;
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

    public static float GetUnbuffedValue(Action.BuffableValue buffType)
    {
        switch (buffType)
        {
            case BuffableValue.PercentDamageReceived: return 1;
            case BuffableValue.PercentHealingReceived: return 1;
            case BuffableValue.ChanceToStunTramplers: return 0;
            default: throw new System.Exception($"Unknown buff type {buffType}");
        }
    }
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
        AnticipatedClient = false; 
        TimeStarted = Time.time;
        return true;
    }

    public virtual bool OnUpdateClient(ClientCharacter clientCharacter)
    {
        return ActionConclusion.Continue;
    }
    public virtual void EndClient(ClientCharacter clientCharacter)
    {
        CancelClient(clientCharacter);
    }
    public virtual void CancelClient(ClientCharacter clientCharacter) { }

    public virtual void OnAnimEventClient(ClientCharacter clientCharacter, string id) { }
    public virtual void OnStoppedChargingUpClient(ClientCharacter clientCharacter, float finalChargeUpPercentage) { }
    protected List<SpecialFXGraphic> InstantiateSpecialFXGraphics(Transform origin, bool parentToOrigin)
    {
        var returnList = new List<SpecialFXGraphic>();
        foreach (var prefab in Config.SpecialFX)
        {
            if (!prefab) { continue; } // skip blank entries in our prefab list
            returnList.Add(InstantiateSpecialFXGraphic(prefab, origin, parentToOrigin));
        }
        return returnList;
    }
    protected SpecialFXGraphic InstantiateSpecialFXGraphic(GameObject prefab, Transform origin, bool parentToOrigin)
    {
        if (prefab.GetComponent<SpecialFXGraphic>() == null)
        {
            throw new System.Exception($"One of the Spawns on action {this.name} does not have a SpecialFXGraphic component and can't be instantiated!");
        }
        var graphicsGO = GameObject.Instantiate(prefab, origin.transform.position, origin.transform.rotation, (parentToOrigin ? origin.transform : null));
        return graphicsGO.GetComponent<SpecialFXGraphic>();
    }
    public virtual void AnticipateActionClient(ClientCharacter clientCharacter) 
    {
        AnticipatedClient = true;
        TimeStarted = Time.time;

        if(!string.IsNullOrEmpty(Config.AnimAnticipation))
        {
            clientCharacter.OurAnimator.SetTrigger(Config.AnimAnticipation);
        }
        
    }  
}
