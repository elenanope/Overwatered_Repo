using System;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class ThirdPersonCamController : MonoBehaviour
{
    [SerializeField] float zoomSpeed = 2f;
    [SerializeField] float zoomLerpSpeed = 10f;
    [SerializeField] float minDistance = 5f;
    [SerializeField] float maxDistance = 15f;

    PlayerInput inputActions;
    CinemachineCamera cam;
    CinemachineOrbitalFollow orbital;
    Vector2 scrollDelta;

    float targetZoom;
    float currentZoom;

    private void Start()
    {
        cam = GetComponent<CinemachineCamera>();
        orbital = cam.GetComponent<CinemachineOrbitalFollow>();
        targetZoom = currentZoom = orbital.Radius;
        inputActions = new PlayerInput();
        inputActions.Enable();
        inputActions.CameraControls.CameraMouseZoom.performed += HandleMouseScroll;

        Cursor.lockState = CursorLockMode.Locked;
    }

    private void HandleMouseScroll(InputAction.CallbackContext context)
    {
        scrollDelta = context.ReadValue<Vector2>();
    }

    private void Update()
    {
        if(scrollDelta.y != 0)
        {
            if(orbital!= null)
            {
                targetZoom = Mathf.Clamp(orbital.Radius - scrollDelta.y * zoomSpeed, minDistance, maxDistance);
                scrollDelta = Vector2.zero;
            }
        }

        float bumperDelta = inputActions.CameraControls.CamControllerZoom.ReadValue<float>();
        if (bumperDelta != 0)
        {
            targetZoom = Mathf.Clamp(orbital.Radius - bumperDelta * zoomSpeed, minDistance, maxDistance);
        }

        currentZoom = Mathf.Lerp(currentZoom, targetZoom, Time.deltaTime * zoomLerpSpeed);
        orbital.Radius = currentZoom;
    }
}
