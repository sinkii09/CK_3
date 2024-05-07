using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class VisualizationConfiguration : ScriptableObject
{
    [Header("Animation Triggers")]
    [SerializeField] string m_AliveStateTrigger = "StandUp";
    [SerializeField] string m_FaintedStateTrigger = "FallDown";
    [SerializeField] string m_DeadStateTrigger = "Dead";
    [SerializeField] string m_AnticipateMoveTrigger = "AnticipateMove";

    [SerializeField] string m_SpeedVariable = "Speed";
    [SerializeField] string m_BaseNodeTag = "BaseNode";

    [Header("Animation Speeds")]
    public float SpeedDead = 0;
    public float SpeedIdle = 0;
    public float SpeedNormal = 1;
    public float SpeedHasted = 1.5f;
    public float SpeedSlowed = 2;

    [Header("Associated Resources")]
    public GameObject TargetReticule;
    public Material ReticuleFriendlyMat;
    public Material ReticuleHostileMat;

    [SerializeField][HideInInspector] public int AliveStateTriggerID;
    [SerializeField][HideInInspector] public int FaintedStateTriggerID;
    [SerializeField][HideInInspector] public int DeadStateTriggerID;
    [SerializeField][HideInInspector] public int AnticipateMoveTriggerID;
    [SerializeField][HideInInspector] public int SpeedVariableID;
    [SerializeField][HideInInspector] public int BaseNodeTagID;

    private void OnValidate()
    {
        AliveStateTriggerID = Animator.StringToHash(m_AliveStateTrigger);
        FaintedStateTriggerID = Animator.StringToHash(m_FaintedStateTrigger);
        DeadStateTriggerID = Animator.StringToHash(m_DeadStateTrigger);
        AnticipateMoveTriggerID = Animator.StringToHash(m_AnticipateMoveTrigger);

        SpeedVariableID = Animator.StringToHash(m_SpeedVariable);
        BaseNodeTagID = Animator.StringToHash(m_BaseNodeTag);
    }
}
