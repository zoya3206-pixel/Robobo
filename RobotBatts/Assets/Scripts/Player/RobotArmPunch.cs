using System.Collections;
using UnityEngine;
using Bhaptics.SDK2;
using Unity.XR.CoreUtils;

public class RobotArmPunchController : MonoBehaviour
{
    [Header("Robot Arm References")]
    [SerializeField] public Transform LeftBicep;
    [SerializeField] public Transform LeftForearm;
    [SerializeField] public Transform RightBicep;
    [SerializeField] public Transform RightForearm;

    [Header("Урон ударов")]
    [SerializeField] private float punchDamage = 450f;

    [Header("Capsule Controller")]
    [SerializeField] private FutuRiftCapsuleController capsuleController;

    [Header("Время анимации")]
    [SerializeField] private float punchAnimationTime = 1.0f;
    [SerializeField] private float holdBeforeReturnTime = 1.0f; 

    public bool IsLeftArmPunching = false;
    public bool IsRightArmPunching = false;

    private Vector3 leftForearmCurrentPos;
    private Quaternion leftForearmCurrentRot;
    private Vector3 leftBicepCurrentPos;
    private Quaternion leftBicepCurrentRot;

    private Vector3 rightForearmCurrentPos;
    private Quaternion rightForearmCurrentRot;
    private Vector3 rightBicepCurrentPos;
    private Quaternion rightBicepCurrentRot;

    private float leftPunchTimer = 0f;
    private float rightPunchTimer = 0f;

    private bool leftPunchDamageDealt = false;
    private bool rightPunchDamageDealt = false;
    private bool leftIsInHitState = false;
    private bool rightIsInHitState = false;

    private void Start()
    {
        UpdateCurrentArmPositions();
    }

    private void UpdateCurrentArmPositions()
    {
        if (LeftForearm != null && LeftBicep != null)
        {
            leftForearmCurrentPos = LeftForearm.localPosition;
            leftForearmCurrentRot = LeftForearm.localRotation;
            leftBicepCurrentPos = LeftBicep.localPosition;
            leftBicepCurrentRot = LeftBicep.localRotation;
        }

        if (RightForearm != null && RightBicep != null)
        {
            rightForearmCurrentPos = RightForearm.localPosition;
            rightForearmCurrentRot = RightForearm.localRotation;
            rightBicepCurrentPos = RightBicep.localPosition;
            rightBicepCurrentRot = RightBicep.localRotation;
        }
    }

    private void Update()
    {
        if (IsRightArmPunching)
        {
            HandleRightArmPunch();
        }
        if (IsLeftArmPunching)
        {
            HandleLeftArmPunch();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("LeftController"))
        {
            if (gameObject.CompareTag("Hit") && !IsLeftArmPunching)
            {
                StartLeftArmPunch();
            }
        }
        else if (other.CompareTag("RightController"))
        {
            if (gameObject.CompareTag("Hit") && !IsRightArmPunching)
            {
                StartRightArmPunch();
            }
        }
        else if (other.CompareTag("Enemy"))
        {
            if (IsLeftArmPunching && !leftPunchDamageDealt && leftIsInHitState)
            {
                DealPunchDamage(other);
                leftPunchDamageDealt = true;
            }
            else if (IsRightArmPunching && !rightPunchDamageDealt && rightIsInHitState)
            {
                DealPunchDamage(other);
                rightPunchDamageDealt = true;
            }
        }
    }

    private void StartLeftArmPunch()
    {
        IsLeftArmPunching = true;
        leftPunchTimer = 0f;
        leftPunchDamageDealt = false;
        leftIsInHitState = false;
        UpdateCurrentArmPositions();

    }

    private void StartRightArmPunch()
    {
        IsRightArmPunching = true;
        rightPunchTimer = 0f;
        rightPunchDamageDealt = false;
        rightIsInHitState = false;
        UpdateCurrentArmPositions();
    }

