using Bhaptics.SDK2;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;

public class RobotDodge : MonoBehaviour
{
    [Header("Robot Reference")]
    [SerializeField] private Transform robotTransform;

    [Header("Camera Reference (VR игрока)")]
    [SerializeField] private Transform vrCamera;

    [Header("Input Actions")]
    [SerializeField] private InputActionReference leftDodgeAction;
    [SerializeField] private InputActionReference rightDodgeAction;

    [Header("Dodge Settings")]
    [SerializeField] private float dodgeDuration = 1f;
    [SerializeField] private float cooldown = 5f;
    [SerializeField] private float dodgeDistance = 8f;

    [Header("Capsule Controller")]
    [SerializeField] private FutuRiftCapsuleController capsuleController;

    private float leftCooldownTimer = 0f;
    private float rightCooldownTimer = 0f;

    private Vector3 currentTargetPosition;
    private float dodgeTimer = 0f;
    private bool isDodging = false;

    private Vector3 accumulatedOffset = Vector3.zero;

    private void Start()
    {
        if (robotTransform != null)
        {
            currentTargetPosition = robotTransform.localPosition;
        }
    }

    private void OnEnable()
    {
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
        if (leftCooldownTimer > 0f)
        {
            leftCooldownTimer -= Time.deltaTime;
            if (leftCooldownTimer < 0f) leftCooldownTimer = 0f;
        }

        if (rightCooldownTimer > 0f)
        {
            rightCooldownTimer -= Time.deltaTime;
            if (rightCooldownTimer < 0f) rightCooldownTimer = 0f;
        }

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
        if (leftCooldownTimer <= 0f)
        {
            DodgeLeft();
            Invoke(nameof(StopAllTilts), 2f);
        }
    }

    private void OnRightDodgePerformed(InputAction.CallbackContext context)
    {
        if (rightCooldownTimer <= 0f)
        {
            DodgeRight();
            Invoke(nameof(StopAllTilts), 2f);
        }
    }

    private void DodgeLeft()
    {
        if (robotTransform == null || vrCamera == null) return;

        Vector3 leftDirection = -vrCamera.right;
        leftDirection.y = 0;
        leftDirection.Normalize();

        Vector3 dodgeOffset = leftDirection * dodgeDistance;

        accumulatedOffset += dodgeOffset;
        currentTargetPosition = robotTransform.localPosition + dodgeOffset;

        BhapticsLibrary.Play("dodgeleft");
        capsuleController?.TriggerDodgeLeftTilt();

        isDodging = true;
        dodgeTimer = 0f;
        leftCooldownTimer = cooldown;
    }

    private void DodgeRight()
    {
        if (robotTransform == null || vrCamera == null) return;

        Vector3 rightDirection = vrCamera.right;
        rightDirection.y = 0;
        rightDirection.Normalize();

        Vector3 dodgeOffset = rightDirection * dodgeDistance;

        accumulatedOffset += dodgeOffset;
        currentTargetPosition = robotTransform.localPosition + dodgeOffset;

        BhapticsLibrary.Play("dodgeright");
        capsuleController?.TriggerDodgeRightTilt();

        isDodging = true;
        dodgeTimer = 0f;
        rightCooldownTimer = cooldown;
    }

    private void StopAllTilts()
    {
        capsuleController?.StopAllTilts();
    }

    public Vector3 GetCurrentBasePosition()
    {
        return GetInitialPosition() + accumulatedOffset;
    }

    private Vector3 GetInitialPosition()
    {
        return new Vector3(1.97040009f, 3.00159979f, -18.0083008f);
    }
}