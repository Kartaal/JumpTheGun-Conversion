using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class MonoCameraController : MonoBehaviour
{
    
    public Vector3 offset;
    Vector3 targetPosition;
    public Camera mainCamera;

    public static MonoCameraController Instance;
    
    private void Awake()
    {
        if (Instance != null)
        {
            DestroyImmediate(gameObject);
        }
        else
        {
            Instance = this;
        }
    }
    
    void Start()
    {
        mainCamera = Camera.main;
        mainCamera.gameObject.transform.position = offset;
    }
    
    void Update()
    {
        
    }
    
    private void LateUpdate()
    {
        mainCamera.gameObject.transform.position = targetPosition + offset;
    }

    public void UpdateTargetPosition(float3 position)
    {
        targetPosition = position;
        targetPosition.y = 0f;
    }
    
}
