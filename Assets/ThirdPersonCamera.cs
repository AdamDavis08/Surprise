using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Vector3 = System.Numerics.Vector3;

public class ThirdPersonCamera : MonoBehaviour
{
    public float mouseSensitivity = 100f;
    public Transform cameraLookAt;

    private float xRotation = 0f;
    
    private void Update()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;
        
        cameraLookAt.Rotate(0f, mouseX, 0f);
        transform.rotation = cameraLookAt.rotation;
    }
}