    private void HandleLeftArmPunch()
    {
        leftPunchTimer += Time.deltaTime;
        float normalizedTime = leftPunchTimer / punchAnimationTime;
        Vector3 forearmHitPos = new Vector3(65.2600021f, 61.2999992f, -45.0099983f);
        Quaternion forearmHitRot = new Quaternion(0.383888751f, -0.264530629f, 0.28698501f, 0.836834967f);
        Vector3 bicepHitPos = new Vector3(8.72745323f, 33.3327637f, -76.2455902f);
        Quaternion bicepHitRot = new Quaternion(0.434348106f, -0.259771347f, -0.0657111406f, 0.859966695f);
        Vector3 forearmIdlePos = new Vector3(63.2700005f, 83.1303101f, -61.6399994f);
        Quaternion forearmIdleRot = new Quaternion(0.680873752f, -0.355771303f, 0.160510227f, 0.619737208f);
        Vector3 bicepIdlePos = new Vector3(1.49691522f, 2.73115563f, 2.0158751f);
        Quaternion bicepIdleRot = new Quaternion(0.0343496725f, -0.224308044f, 0.00791161321f, 0.973880649f);

        if (normalizedTime <= 0.5f) 
        {
            float punchProgress = normalizedTime * 2f; // 0 до 1
            float smoothProgress = Mathf.SmoothStep(0f, 1f, punchProgress);

            LeftForearm.localPosition = Vector3.Lerp(leftForearmCurrentPos, forearmHitPos, smoothProgress);
            LeftForearm.localRotation = Quaternion.Slerp(leftForearmCurrentRot, forearmHitRot, smoothProgress);

            LeftBicep.localPosition = Vector3.Lerp(leftBicepCurrentPos, bicepHitPos, smoothProgress);
            LeftBicep.localRotation = Quaternion.Slerp(leftBicepCurrentRot, bicepHitRot, smoothProgress);
            if (punchProgress >= 0.5f && !leftIsInHitState)
            {
                leftIsInHitState = true;
                BhapticsLibrary.Play("lefthit");
                capsuleController?.TriggerNockbackTilt();
            }
        }
        else if (normalizedTime <= 0.5f + (holdBeforeReturnTime / punchAnimationTime))
        {
            LeftForearm.localPosition = forearmHitPos;
            LeftForearm.localRotation = forearmHitRot;
            LeftBicep.localPosition = bicepHitPos;
            LeftBicep.localRotation = bicepHitRot;
        }
        else 
        {
            float returnProgress = (normalizedTime - 0.5f - (holdBeforeReturnTime / punchAnimationTime)) / (0.5f);
            returnProgress = Mathf.Clamp01(returnProgress);
            float smoothReturnProgress = Mathf.SmoothStep(0f, 1f, returnProgress);
            LeftForearm.localPosition = Vector3.Lerp(forearmHitPos, forearmIdlePos, smoothReturnProgress);
            LeftForearm.localRotation = Quaternion.Slerp(forearmHitRot, forearmIdleRot, smoothReturnProgress);

            LeftBicep.localPosition = Vector3.Lerp(bicepHitPos, bicepIdlePos, smoothReturnProgress);
            LeftBicep.localRotation = Quaternion.Slerp(bicepHitRot, bicepIdleRot, smoothReturnProgress);

            if (returnProgress >= 1f)
            {
                // Завершаем удар
                LeftForearm.localPosition = forearmIdlePos;
                LeftForearm.localRotation = forearmIdleRot;
                LeftBicep.localPosition = bicepIdlePos;
                LeftBicep.localRotation = bicepIdleRot;

                IsLeftArmPunching = false;
                leftPunchTimer = 0f;
                leftIsInHitState = false;
                capsuleController?.StopAllTilts();
            }
        }
    }

