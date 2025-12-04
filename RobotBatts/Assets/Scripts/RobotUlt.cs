using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;

public class RobotUltimateController : MonoBehaviour
{
    [Header("Robot Reference")]
    [SerializeField] private Transform robotTransform;

    [Header("XR Input Actions")]
    [SerializeField] private InputActionProperty activateUltimateAction;

    [Header("Animation Settings")]
    [SerializeField] private float riseTime = 2f;
    [SerializeField] private float fallTime = 2f;
    [SerializeField] private float waitTime = 1f;

    [Header("Position Settings")]
    [SerializeField] private Vector3 idlePosition = new Vector3(1.97040009f, 3.00159979f, -18.0083008f);
    [SerializeField] private Vector3 ultimatePosition = new Vector3(1.97040009f, 7.36000013f, -18.0083008f);

    private bool isUltimateActive = false;
    private bool isRising = false;
    private bool isFalling = false;
    private bool isWaiting = false;

    private float animationTimer = 0f;
    private float waitTimer = 0f;

    private void Awake()
    {
        // Если позиции не заданы, используем текущие
        if (robotTransform != null)
        {
            if (idlePosition == Vector3.zero)
                idlePosition = robotTransform.localPosition;

            if (ultimatePosition == Vector3.zero)
                ultimatePosition = idlePosition + Vector3.up * 4.3584f;
        }
    }

    private void OnEnable()
    {
        // Активируем input action
        if (activateUltimateAction.action != null)
        {
            activateUltimateAction.action.Enable();
            activateUltimateAction.action.performed += OnUltimatePerformed;
        }
    }

    private void OnDisable()
    {
        // Деактивируем input action
        if (activateUltimateAction.action != null)
        {
            activateUltimateAction.action.performed -= OnUltimatePerformed;
            activateUltimateAction.action.Disable();
        }
    }

    private void Update()
    {
        if (!isUltimateActive) return;

        if (isRising)
        {
            RiseAnimation();
        }
        else if (isWaiting)
        {
            WaitAnimation();
        }
        else if (isFalling)
        {
            FallAnimation();
        }
    }

    private void OnUltimatePerformed(InputAction.CallbackContext context)
    {
        if (!isUltimateActive)
        {
            StartUltimate();
        }
    }

    private void StartUltimate()
    {
        isUltimateActive = true;
        isRising = true;
        isFalling = false;
        isWaiting = false;
        animationTimer = 0f;
        waitTimer = 0f;
    }

    private void RiseAnimation()
    {
        animationTimer += Time.deltaTime;
        float progress = Mathf.Clamp01(animationTimer / riseTime);

        if (robotTransform != null)
        {
            robotTransform.localPosition = Vector3.Lerp(idlePosition, ultimatePosition, progress);
        }

        if (animationTimer >= riseTime)
        {
            if (robotTransform != null)
            {
                robotTransform.localPosition = ultimatePosition;
            }
            isRising = false;
            isWaiting = true;
            animationTimer = 0f;
        }
    }

    private void WaitAnimation()
    {
        waitTimer += Time.deltaTime;

        if (waitTimer >= waitTime)
        {
            isWaiting = false;
            isFalling = true;
            waitTimer = 0f;
        }
    }

    private void FallAnimation()
    {
        animationTimer += Time.deltaTime;
        float progress = Mathf.Clamp01(animationTimer / fallTime);

        if (robotTransform != null)
        {
            robotTransform.localPosition = Vector3.Lerp(ultimatePosition, idlePosition, progress);
        }

        if (animationTimer >= fallTime)
        {
            if (robotTransform != null)
            {
                robotTransform.localPosition = idlePosition;
            }
            isFalling = false;
            isUltimateActive = false;
            animationTimer = 0f;
        }
    }

    // Метод для ручного тестирования
    [ContextMenu("Test Ultimate")]
    public void TestUltimate()
    {
        if (!isUltimateActive)
        {
            StartUltimate();
        }
    }
}