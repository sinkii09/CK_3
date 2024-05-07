using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public partial class TargetAction 
{
    private const float k_ReticuleGroundHeight = 0.2f;

    private GameObject m_TargetReticule;
    private ulong m_CurrentTarget;
    private ulong m_NewTarget;

    public override void CancelClient(ClientCharacter clientCharacter)
    {
        Destroy(m_TargetReticule);
        clientCharacter.serverCharacter.TargetId.OnValueChanged -= OnTargetChanged;
        if (clientCharacter.TryGetComponent(out ClientInputSender inputSender))
        {
            inputSender.ActionInputEvent -= OnActionInput;
        }

    }

    public override bool OnStartClient(ClientCharacter clientCharacter)
    {
        base.OnStartClient(clientCharacter);
        clientCharacter.serverCharacter.TargetId.OnValueChanged += OnTargetChanged;
        clientCharacter.serverCharacter.GetComponent<ClientInputSender>().ActionInputEvent += OnActionInput;

        return true;
    }

    public override bool OnUpdateClient(ClientCharacter clientCharacter)
    {
        if(m_CurrentTarget != m_NewTarget)
        {
            m_CurrentTarget = m_NewTarget;
            if(NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(m_CurrentTarget, out NetworkObject targetObject))
            {
                var targetEntity = targetObject != null ? targetObject.GetComponent<ITargetable>() : null;
                if (targetEntity != null)
                {
                    validateReticule(clientCharacter, targetObject);
                    m_TargetReticule.SetActive(true);

                    var parentTransform = targetObject.transform;
                    if(targetObject.TryGetComponent(out ServerCharacter serverCharacter) && serverCharacter.clientCharacter)
                    {
                        parentTransform = serverCharacter.clientCharacter.transform;
                    }

                    m_TargetReticule.transform.parent = parentTransform;
                    m_TargetReticule.transform.localPosition = new Vector3(0, k_ReticuleGroundHeight, 0);
                }
                else
                {
                    if(m_TargetReticule != null)
                    {
                        m_TargetReticule.transform.parent = null;
                        m_TargetReticule.SetActive(false);
                    }
                }
            }
        }
        return true;
    }
    void validateReticule(ClientCharacter parent, NetworkObject targetObject)
    {
        if(m_TargetReticule ==  null)
        {
            m_TargetReticule = Instantiate(parent.TargetReticulePrefab);
            bool target_isNpc = targetObject.GetComponent<ITargetable>().IsNPC;
            bool myself_isNpc = parent.serverCharacter.CharacterClass.IsNpc;
            bool hostile = target_isNpc != myself_isNpc;

            m_TargetReticule.GetComponent<MeshRenderer>().material = hostile ? parent.ReticuleHostileMat : parent.ReticuleFriendlyMat;
        }
    }
    private void OnTargetChanged(ulong oldTarget, ulong newTarget)
    {
        m_NewTarget = newTarget;
    }

    private void OnActionInput(ActionRequestData data)
    {
        if (GameDataSource.Instance.GetActionPrototypeByID(data.ActionID).IsGeneralTargetAction)
        {
            m_NewTarget = data.TargetIds[0];
        }
    }
}
