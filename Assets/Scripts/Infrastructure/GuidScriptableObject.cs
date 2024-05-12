using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public abstract class GuidScriptableObject : ScriptableObject
{
    [HideInInspector]
    [SerializeField]
    byte[] m_Guid;

    public Guid Guid => new Guid(m_Guid);

    //void OnValidate()
    //{
    //    if (m_Guid.Length == 0)
    //    {
    //        m_Guid = Guid.NewGuid().ToByteArray();
    //    }
    //}

    protected GuidScriptableObject()
    {
        m_Guid = Guid.NewGuid().ToByteArray();
    }
}
