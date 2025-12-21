using UnityEngine;

public class PlayerPunchDamage : MonoBehaviour
{
    [Header("Настройки урона")]
    [SerializeField] private float normalPunchDamage = 20f;     
    [SerializeField] private float ultimatePunchDamage = 100f; 
    [SerializeField] private float punchCooldown = 0.5f;     
    [SerializeField] private float punchRange = 1.5f;  
    [SerializeField] private LayerMask enemyLayer;         

    [Header("Ссылки")]
    [SerializeField] private Transform leftHand;
    [SerializeField] private Transform rightHand;
    [SerializeField] private RobotUltimateController ultimateController;

    private bool canLeftPunch = true;
    private bool canRightPunch = true;
    private EnemyHealth currentEnemyHealth;
    private RobotArmPunchController punchController;

    void Start()
    {
        GameObject enemy = GameObject.FindGameObjectWithTag("Enemy");
        if (enemy != null)
        {
            currentEnemyHealth = enemy.GetComponent<EnemyHealth>();
        }
        punchController = GetComponent<RobotArmPunchController>();
        if (punchController == null)
        {
            punchController = FindObjectOfType<RobotArmPunchController>();
        }
        if (ultimateController == null)
        {
            ultimateController = FindObjectOfType<RobotUltimateController>();
        }
    }

    void Update()
    {
        if (punchController != null)
        {
            if (punchController.IsLeftArmPunching && canLeftPunch)
            {
                CheckAndApplyPunch(leftHand);
                StartCoroutine(PunchCooldown(true));
            }

            if (punchController.IsRightArmPunching && canRightPunch)
            {
                CheckAndApplyPunch(rightHand);
                StartCoroutine(PunchCooldown(false));
            }
        }
    }

    void CheckAndApplyPunch(Transform hand)
    {
        if (currentEnemyHealth == null || currentEnemyHealth.IsDead()) return;
        float distance = Vector3.Distance(hand.position, currentEnemyHealth.transform.position);

        if (distance <= punchRange)
        {
            ApplyDamage();
        }
    }

    void ApplyDamage()
    {
        float damage = normalPunchDamage;

        if (ultimateController != null && ultimateController.isUltimateActive)
        {
            damage = ultimatePunchDamage;
        }
        currentEnemyHealth.TakeDamage(damage);
    }

    System.Collections.IEnumerator PunchCooldown(bool isLeft)
    {
        if (isLeft)
        {
            canLeftPunch = false;
            yield return new WaitForSeconds(punchCooldown);
            canLeftPunch = true;
        }
        else
        {
            canRightPunch = false;
            yield return new WaitForSeconds(punchCooldown);
            canRightPunch = true;
        }
    }
}