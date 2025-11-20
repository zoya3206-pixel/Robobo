using System.Collections;
using UnityEngine;

public class RobotArmPunch : MonoBehaviour
{
    [System.Serializable]
    public class ArmJoints
    {
        public Transform shoulder;  // Плечо
        public Transform elbow;     // Локоть
        public Transform wrist;     // Кисть
        [HideInInspector] public Vector3 initialShoulderPos;
        [HideInInspector] public Vector3 initialElbowPos;
        [HideInInspector] public Vector3 initialWristPos;
    }

    [Header("Части рук робота")]
    public ArmJoints leftArm;
    public ArmJoints rightArm;

    [Header("Настройки удара")]
    public float punchDelay = 0.3f;        // Задержка после триггера
    public float punchSpeed = 2f;          // Скорость удара (постоянная)
    public float punchDistance = 1.5f;     // Дистанция удара
    public float returnSpeed = 1f;         // Скорость возврата

    [Header("Цели для удара (опционально)")]
    public Transform leftPunchTarget;
    public Transform rightPunchTarget;

    private bool leftArmPunching = false;
    private bool rightArmPunching = false;
    private Coroutine leftPunchCoroutine;
    private Coroutine rightPunchCoroutine;

    void Start()
    {
        // Сохраняем начальные позиции
        SaveInitialPositions(leftArm);
        SaveInitialPositions(rightArm);
    }

    void SaveInitialPositions(ArmJoints arm)
    {
        arm.initialShoulderPos = arm.shoulder.localPosition;
        arm.initialElbowPos = arm.elbow.localPosition;
        arm.initialWristPos = arm.wrist.localPosition;
    }

    // Вызывается из триггера на руках игрока
    public void TriggerPunch(bool isLeftHand)
    {
        if (isLeftHand && !leftArmPunching)
        {
            if (leftPunchCoroutine != null)
                StopCoroutine(leftPunchCoroutine);
            leftPunchCoroutine = StartCoroutine(PerformPunchSequence(leftArm, true));
        }
        else if (!isLeftHand && !rightArmPunching)
        {
            if (rightPunchCoroutine != null)
                StopCoroutine(rightPunchCoroutine);
            rightPunchCoroutine = StartCoroutine(PerformPunchSequence(rightArm, false));
        }
    }

    IEnumerator PerformPunchSequence(ArmJoints arm, bool isLeft)
    {
        if (isLeft) leftArmPunching = true;
        else rightArmPunching = true;

        // Задержка перед ударом
        yield return new WaitForSeconds(punchDelay);

        // Выполняем удар
        yield return StartCoroutine(PerformPunch(arm, isLeft));

        // Возвращаем руку
        yield return StartCoroutine(ReturnArm(arm));

        if (isLeft) leftArmPunching = false;
        else rightArmPunching = false;
    }

    IEnumerator PerformPunch(ArmJoints arm, bool isLeft)
    {
        Vector3 targetPosition = GetPunchTargetPosition(isLeft);
        float distance = 0f;
        float maxDistance = Vector3.Distance(arm.wrist.position, targetPosition);

        while (distance < maxDistance)
        {
            distance += punchSpeed * Time.deltaTime;
            float t = distance / maxDistance;

            // Двигаем все части руки к цели удара
            MoveArmToTarget(arm, targetPosition, t);

            yield return null;
        }
    }

    IEnumerator ReturnArm(ArmJoints arm)
    {
        float t = 1f;

        while (t > 0f)
        {
            t -= returnSpeed * Time.deltaTime;
            t = Mathf.Clamp01(t);

            // Возвращаем руку в исходное положение
            ReturnArmToInitial(arm, t);

            yield return null;
        }

        // Гарантируем точное возвращение
        ReturnArmToInitial(arm, 0f);
    }

    Vector3 GetPunchTargetPosition(bool isLeft)
    {
        // Если заданы цели удара - используем их
        if (isLeft && leftPunchTarget != null)
            return leftPunchTarget.position;
        if (!isLeft && rightPunchTarget != null)
            return rightPunchTarget.position;

        // Иначе вычисляем цель удара вперед от робота
        Vector3 forward = transform.forward;
        Vector3 side = isLeft ? -transform.right : transform.right;

        return transform.position + forward * punchDistance + side * 0.5f;
    }

    void MoveArmToTarget(ArmJoints arm, Vector3 target, float t)
    {
        // Простая анимация выпрямления руки
        // Плечо немного поднимается и поворачивается
        arm.shoulder.localPosition = Vector3.Lerp(
            arm.initialShoulderPos,
            arm.initialShoulderPos + new Vector3(0, 0.1f, 0.1f),
            t
        );

        // Локоть выпрямляется
        arm.elbow.localPosition = Vector3.Lerp(
            arm.initialElbowPos,
            arm.initialElbowPos + new Vector3(0, 0.2f, 0.3f),
            t
        );

        // Кисть достигает цели
        arm.wrist.position = Vector3.Lerp(arm.wrist.position, target, t);

        // Поворачиваем суставы для естественного движения
        arm.shoulder.LookAt(target);
        arm.elbow.LookAt(target);
    }

    void ReturnArmToInitial(ArmJoints arm, float t)
    {
        // Возвращаем все части в исходное положение
        arm.shoulder.localPosition = Vector3.Lerp(
            arm.initialShoulderPos + new Vector3(0, 0.1f, 0.1f),
            arm.initialShoulderPos,
            t
        );

        arm.elbow.localPosition = Vector3.Lerp(
            arm.initialElbowPos + new Vector3(0, 0.2f, 0.3f),
            arm.initialElbowPos,
            t
        );

        arm.wrist.localPosition = Vector3.Lerp(
            arm.wrist.localPosition,
            arm.initialWristPos,
            t
        );
    }
}