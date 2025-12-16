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

    [Header("Settings")]
    [SerializeField] private float sightRange = 15f;
    [SerializeField] private float attackRange = 10f;
    [SerializeField] private float patrolRange = 20f;
    [SerializeField] private float initialIdleTime = 5f;

    [Header("Berserk Mode")]
    [SerializeField] private float berserkStartTime = 60f;
    [SerializeField] private float berserkDuration = 20f;
    [SerializeField] private float stunDuration = 30f;

    private enum AIState { Idle, Patrol, Chase, Attack, Berserk, Stunned }
    private AIState currentState = AIState.Idle;

    private Vector3 startPosition;
    private bool isDead = false;
    private float gameStartTime;
    private float modeTimer = 0f;
    private bool inBerserkMode = false;
    private bool inStunMode = false;

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

        gameStartTime = Time.time;
        StartCoroutine(InitialIdle());
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

        if (currentState == AIState.Stunned) return;

        // Обновляем аниматор
        UpdateAnimator();

        // Простая логика
        if (player != null)
        {
            float distance = Vector3.Distance(transform.position, player.position);

            if (distance <= attackRange && currentState != AIState.Berserk)
            {
                SetState(AIState.Attack);
            }
            else if (distance <= sightRange && currentState != AIState.Berserk)
            {
                SetState(AIState.Chase);
            }
            else if (currentState != AIState.Patrol && currentState != AIState.Berserk)
            {
                SetState(AIState.Patrol);
            }

            // В режиме преследования двигаемся к игроку
            if (currentState == AIState.Chase || currentState == AIState.Berserk)
            {
                agent.SetDestination(player.position);
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
                gameStartTime = Time.time; // Сброс таймера
            }
        }
    }

    private void StartBerserkMode()
    {
        inBerserkMode = true;
        modeTimer = 0f;
        SetState(AIState.Berserk);
        Debug.Log("Безумие началось!");
    }

    private void EndBerserkMode()
    {
        inBerserkMode = false;
        modeTimer = 0f;
        if (shooting != null) shooting.StopShooting();
    }

    private void StartStunMode()
    {
        inStunMode = true;
        modeTimer = 0f;
        SetState(AIState.Stunned);
        Debug.Log("Оглушение на 30 секунд");
    }

    private void EndStunMode()
    {
        inStunMode = false;
        modeTimer = 0f;
        SetState(AIState.Patrol);
    }

    private void SetState(AIState newState)
    {
        currentState = newState;

        // Остановка стрельбы
        if (shooting != null && newState != AIState.Attack && newState != AIState.Berserk)
        {
            shooting.StopShooting();
        }

        // Настройка агента
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

        // Управление аниматором
        if (animator != null)
        {
            // Сначала сбрасываем все bool параметры
            animator.SetBool("IsShooting", false);
            animator.SetBool("IsStunned", false);

            // Затем устанавливаем нужные
            if (newState == AIState.Attack || newState == AIState.Berserk)
            {
                animator.SetBool("IsShooting", true);
            }
            else if (newState == AIState.Stunned)
            {
                animator.SetBool("IsStunned", true);
            }
        }
    }

    private void UpdateAnimator()
    {
        if (animator == null) return;

        // Speed для ходьбы/бега
        float speed = 0f;
        if (currentState == AIState.Patrol || currentState == AIState.Chase || currentState == AIState.Berserk)
        {
            speed = agent.velocity.magnitude / agent.speed;
        }
        animator.SetFloat("Speed", speed);

        // Патрулирование
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

            NavMeshHit hit;
            if (NavMesh.SamplePosition(randomDirection, out hit, patrolRange, 1))
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