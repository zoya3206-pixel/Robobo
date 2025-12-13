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
        // Обработка автоматического возврата к нейтрали, если нет активных действий
        if (currentActionCoroutine == null && !isWalkingAnimation)
        {
            _currentPitch = Mathf.Lerp(_currentPitch, 0f, tiltRecoverySpeed * Time.deltaTime);
            _currentRoll = Mathf.Lerp(_currentRoll, 0f, tiltRecoverySpeed * Time.deltaTime);
        }

        // Обработка анимации ходьбы
        if (isWalkingAnimation)
        {
            UpdateWalkingTilt();
        }

        // Применение наклонов к капсуле
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

    // ========== ОСНОВНЫЕ МЕТОДЫ УПРАВЛЕНИЯ ==========

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

    // ========== МЕТОДЫ ДЛЯ ИГРОВЫХ СИТУАЦИЙ ==========

    // 1. Удар с отдачей (nockback)
    public void TriggerNockbackTilt()
    {
        if (currentActionCoroutine != null)
            StopCoroutine(currentActionCoroutine);

        currentActionCoroutine = StartCoroutine(NockbackTiltCoroutine());
    }

    private IEnumerator NockbackTiltCoroutine()
    {
        // Резкий наклон назад
        SetManualTilt(-nockbackAngle, 0f);
        yield return new WaitForSeconds(0.15f);

        // Быстрый наклон вперед
        SetManualTilt(nockbackAngle, 0f);
        yield return new WaitForSeconds(0.1f);

        // Возврат в нормальное состояние
        ResetTilt();
        currentActionCoroutine = null;
    }

    // 2. Уклонение вправо
    public void TriggerDodgeRightTilt()
    {
        if (currentActionCoroutine != null)
            StopCoroutine(currentActionCoroutine);

        currentActionCoroutine = StartCoroutine(DodgeRightTiltCoroutine());
    }

    private IEnumerator DodgeRightTiltCoroutine()
    {
        // Слегка вправо
        SetManualTilt(0f, dodgeInitialAngle);
        yield return new WaitForSeconds(0.1f);

        // Резко влево на максимальный угол
        SetManualTilt(0f, -maxTiltAngle);
        yield return new WaitForSeconds(0.2f);

        ResetTilt();
        currentActionCoroutine = null;
    }

    // 3. Уклонение влево
    public void TriggerDodgeLeftTilt()
    {
        if (currentActionCoroutine != null)
            StopCoroutine(currentActionCoroutine);

        currentActionCoroutine = StartCoroutine(DodgeLeftTiltCoroutine());
    }

    private IEnumerator DodgeLeftTiltCoroutine()
    {
        // Слегка влево
        SetManualTilt(0f, -dodgeInitialAngle);
        yield return new WaitForSeconds(0.1f);

        // Резко вправо на максимальный угол
        SetManualTilt(0f, maxTiltAngle);
        yield return new WaitForSeconds(0.2f);

        ResetTilt();
        currentActionCoroutine = null;
    }

    // 4. Ultimate - подъем вверх
    public void TriggerUltimateRiseTilt()
    {
        if (currentActionCoroutine != null)
            StopCoroutine(currentActionCoroutine);

        // Наклон максимально назад
        SetManualTilt(-maxTiltAngle, 0f);
    }

    // 5. Стабилизация на высоте
    public void TriggerUltimateStabilizeTilt()
    {
        ResetTilt();
        if (currentActionCoroutine != null)
        {
            StopCoroutine(currentActionCoroutine);
            currentActionCoroutine = null;
        }
    }

    // 6. Падение
    public void TriggerFallingTilt()
    {
        if (currentActionCoroutine != null)
            StopCoroutine(currentActionCoroutine);

        currentActionCoroutine = StartCoroutine(FallingTiltCoroutine());
    }

    private IEnumerator FallingTiltCoroutine()
    {
        // Резкий наклон вперед на максимальный угол
        SetManualTilt(maxTiltAngle, 0f);
        yield return new WaitForSeconds(0.3f);

        // Резкий возврат назад
        SetManualTilt(-maxTiltAngle / 2, 0f);
        yield return new WaitForSeconds(0.2f);

        ResetTilt();
        currentActionCoroutine = null;
    }

    // 7. Попадание врага по игроку
    public void TriggerEnemyHitTilt()
    {
        if (currentActionCoroutine != null)
            StopCoroutine(currentActionCoroutine);

        currentActionCoroutine = StartCoroutine(EnemyHitTiltCoroutine());
    }

    private IEnumerator EnemyHitTiltCoroutine()
    {
        // Наклон немного назад
        SetManualTilt(-enemyHitAngle, 0f);
        yield return new WaitForSeconds(0.25f);

        ResetTilt();
        currentActionCoroutine = null;
    }

    // 8. Ходьба робота
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

        // Циклическая анимация ходьбы робота
        if (cycleProgress < 0.25f) // Фаза 1: Шаг правой ногой
        {
            float phaseProgress = cycleProgress / 0.25f;
            float pitch = Mathf.Lerp(0, walkForwardAngle, phaseProgress);
            float roll = Mathf.Lerp(0, walkSideAngle, phaseProgress);
            SetManualTilt(pitch, roll);
        }
        else if (cycleProgress < 0.5f) // Фаза 2: Стабилизация
        {
            float phaseProgress = (cycleProgress - 0.25f) / 0.25f;
            float pitch = Mathf.Lerp(walkForwardAngle, 0, phaseProgress);
            float roll = Mathf.Lerp(walkSideAngle, 0, phaseProgress);
            SetManualTilt(pitch, roll);
        }
        else if (cycleProgress < 0.75f) // Фаза 3: Шаг левой ногой
        {
            float phaseProgress = (cycleProgress - 0.5f) / 0.25f;
            float pitch = Mathf.Lerp(0, walkForwardAngle * 0.8f, phaseProgress);
            float roll = Mathf.Lerp(0, -walkSideAngle * 0.7f, phaseProgress);
            SetManualTilt(pitch, roll);
        }
        else // Фаза 4: Стабилизация
        {
            float phaseProgress = (cycleProgress - 0.75f) / 0.25f;
            float pitch = Mathf.Lerp(walkForwardAngle * 0.8f, 0, phaseProgress);
            float roll = Mathf.Lerp(-walkSideAngle * 0.7f, 0, phaseProgress);
            SetManualTilt(pitch, roll);
        }
    }

    // ========== ВСПОМОГАТЕЛЬНЫЕ МЕТОДЫ ==========

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

    // Простая структура для отладки
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