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
    [SerializeField] private float riseTime = 1.8f;        
    [SerializeField] private float fallTime = 0.6f;       
    [SerializeField] private float waitTime = 0.4f;        
    [SerializeField] private float riseHeight = 4.3584f;

    [Header("Урон ультимейта")]
    [SerializeField] private float ultimateDamage = 2500f;

    [Header("Capsule Controller")]
    [SerializeField] private FutuRiftCapsuleController capsuleController;

    [Header("Свет в кабине")]
    [SerializeField] private Light cabinLight;
    private float originalLightIntensity;

    [SerializeField] private EnemyHealth enemyHealth;

    public bool isUltimateActive = false;
    private bool isRising = false;
    private bool isFalling = false;
    private bool isWaiting = false;

    private float animationTimer = 0f;
    private float waitTimer = 0f;

    private Vector3 startPosition;
    private Vector3 targetPosition;

    private void Start()
    {
        if (enemyHealth == null)
        {
            GameObject enemy = GameObject.FindGameObjectWithTag("Enemy");
            if (enemy != null)
            {
                enemyHealth = enemy.GetComponent<EnemyHealth>();
            }
        }

        if (cabinLight != null)
        {
            originalLightIntensity = cabinLight.intensity;
        }
    }

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
            if (enemyHealth != null && enemyHealth.IsStunned())
            {
                StartUltimate();
            }
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

        if (cabinLight != null)
        {
            cabinLight.intensity = originalLightIntensity * 2f;
        }
    }

    private void RiseAnimation()
    {
        animationTimer += Time.deltaTime;

        float progress = Mathf.Clamp01(animationTimer / riseTime);

        float easeOutProgress = 1f - Mathf.Pow(1f - progress, 2);

        robotTransform.position = Vector3.Lerp(startPosition, targetPosition, easeOutProgress);

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
        float easeInProgress = Mathf.Pow(progress, 3);
        robotTransform.position = Vector3.Lerp(targetPosition, startPosition, easeInProgress);

        if (animationTimer >= fallTime)
        {
            robotTransform.position = startPosition;

            if (enemyHealth != null && !enemyHealth.IsDead())
            {
                enemyHealth.TakeDamage(ultimateDamage);
            }


            if (cabinLight != null)
            {
                cabinLight.intensity = originalLightIntensity;
            }

            isFalling = false;
            isUltimateActive = false;
            animationTimer = 0f;
            capsuleController?.TriggerFallingTilt();
            Invoke(nameof(StopAllTilts), 1f);
        }
    }

    private void StopAllTilts()
    {
        capsuleController?.StopAllTilts();
    }
}