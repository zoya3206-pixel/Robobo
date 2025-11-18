using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class HeadBasedRotation : MonoBehaviour
{
    [Header("Настройки физического поворота")]
    public float rotationSensitivity = 0.3f; // Чувствительность поворота
    public float rotationThreshold = 15f;     // Порог активации поворота (градусы)
    public float maxHeadTurnAngle = 60f;      // Максимальный угол поворота головы
    public float smoothDampTime = 0.1f;       // Время плавности поворота

    private XROrigin xrOrigin;
    private Camera xrCamera;
    private float initialHeadYaw;
    private float targetRobotRotation;
    private float currentRobotRotation;
    private float rotationVelocity;

    void Start()
    {
        xrOrigin = GetComponent<XROrigin>();
        xrCamera = xrOrigin.Camera;

        if (xrCamera == null)
        {
            Debug.LogError("XR Camera не найден!");
            return;
        }

        // Запоминаем начальное положение головы
        initialHeadYaw = NormalizeAngle(xrCamera.transform.eulerAngles.y);
    }

    void Update()
    {
        if (xrCamera == null) return;

        float currentHeadYaw = NormalizeAngle(xrCamera.transform.eulerAngles.y);
        float headRotationDelta = Mathf.DeltaAngle(initialHeadYaw, currentHeadYaw);

        // Ограничиваем поворот головы для комфорта
        headRotationDelta = Mathf.Clamp(headRotationDelta, -maxHeadTurnAngle, maxHeadTurnAngle);

        // Если поворот головы превышает порог - начинаем поворачивать робота
        if (Mathf.Abs(headRotationDelta) > rotationThreshold)
        {
            // Вычисляем целевой поворот робота на основе поворота головы
            targetRobotRotation = headRotationDelta * rotationSensitivity;
        }
        else
        {
            // Плавно возвращаем к нулю, если голова в нейтральной позиции
            targetRobotRotation = 0f;
        }

        // Плавно применяем поворот
        currentRobotRotation = Mathf.SmoothDamp(currentRobotRotation, targetRobotRotation,
                                              ref rotationVelocity, smoothDampTime);

        // Поворачиваем робота
        xrOrigin.transform.Rotate(0, currentRobotRotation * Time.deltaTime, 0);

        // Корректируем начальное значение для плавности
        if (Mathf.Abs(currentRobotRotation) < 1f)
        {
            initialHeadYaw = currentHeadYaw;
        }
    }

    // Нормализует угол в диапазон [-180, 180]
    float NormalizeAngle(float angle)
    {
        while (angle > 180f) angle -= 360f;
        while (angle < -180f) angle += 360f;
        return angle;
    }

    // Метод для сброса системы (можно вызвать извне)
    public void ResetRotation()
    {
        initialHeadYaw = NormalizeAngle(xrCamera.transform.eulerAngles.y);
        targetRobotRotation = 0f;
        currentRobotRotation = 0f;
        rotationVelocity = 0f;
    }
}