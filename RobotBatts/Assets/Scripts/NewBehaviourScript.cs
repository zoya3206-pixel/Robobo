using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;

public class SimpleVRNavigation : MonoBehaviour
{
    public float moveSpeed = 1.5f;
    private InputAction moveAction;
    private XROrigin xrOrigin;

    void Start()
    {
        xrOrigin = GetComponent<XROrigin>();

        // Создаем действие для движения
        moveAction = new InputAction("Move", InputActionType.Value, "<XRController>{LeftHand}/primary2DAxis");
        moveAction.Enable();
    }

    void Update()
    {
        if (xrOrigin == null) return;

        // Получаем ввод от левого джойстика
        Vector2 moveInput = moveAction.ReadValue<Vector2>();

        if (moveInput.magnitude > 0.1f)
        {
            // Преобразуем ввод в движение относительно камеры
            Vector3 direction = new Vector3(moveInput.x, 0, moveInput.y);
            direction = xrOrigin.Camera.transform.TransformDirection(direction);
            direction.y = 0;

            // Двигаем XR Origin
            xrOrigin.transform.Translate(direction * moveSpeed * Time.deltaTime, Space.World);
        }
    }

    void OnDestroy()
    {
        moveAction?.Disable();
    }
}