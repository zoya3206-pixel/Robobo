using Bhaptics.SDK2;
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

    [Header("Dodge Settings")]
    [SerializeField] private float dodgeDuration = 0.3f;
    [SerializeField] private float cooldown = 5f; 

    [Header("Dodge Offsets")]
    [SerializeField] private Vector3 leftDodgeOffset = new Vector3(-3f, 0f, 0f); 
    [SerializeField] private Vector3 rightDodgeOffset = new Vector3(3f, 0f, 0f);

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
            Invoke("capsuleController?.StopAllTilts()", 2f);
        }
    }

    private void OnRightDodgePerformed(InputAction.CallbackContext context)
    {
        if (rightCooldownTimer <= 0f)
        {
            DodgeRight();
            Invoke("capsuleController?.StopAllTilts()", 2f);
        }
    }

    private void DodgeLeft()
    {
        if (robotTransform == null) return;

        accumulatedOffset += leftDodgeOffset;

        currentTargetPosition = robotTransform.localPosition + leftDodgeOffset;

        BhapticsLibrary.Play("dodgeleft");
        capsuleController?.TriggerDodgeLeftTilt();

        isDodging = true;
        dodgeTimer = 0f;

        leftCooldownTimer = cooldown;
    }

    private void DodgeRight()
    {
        if (robotTransform == null) return;

        accumulatedOffset += rightDodgeOffset;

        currentTargetPosition = robotTransform.localPosition + rightDodgeOffset;

        BhapticsLibrary.Play("dodgeright");
        capsuleController?.TriggerDodgeRightTilt();

        isDodging = true;
        dodgeTimer = 0f;

        rightCooldownTimer = cooldown;
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