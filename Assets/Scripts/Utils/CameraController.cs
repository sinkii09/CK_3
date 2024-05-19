using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class CameraController : MonoBehaviour
{
    private CinemachineVirtualCamera m_MainCamera;
    private void Start()
    {
        AttachCamera();
    }
    private void AttachCamera()
    {
        m_MainCamera = FindObjectOfType<CinemachineVirtualCamera>();
       
        if (m_MainCamera)
        {
            m_MainCamera.Follow = transform;
        }
    }
}
