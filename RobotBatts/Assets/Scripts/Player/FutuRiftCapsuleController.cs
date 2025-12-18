using Futurift;
using Futurift.DataSenders;
using Futurift.Options;
using System.Collections;
using UnityEngine;

public class FutuRiftCapsuleController : MonoBehaviour
{
    [Header("FutuRift Connection")]
    [SerializeField] private string ipAddress = "127.0.0.1";
    [SerializeField] private int port = 6065;

    [Header("Basic Tilt Settings")]
    [Tooltip("Max tilt angle in degrees")]
    public float maxTiltAngle = 25f;
    [Tooltip("How quickly the capsule returns to neutral position")]
    public float tiltRecoverySpeed = 3f;

    [Header("Action Tilt Settings")]
    public float nockbackAngle = 5f;
    public float dodgeInitialAngle = 3f;
    public float enemyHitAngle = 4f;
    public float walkForwardAngle = 2f;
    public float walkSideAngle = 1f;
    public float walkCycleDuration = 0.8f;

    // Current tilt values
    private float _currentPitch = 0f;
    private float _currentRoll = 0f;

    private FutuRiftController _futuRiftController;
    private Coroutine currentActionCoroutine;
    private bool isWalkingAnimation = false;
    private float walkTimer = 0f;

    void Awake()
    {
        InitializeFutuRift();
    }

    void Update()
    {
        if (currentActionCoroutine == null && !isWalkingAnimation)
        {
            _currentPitch = Mathf.Lerp(_currentPitch, 0f, tiltRecoverySpeed * Time.deltaTime);
            _currentRoll = Mathf.Lerp(_currentRoll, 0f, tiltRecoverySpeed * Time.deltaTime);
        }
        if (isWalkingAnimation)
        {
            UpdateWalkingTilt();
        }
        ApplyTilts();
    }

    private void InitializeFutuRift()
    {
        var udpOptions = new UdpOptions
        {
            ip = ipAddress,
            port = port
        };
        _futuRiftController = new FutuRiftController(new UdpPortSender(udpOptions));
    }

    private void ApplyTilts()
    {
        if (_futuRiftController != null)
        {
            _futuRiftController.Pitch = _currentPitch;
            _futuRiftController.Roll = _currentRoll;
        }
    }

    public void SetManualTilt(float pitch, float roll)
    {
        _currentPitch = Mathf.Clamp(pitch, -maxTiltAngle, maxTiltAngle);
        _currentRoll = Mathf.Clamp(roll, -maxTiltAngle, maxTiltAngle);
    }

    public void ResetTilt()
    {
        _currentPitch = 0f;
        _currentRoll = 0f;
    }

    public void TriggerNockbackTilt()
    {
        if (currentActionCoroutine != null)
            StopCoroutine(currentActionCoroutine);

        currentActionCoroutine = StartCoroutine(NockbackTiltCoroutine());
    }

    private IEnumerator NockbackTiltCoroutine()
    {
        SetManualTilt(-nockbackAngle, 0f);
        yield return new WaitForSeconds(0.15f);
        SetManualTilt(nockbackAngle, 0f);
        yield return new WaitForSeconds(0.1f);
        ResetTilt();
        currentActionCoroutine = null;
    }

    public void TriggerDodgeRightTilt()
    {
        if (currentActionCoroutine != null)
            StopCoroutine(currentActionCoroutine);

        currentActionCoroutine = StartCoroutine(DodgeRightTiltCoroutine());
    }

    private IEnumerator DodgeRightTiltCoroutine()
    {
        SetManualTilt(0f, dodgeInitialAngle);
        yield return new WaitForSeconds(0.1f);
        SetManualTilt(0f, -maxTiltAngle);
        yield return new WaitForSeconds(0.2f);
        ResetTilt();
        currentActionCoroutine = null;
    }

    public void TriggerDodgeLeftTilt()
    {
        if (currentActionCoroutine != null)
            StopCoroutine(currentActionCoroutine);

        currentActionCoroutine = StartCoroutine(DodgeLeftTiltCoroutine());
    }

    private IEnumerator DodgeLeftTiltCoroutine()
    {
        SetManualTilt(0f, -dodgeInitialAngle);
        yield return new WaitForSeconds(0.1f);
        SetManualTilt(0f, maxTiltAngle);
        yield return new WaitForSeconds(0.2f);

        ResetTilt();
        currentActionCoroutine = null;
    }

