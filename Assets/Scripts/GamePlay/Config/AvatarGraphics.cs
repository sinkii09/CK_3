using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public enum HandSlot
{
    LeftHand,
    RightHand,
}
public class AvatarGraphics : MonoBehaviour
{
    [SerializeField] Transform LeftSlot;
    [SerializeField] Transform RightSlot;
    private void Start()
    {
        
    }
    public void ClearHandSlot()
    {
        ClearChild(LeftSlot.transform);
        ClearChild(RightSlot.transform);
    }
    public Transform GetHandSlot(HandSlot handSlot)
    {
        switch (handSlot)
        {
            case HandSlot.LeftHand:
                ClearChild(LeftSlot.transform);
                return LeftSlot;
            case HandSlot.RightHand:
                ClearChild (RightSlot.transform);
                return RightSlot;
            default: return null;
        }
    }
    void ClearChild(Transform transform)
    {
        foreach (Transform child in transform)
        {
            if (child != null)
            {
                Destroy(child.gameObject);
            }
        }
    }
}
