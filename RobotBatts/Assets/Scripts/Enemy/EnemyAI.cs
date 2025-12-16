using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class EnemyAI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform player;
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private Animator animator;
    [SerializeField] private EnemyShooting shooting;

    [Header("Detection Settings")]
    [SerializeField] private float sightRange = 15f;
    [SerializeField] private float attackRange = 10f;
    [SerializeField] private float patrolRange = 20f;
    [SerializeField] private float initialIdleTime = 5f;

    [Header("Berserk Mode")]
    [SerializeField] private float berserkStartTime = 60f;
    [SerializeField] private float berserkDuration = 20f;
    [SerializeField] private float stunDuration = 30f;

    [Header("Attack Settings")]
    [SerializeField] private float attackCooldown = 2f;

    private enum AIState { Idle, Patrol, Chase, Attack, Berserk, Stunned }
    private AIState currentState = AIState.Idle;

    private Vector3 startPosition;
    private bool isDead = false;
    private float gameStartTime;
    private float modeTimer = 0f;
    private float attackTimer = 0f;
    private bool inBerserkMode = false;
    private bool inStunMode = false;
    private bool canAttack = true;

    void Start()
    {
        startPosition = transform.position;

        if (agent == null) agent = GetComponent<NavMeshAgent>();
        if (animator == null) animator = GetComponent<Animator>();
        if (shooting == null) shooting = GetComponent<EnemyShooting>();

        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null) player = playerObj.transform;
        }

        // Отключаем автоматический поворот агента
        if (agent != null)
        {
            agent.updateRotation = false;  // ВАЖНО!
            agent.updatePosition = true;
        }

        gameStartTime = Time.time;
        StartCoroutine(InitialIdle());
    }

    private void RotateTowardsPlayer()
    {
        if (player == null) return;

        Vector3 direction = player.position - transform.position;
        direction.y = 0; // Игнорируем разницу по высоте

        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 5f);
        }
    }

    private IEnumerator InitialIdle()
    {
        SetState(AIState.Idle);
        yield return new WaitForSeconds(initialIdleTime);
        SetState(AIState.Patrol);
    }

    void Update()
    {
        if (isDead) return;

        CheckModeTimers();
        UpdateAttackCooldown();

        if (currentState == AIState.Stunned) return;

        UpdateAnimator();

        if (player == null) return;

        float distance = Vector3.Distance(transform.position, player.position);

        // Поворачиваемся к игроку в состояниях атаки и преследования
        if (currentState == AIState.Attack || currentState == AIState.Berserk)
        {
            RotateTowardsPlayer();
        }

        // Логика состояний
        switch (currentState)
        {
            case AIState.Patrol:
                if (distance <= sightRange)
                {
                    SetState(AIState.Chase);
                }
                break;

            case AIState.Chase:
                if (distance <= attackRange)
                {
                    SetState(AIState.Attack);
                }
                else if (distance > sightRange)
                {
                    SetState(AIState.Patrol);
                }
                else
                {
                    agent.SetDestination(player.position);
                }
                break;

            case AIState.Attack:
                if (distance > attackRange)
                {
                    SetState(AIState.Chase);
                }
                else if (distance > sightRange)
                {
                    SetState(AIState.Patrol);
                }
                else
                {
                    // Стреляем, если можно
                    if (canAttack)
                    {
                        shooting.StartShooting(false);
                        canAttack = false;
                        attackTimer = attackCooldown;
                    }
                }
                break;

            case AIState.Berserk:
                agent.SetDestination(player.position);
                shooting.StartShooting(true);
                break;
        }
    }

    private void UpdateAttackCooldown()
    {
        if (!canAttack)
        {
            attackTimer -= Time.deltaTime;
            if (attackTimer <= 0)
            {
                canAttack = true;
                shooting.StopShooting();
            }
        }
    }

    private void CheckModeTimers()
    {
        float timeSinceStart = Time.time - gameStartTime;

        // Через 60 секунд - режим безумия
        if (timeSinceStart > berserkStartTime && !inBerserkMode && !inStunMode)
        {
            StartBerserkMode();
        }

        if (inBerserkMode)
        {
            modeTimer += Time.deltaTime;

            if (modeTimer > berserkDuration)
            {
                EndBerserkMode();
                StartStunMode();
            }
        }

        if (inStunMode)
        {
            modeTimer += Time.deltaTime;

            if (modeTimer > stunDuration)
            {
                EndStunMode();
                gameStartTime = Time.time;
            }
        }
    }

    private void StartBerserkMode()
    {
        inBerserkMode = true;
        modeTimer = 0f;
        SetState(AIState.Berserk);
        agent.speed = 6f; // Увеличиваем скорость в режиме безумия
        Debug.Log("Безумие началось! Враг неистово стреляет!");
    }

    private void EndBerserkMode()
    {
        inBerserkMode = false;
        modeTimer = 0f;
        shooting.StopShooting();
        agent.speed = 3.5f; // Возвращаем нормальную скорость
    }

    private void StartStunMode()
    {
        inStunMode = true;
        modeTimer = 0f;
        SetState(AIState.Stunned);
        shooting.StopShooting();
        Debug.Log("Враг оглушён на 30 секунд!");
    }

    private void EndStunMode()
    {
        inStunMode = false;
        modeTimer = 0f;
        SetState(AIState.Patrol);
        Debug.Log("Враг оправился от оглушения!");
    }

    private void SetState(AIState newState)
    {
        if (currentState == newState) return;

        currentState = newState;

        // Остановка стрельбы при смене состояния
        if (newState != AIState.Attack && newState != AIState.Berserk)
        {
            shooting.StopShooting();
        }

        switch (newState)
        {
            case AIState.Idle:
            case AIState.Attack:
            case AIState.Stunned:
                agent.isStopped = true;
                break;

            case AIState.Patrol:
            case AIState.Chase:
            case AIState.Berserk:
                agent.isStopped = false;
                break;
        }

        UpdateAnimatorParameters();
    }

    private void UpdateAnimatorParameters()
    {
        animator.SetBool("IsShooting", false);
        animator.SetBool("IsStunned", false);

        if (currentState == AIState.Attack || currentState == AIState.Berserk)
        {
            animator.SetBool("IsShooting", true);
        }
        else if (currentState == AIState.Stunned)
        {
            animator.SetBool("IsStunned", true);
        }
    }

    private void UpdateAnimator()
    {
        if (animator == null) return;

        // ВРЕМЕННО УБИРАЕМ Speed - будем настраивать позже
        // float speed = 0f;
        // if (currentState == AIState.Patrol || currentState == AIState.Chase || currentState == AIState.Berserk)
        // {
        //     speed = agent.velocity.magnitude / agent.speed;
        // }
        // animator.SetFloat("Speed", speed);

        // Просто включаем анимацию ходьбы, если двигаемся
        if (agent.velocity.magnitude > 0.1f)
        {
            animator.SetBool("IsWalking", true);
        }
        else
        {
            animator.SetBool("IsWalking", false);
        }

        if (currentState == AIState.Patrol)
        {
            FindPatrolPoint();
        }
    }

    private void FindPatrolPoint()
    {
        if (!agent.pathPending && agent.remainingDistance < 1f)
        {
            Vector3 randomDirection = Random.insideUnitSphere * patrolRange;
            randomDirection += startPosition;
            randomDirection.y = startPosition.y;

            NavMeshHit hit;
            if (NavMesh.SamplePosition(randomDirection, out hit, patrolRange, NavMesh.AllAreas))
            {
                agent.SetDestination(hit.position);
            }
        }
    }

    public void SetDead(bool dead)
    {
        isDead = dead;
        agent.isStopped = true;
        enabled = false;

        if (animator != null)
            animator.SetBool("IsDead", true);
    }
}