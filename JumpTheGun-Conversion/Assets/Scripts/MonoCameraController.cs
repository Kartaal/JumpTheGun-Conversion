using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class MonoCameraController : MonoBehaviour
{
    public static MonoCameraController instance;
    
    private Camera mainCamera;
    private Vector3 targetPosition;
    
    [SerializeField] private Vector3 offset;
    
    
    private void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
        }
        else
        {
            instance = this;
        }
    }
    
    void Start()
    {
        mainCamera = Camera.main;
        mainCamera.gameObject.transform.position = offset;
    }
    
    private void LateUpdate()
    {
        mainCamera.gameObject.transform.position = targetPosition + offset;
    }

    // invoked from CameraSystem using static reference
    public void UpdateCamTarget(float3 position)
    {
        targetPosition = position;
        targetPosition.y = 0f; // height of camera origin is locked to offset y-value
    }
    
}
