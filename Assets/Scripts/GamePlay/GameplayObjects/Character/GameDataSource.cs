using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameDataSource : MonoBehaviour
{
    public static GameDataSource Instance { get; private set; }

    [SerializeField]
    private CharacterClass[] m_CharacterData;

    Dictionary<CharacterTypeEnum, CharacterClass> m_CharacterDataMap;

    public Dictionary<CharacterTypeEnum, CharacterClass> CharacterDataByType
    {
        get 
        {
            if (m_CharacterDataMap == null)
            {
                m_CharacterDataMap = new Dictionary<CharacterTypeEnum, CharacterClass>();
                foreach(CharacterClass data in m_CharacterData)
                {
                    if(m_CharacterDataMap.ContainsKey(data.CharacterType))
                    {
                        throw new System.Exception($"Duplicate character definition detected: {data.CharacterType}");
                    }
                    m_CharacterDataMap[data.CharacterType] = data;
                }
            }
            return m_CharacterDataMap; 
        }
    }

    [Header("Common action prototypes")]
    [SerializeField]
    Action m_GeneralChaseActionPrototype;
    [SerializeField]
    Action m_GeneralTargetActionPrototype;
    [SerializeField]
    Action m_StunnedActionPrototype;
    public Action GeneralChaseActionPrototype => m_GeneralChaseActionPrototype;
    public Action GeneralTargetActionPrototype => m_GeneralTargetActionPrototype;
    public Action StunnedActionPrototype => m_StunnedActionPrototype;
    [SerializeField]
    private Action[] m_ActionPrototypes;

    List<Action> m_AllActions;
    private void Awake()
    {
        if (Instance != null)
        {
            throw new System.Exception("Multiple GameDataSources defined!");
        }

        BuildActionIDs();

        DontDestroyOnLoad(gameObject);
        Instance = this;

        
    }
    public Action GetActionPrototypeByID(ActionID index)
    {
        return m_AllActions[index.ID];
    }
    public bool TryGetActionPrototypeByID(ActionID index, out Action action)
    {
        for (int i = 0;i < m_AllActions.Count;i++)
        {
            if (m_AllActions[i].ActionID == index)
            {
                action = m_AllActions[i];
                return true;
            }
        }
        action = null;
        return false;
    }
    private void BuildActionIDs()
    {
        var uniqueActions = new HashSet<Action>(m_ActionPrototypes);
        uniqueActions.Add(GeneralChaseActionPrototype);
        uniqueActions.Add(GeneralTargetActionPrototype);
        //uniqueActions.Add(StunnedActionPrototype);


        m_AllActions = new List<Action>(uniqueActions.Count);

        int i = 0;
        foreach(var uniqueAction in uniqueActions)
        {
            uniqueAction.ActionID = new ActionID { ID = i };
            m_AllActions.Add(uniqueAction);
            i++;
        }
    }
}
