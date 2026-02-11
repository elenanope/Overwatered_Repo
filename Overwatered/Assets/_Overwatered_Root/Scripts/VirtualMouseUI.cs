using UnityEngine;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.UI;

public class VirtualMouseUI : MonoBehaviour
{
    [SerializeField] private RectTransform canvasRectTransform;
    VirtualMouseInput virtualMouseInput;
    Vector2 virtualMousePos;

    private void Awake()
    {
        virtualMouseInput = GetComponent<VirtualMouseInput>();
    }
    private void Update()
    {
        transform.localScale = Vector3.one * (1f/canvasRectTransform.localScale.x);
    }

    private void LateUpdate()
    {
        virtualMousePos = virtualMouseInput.virtualMouse.position.value;
        virtualMousePos.x = Mathf.Clamp(virtualMousePos.x, 0f, Screen.width);
        virtualMousePos.y = Mathf.Clamp(virtualMousePos.y, 0f, Screen.height);
        InputState.Change(virtualMouseInput.virtualMouse.position, virtualMousePos);
    }
}