    private void HandleRightArmPunch()
    {
        rightPunchTimer += Time.deltaTime;
        float normalizedTime = rightPunchTimer / punchAnimationTime;

        Vector3 forearmHitPos = new Vector3(65.2600021f, 61.2999992f, -45.0099983f);
        Quaternion forearmHitRot = new Quaternion(0.383888751f, -0.264530629f, 0.28698501f, 0.836834967f);
        Vector3 bicepHitPos = new Vector3(8.72745323f, 33.3327637f, -76.2455902f);
        Quaternion bicepHitRot = new Quaternion(0.434348106f, -0.259771347f, -0.0657111406f, 0.859966695f);
        Vector3 forearmIdlePos = new Vector3(63.2700005f, 83.1303101f, -61.6399994f);
        Quaternion forearmIdleRot = new Quaternion(0.680873752f, -0.355771303f, 0.160510227f, 0.619737208f);
        Vector3 bicepIdlePos = new Vector3(1.49691522f, 2.73115563f, 2.0158751f);
        Quaternion bicepIdleRot = new Quaternion(0.0343496725f, -0.224308044f, 0.00791161321f, 0.973880649f);

        if (normalizedTime <= 0.5f)
        {
            float punchProgress = normalizedTime * 2f; // 0 до 1
            float smoothProgress = Mathf.SmoothStep(0f, 1f, punchProgress);

            RightForearm.localPosition = Vector3.Lerp(rightForearmCurrentPos, forearmHitPos, smoothProgress);
            RightForearm.localRotation = Quaternion.Slerp(rightForearmCurrentRot, forearmHitRot, smoothProgress);

            RightBicep.localPosition = Vector3.Lerp(rightBicepCurrentPos, bicepHitPos, smoothProgress);
            RightBicep.localRotation = Quaternion.Slerp(rightBicepCurrentRot, bicepHitRot, smoothProgress);

            if (punchProgress >= 0.5f && !rightIsInHitState)
            {
                rightIsInHitState = true;
                BhapticsLibrary.Play("righthit");
                capsuleController?.TriggerNockbackTilt();
            }
        }
        else if (normalizedTime <= 0.5f + (holdBeforeReturnTime / punchAnimationTime)) 
        {
            RightForearm.localPosition = forearmHitPos;
            RightForearm.localRotation = forearmHitRot;
            RightBicep.localPosition = bicepHitPos;
            RightBicep.localRotation = bicepHitRot;
        }
        else 
        {
            float returnProgress = (normalizedTime - 0.5f - (holdBeforeReturnTime / punchAnimationTime)) / (0.5f);
            returnProgress = Mathf.Clamp01(returnProgress);
            float smoothReturnProgress = Mathf.SmoothStep(0f, 1f, returnProgress);

            RightForearm.localPosition = Vector3.Lerp(forearmHitPos, forearmIdlePos, smoothReturnProgress);
            RightForearm.localRotation = Quaternion.Slerp(forearmHitRot, forearmIdleRot, smoothReturnProgress);

            RightBicep.localPosition = Vector3.Lerp(bicepHitPos, bicepIdlePos, smoothReturnProgress);
            RightBicep.localRotation = Quaternion.Slerp(bicepHitRot, bicepIdleRot, smoothReturnProgress);

            if (returnProgress >= 1f)
            {
                RightForearm.localPosition = forearmIdlePos;
                RightForearm.localRotation = forearmIdleRot;
                RightBicep.localPosition = bicepIdlePos;
                RightBicep.localRotation = bicepIdleRot;

                IsRightArmPunching = false;
                rightPunchTimer = 0f;
                rightIsInHitState = false;
                capsuleController?.StopAllTilts();
            }
        }
    }

    private void DealPunchDamage(Collider enemyCollider)
    {
        EnemyHealth enemy = enemyCollider.GetComponent<EnemyHealth>();
        if (enemy != null && !enemy.IsDead())
        {
            enemy.TakeDamage(punchDamage);
        }
    }
}