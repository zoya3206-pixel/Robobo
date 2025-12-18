using Bhaptics.SDK2;
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
    [SerializeField] private float riseHeight = 4.3584f;

    [Header("Capsule Controller")]
    [SerializeField] private FutuRiftCapsuleController capsuleController;

    private bool isUltimateActive = false;
    private bool isRising = false;
    private bool isFalling = false;
    private bool isWaiting = false;

    private float animationTimer = 0f;
    private float waitTimer = 0f;

    private Vector3 startPosition;
    private Vector3 targetPosition;

    private void OnEnable()
    {
        if (activateUltimateAction.action != null)
        {
            activateUltimateAction.action.Enable();
            activateUltimateAction.action.performed += OnUltimatePerformed;
        }
    }

    private void OnDisable()
    {
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

        startPosition = robotTransform.position; 
        targetPosition = startPosition + Vector3.up * riseHeight;

        animationTimer = 0f;
        waitTimer = 0f;
    }

    private void RiseAnimation()
    {
        animationTimer += Time.deltaTime;
        float progress = Mathf.Clamp01(animationTimer / riseTime);

        robotTransform.position = Vector3.Lerp(startPosition, targetPosition, progress);

        if (animationTimer >= riseTime)
        {
            robotTransform.position = targetPosition;
            BhapticsLibrary.Play("ultimate");
            capsuleController?.TriggerUltimateRiseTilt();

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
            capsuleController?.TriggerUltimateStabilizeTilt();
        }
    }

    private void FallAnimation()
    {
        animationTimer += Time.deltaTime;
        float progress = Mathf.Clamp01(animationTimer / fallTime);

        robotTransform.position = Vector3.Lerp(targetPosition, startPosition, progress);

        if (animationTimer >= fallTime)
        {
            robotTransform.position = startPosition;
            isFalling = false;
            isUltimateActive = false;
            animationTimer = 0f;
            capsuleController?.TriggerFallingTilt();
            Invoke(nameof(StopAllTilts), 2f);
        }
    }

    private void StopAllTilts()
    {
        capsuleController?.StopAllTilts();
    }
}