using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameDataSource : MonoBehaviour
{
    public static GameDataSource Instance { get; private set; }

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
    private void BuildActionIDs()
    {
        var uniqueActions = new HashSet<Action>(m_ActionPrototypes);
        m_AllActions = new List<Action>();
        int i = 0;
        foreach(var uniqueAction in uniqueActions)
        {
            uniqueAction.ActionID = new ActionID { ID = i };
            m_AllActions.Add(uniqueAction);
            i++;
        }
    }
}