    public void TriggerUltimateRiseTilt()
    {
        if (currentActionCoroutine != null)
            StopCoroutine(currentActionCoroutine);
        SetManualTilt(-maxTiltAngle, 0f);
    }

    public void TriggerUltimateStabilizeTilt()
    {
        ResetTilt();
        if (currentActionCoroutine != null)
        {
            StopCoroutine(currentActionCoroutine);
            currentActionCoroutine = null;
        }
    }
    public void TriggerFallingTilt()
    {
        if (currentActionCoroutine != null)
            StopCoroutine(currentActionCoroutine);

        currentActionCoroutine = StartCoroutine(FallingTiltCoroutine());
    }

    private IEnumerator FallingTiltCoroutine()
    {
        SetManualTilt(maxTiltAngle, 0f);
        yield return new WaitForSeconds(0.3f);
        SetManualTilt(-maxTiltAngle / 2, 0f);
        yield return new WaitForSeconds(0.2f);

        ResetTilt();
        currentActionCoroutine = null;
    }

    public void TriggerEnemyHitTilt()
    {
        if (currentActionCoroutine != null)
            StopCoroutine(currentActionCoroutine);

        currentActionCoroutine = StartCoroutine(EnemyHitTiltCoroutine());
    }

    private IEnumerator EnemyHitTiltCoroutine()
    {
        SetManualTilt(-enemyHitAngle, 0f);
        yield return new WaitForSeconds(0.25f);

        ResetTilt();
        currentActionCoroutine = null;
    }

    public void StartWalkingTilt()
    {
        isWalkingAnimation = true;
        walkTimer = 0f;

        if (currentActionCoroutine != null)
        {
            StopCoroutine(currentActionCoroutine);
            currentActionCoroutine = null;
        }
    }

    public void StopWalkingTilt()
    {
        isWalkingAnimation = false;
        ResetTilt();
    }

    private void UpdateWalkingTilt()
    {
        walkTimer += Time.deltaTime;
        float cycleProgress = (walkTimer % walkCycleDuration) / walkCycleDuration;
        if (cycleProgress < 0.25f)
        {
            float phaseProgress = cycleProgress / 0.25f;
            float pitch = Mathf.Lerp(0, walkForwardAngle, phaseProgress);
            float roll = Mathf.Lerp(0, walkSideAngle, phaseProgress);
            SetManualTilt(pitch, roll);
        }
        else if (cycleProgress < 0.5f)
        {
            float phaseProgress = (cycleProgress - 0.25f) / 0.25f;
            float pitch = Mathf.Lerp(walkForwardAngle, 0, phaseProgress);
            float roll = Mathf.Lerp(walkSideAngle, 0, phaseProgress);
            SetManualTilt(pitch, roll);
        }
        else if (cycleProgress < 0.75f)
        {
            float phaseProgress = (cycleProgress - 0.5f) / 0.25f;
            float pitch = Mathf.Lerp(0, walkForwardAngle * 0.8f, phaseProgress);
            float roll = Mathf.Lerp(0, -walkSideAngle * 0.7f, phaseProgress);
            SetManualTilt(pitch, roll);
        }
        else
        {
            float phaseProgress = (cycleProgress - 0.75f) / 0.25f;
            float pitch = Mathf.Lerp(walkForwardAngle * 0.8f, 0, phaseProgress);
            float roll = Mathf.Lerp(-walkSideAngle * 0.7f, 0, phaseProgress);
            SetManualTilt(pitch, roll);
        }
    }

    public void StopAllTilts()
    {
        if (currentActionCoroutine != null)
        {
            StopCoroutine(currentActionCoroutine);
            currentActionCoroutine = null;
        }

        isWalkingAnimation = false;
        ResetTilt();
    }

    public Vector2 GetCurrentTilt()
    {
        return new Vector2(_currentPitch, _currentRoll);
    }

    void OnEnable()
    {
        _futuRiftController?.Start();
    }

    void OnDisable()
    {
        _futuRiftController?.Stop();
        StopAllTilts();
    }
    [System.Serializable]
    public struct TiltData
    {
        public float currentPitch;
        public float currentRoll;
    }

    public TiltData GetTiltData()
    {
        return new TiltData
        {
            currentPitch = _currentPitch,
            currentRoll = _currentRoll
        };
    }
}