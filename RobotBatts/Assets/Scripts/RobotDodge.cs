using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;

public class RobotDodgeWithCooldown : MonoBehaviour
{
    [Header("Robot Reference")]
    [SerializeField] private Transform robotTransform;

    [Header("Input Actions")]
    [SerializeField] private InputActionReference leftDodgeAction;
    [SerializeField] private InputActionReference rightDodgeAction;

    [Header("Dodge Positions")]
    [SerializeField] private Vector3 basePosition = new Vector3(1.97040009f, 3.00159979f, -18.0083008f);
    [SerializeField] private Vector3 leftDodgePosition = new Vector3(-1.54000001f, 3.00159979f, -18.0083008f);
    [SerializeField] private Vector3 rightDodgePosition = new Vector3(4.82999992f, 3.00159979f, -18.0083008f);

    [Header("Dodge Settings")]
    [SerializeField] private float dodgeDuration = 0.3f; // Время анимации уворота
    [SerializeField] private float cooldown = 5f; // Кулдаун 5 секунд

    private Vector3 currentTargetPosition;
    private float dodgeTimer = 0f;
    private float cooldownTimer = 0f;
    private bool isDodging = false;
    private bool isOnCooldown = false;

    private void Start()
    {
        // Устанавливаем начальную позицию
        if (robotTransform != null)
        {
            robotTransform.localPosition = basePosition;
            currentTargetPosition = basePosition;
        }
    }

    private void OnEnable()
    {
        // Активируем input actions
        if (leftDodgeAction != null)
        {
            leftDodgeAction.action.Enable();
            leftDodgeAction.action.performed += OnLeftDodgePerformed;
        }

        if (rightDodgeAction != null)
        {
            rightDodgeAction.action.Enable();
            rightDodgeAction.action.performed += OnRightDodgePerformed;
        }
    }

    private void OnDisable()
    {
        // Деактивируем input actions
        if (leftDodgeAction != null)
        {
            leftDodgeAction.action.performed -= OnLeftDodgePerformed;
            leftDodgeAction.action.Disable();
        }

        if (rightDodgeAction != null)
        {
            rightDodgeAction.action.performed -= OnRightDodgePerformed;
            rightDodgeAction.action.Disable();
        }
    }

    private void Update()
    {
        // Обновляем таймер кулдауна
        if (isOnCooldown)
        {
            cooldownTimer -= Time.deltaTime;
            if (cooldownTimer <= 0f)
            {
                isOnCooldown = false;
                cooldownTimer = 0f;
            }
        }

        // Обрабатываем анимацию уворота
        if (isDodging)
        {
            dodgeTimer += Time.deltaTime;
            float progress = Mathf.Clamp01(dodgeTimer / dodgeDuration);

            if (robotTransform != null)
            {
                robotTransform.localPosition = Vector3.Lerp(
                    robotTransform.localPosition,
                    currentTargetPosition,
                    progress
                );
            }

            if (dodgeTimer >= dodgeDuration)
            {
                if (robotTransform != null)
                {
                    robotTransform.localPosition = currentTargetPosition;
                }
                isDodging = false;
                dodgeTimer = 0f;
            }
        }
    }

    private void OnLeftDodgePerformed(InputAction.CallbackContext context)
    {
        if (!isOnCooldown)
        {
            DodgeLeft();
        }
    }

    private void OnRightDodgePerformed(InputAction.CallbackContext context)
    {
        if (!isOnCooldown)
        {
            DodgeRight();
        }
    }

    private void DodgeLeft()
    {
        if (robotTransform == null) return;

        currentTargetPosition = leftDodgePosition;
        isDodging = true;
        dodgeTimer = 0f;

        // Стартуем кулдаун
        StartCooldown();
    }

    private void DodgeRight()
    {
        if (robotTransform == null) return;

        currentTargetPosition = rightDodgePosition;
        isDodging = true;
        dodgeTimer = 0f;

        // Стартуем кулдаун
        StartCooldown();
    }

    private void StartCooldown()
    {
        isOnCooldown = true;
        cooldownTimer = cooldown;
    }

    [ContextMenu("Dodge Left")]
    public void TestDodgeLeft()
    {
        if (!isOnCooldown)
        {
            DodgeLeft();
        }
    }

    [ContextMenu("Dodge Right")]
    public void TestDodgeRight()
    {
        if (!isOnCooldown)
        {
            DodgeRight();
        }
    }

    [ContextMenu("Reset to Base")]
    public void ResetToBase()
    {
        robotTransform.localPosition = basePosition;
        currentTargetPosition = basePosition;
        isDodging = false;
        dodgeTimer = 0f;
        isOnCooldown = false;
        cooldownTimer = 0f;
    }
}