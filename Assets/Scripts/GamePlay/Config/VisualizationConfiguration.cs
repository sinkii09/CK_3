using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class VisualizationConfiguration : ScriptableObject
{

    [SerializeField] string m_AnticipateMoveTrigger = "AnticipateMove";

    [SerializeField] string m_SpeedVariable = "Speed";
    [SerializeField] string m_BaseNodeTag = "BaseNode";

    public float SpeedDead = 0;
    public float SpeedIdle = 0;
    public float SpeedNormal = 1;
    public float SpeedHasted = 1.5f;
    public float SpeedSlowed = 2;


    [SerializeField][HideInInspector] public int AnticipateMoveTriggerID;
    [SerializeField][HideInInspector] public int SpeedVariableID;
    [SerializeField][HideInInspector] public int BaseNodeTagID;

    private void OnValidate()
    {
        AnticipateMoveTriggerID = Animator.StringToHash(m_AnticipateMoveTrigger);

        SpeedVariableID = Animator.StringToHash(m_SpeedVariable);
        BaseNodeTagID = Animator.StringToHash(m_BaseNodeTag);
    }
}
