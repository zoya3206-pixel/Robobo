using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class EnemyController : MonoBehaviour
{
    [Header("Основные настройки")]
    [SerializeField] private Transform playerTarget;
    [SerializeField] private float startDelay = 5f;
    [SerializeField] private float moveSpeed = 3.5f;
    [SerializeField] private float rotationSpeed = 10f;

    [Header("Дистанции поведения")]
    [SerializeField] private float meleeDangerRange = 2f;
    [SerializeField] private float meleeComfortRange = 4f;
    [SerializeField] private float optimalShootingRange = 8f;
    [SerializeField] private float maxShootingRange = 15f;

    [Header("Боевые настройки")]
    [SerializeField] private float timeBetweenShots = 2f;
    [SerializeField] private float retreatDistance = 10f;
    [SerializeField] private int minShotsBeforeRetreat = 2;
    [SerializeField] private int maxShotsBeforeRetreat = 5;
    [SerializeField] private float retreatWaitTime = 3f;

    [Header("Режим бешенства")]
    [SerializeField] private float berserkStartTime = 60f;
    [SerializeField] private float berserkDuration = 20f;
    [SerializeField] private float stunDuration = 20f;
    [SerializeField] private float berserkRetreatDistance = 6f;

    [Header("Очередь стрельбы и анимация")]
    [SerializeField] private int normalBurstCount = 3;
    [SerializeField] private float burstInterval = 0.3f;
    [SerializeField] private int berserkBurstCount = 6;
    [SerializeField] private float aimingTime = 1f;

    [Header("Проверка стен")]
    [SerializeField] private float wallCheckDistance = 0.3f;
    [SerializeField] private LayerMask wallLayerMask = ~0;

    private NavMeshAgent agent;
    private Animator animator;
    private EnemyShooter shooter;
    private EnemyHealth enemyHealth;

    private enum State { Idle, Approach, Shooting, Retreating, Berserk, Stun }
    private State currentState = State.Idle;

    private bool gameStarted = false;
    private bool canShoot = true;
    private bool isBerserk = false;
    private bool isShootingBurst = false;
    private bool isStunned = false;
    private int shotsToMakeBeforeRetreat;
    private int shotsMadeInCurrentPhase;
    private float lastShotTime = 0f;
    private float stateEnterTime = 0f;
    private Coroutine shootingCoroutine;

    private float retreatStartDistance;
    private Vector3 retreatDirection;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponentInChildren<Animator>();
        shooter = GetComponent<EnemyShooter>();
        enemyHealth = GetComponent<EnemyHealth>();

        agent.speed = moveSpeed;
        agent.angularSpeed = 360f;
        agent.acceleration = 8f;
        agent.updateRotation = false;

        if (playerTarget == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null) playerTarget = player.transform;
        }

        shotsToMakeBeforeRetreat = Random.Range(minShotsBeforeRetreat, maxShotsBeforeRetreat + 1);
        shotsMadeInCurrentPhase = 0;

        StartCoroutine(StartDelay());
        StartCoroutine(BerserkModeTimer());
    }

    IEnumerator StartDelay()
    {
        SetState(State.Idle);
        agent.isStopped = true;

        yield return new WaitForSeconds(startDelay);

        gameStarted = true;
        SetState(State.Approach);
        agent.isStopped = false;
    }

    IEnumerator BerserkModeTimer()
    {
        yield return new WaitForSeconds(berserkStartTime);

        isBerserk = true;
        SetState(State.Berserk);
        shooter.SetBerserkMode(true);

        yield return new WaitForSeconds(berserkDuration);

        isBerserk = false;
        shooter.SetBerserkMode(false);
        isStunned = true;
        SetState(State.Stun);

        yield return new WaitForSeconds(stunDuration);

        isStunned = false;
        SetState(State.Approach);
    }

    void Update()
    {
        if (!gameStarted || playerTarget == null || enemyHealth.IsDead())
            return;

        float distanceToPlayer = Vector3.Distance(transform.position, playerTarget.position);

        switch (currentState)
        {
            case State.Approach:
                HandleApproachState(distanceToPlayer);
                break;

            case State.Shooting:
                HandleShootingState(distanceToPlayer);
                break;

            case State.Retreating:
                HandleRetreatingState(distanceToPlayer);
                break;

            case State.Berserk:
                HandleBerserkState(distanceToPlayer);
                break;

            case State.Stun:
                HandleStunState();
                break;
        }

        UpdateAnimation();
    }

    void HandleApproachState(float distance)
    {
        if (distance < meleeDangerRange)
        {
            EmergencyRetreat();
            return;
        }

        if (distance <= meleeComfortRange && distance >= meleeDangerRange)
        {
            if (Random.value > 0.5f)
            {
                SetState(State.Shooting);
            }
            else
            {
                StartRetreat();
            }
            return;
        }

        if (distance <= optimalShootingRange && distance > meleeComfortRange)
        {
            SetState(State.Shooting);
            return;
        }

        if (distance > optimalShootingRange)
        {
            agent.isStopped = false;

            Vector3 directionToPlayer = (playerTarget.position - transform.position).normalized;
            Vector3 targetPosition = playerTarget.position - directionToPlayer * (optimalShootingRange - 1f);

            agent.SetDestination(targetPosition);
            RotateTowardsPlayer();
        }
        else
        {
            agent.isStopped = false;

            Vector3 directionToPlayer = (playerTarget.position - transform.position).normalized;
            Vector3 targetPosition = playerTarget.position - directionToPlayer * optimalShootingRange;

            agent.SetDestination(targetPosition);
            RotateTowardsPlayer();
        }
    }

    void HandleShootingState(float distance)
    {
        if (distance < meleeDangerRange)
        {
            EmergencyRetreat();
            return;
        }

        if (distance > maxShootingRange)
        {
            SetState(State.Approach);
            return;
        }

        agent.isStopped = true;
        RotateTowardsPlayer();

        if (canShoot && Time.time - lastShotTime > timeBetweenShots && !isShootingBurst)
        {
            shootingCoroutine = StartCoroutine(ShootBurst());
        }

        if (!isShootingBurst && shotsMadeInCurrentPhase >= shotsToMakeBeforeRetreat)
        {
            if (distance < meleeComfortRange || Random.value > 0.7f)
            {
                StartRetreat();
            }
            else
            {
                shotsMadeInCurrentPhase = 0;
                shotsToMakeBeforeRetreat = Random.Range(minShotsBeforeRetreat, maxShotsBeforeRetreat + 1);
            }
        }
    }

    void HandleRetreatingState(float distance)
    {
        if (CheckForWall())
        {
            agent.isStopped = true;

            if (Time.time - stateEnterTime > 1f)
            {
                SetState(State.Shooting);
                shotsMadeInCurrentPhase = 0;
            }
            return;
        }

        float distanceTraveled = Vector3.Distance(transform.position,
            transform.position - retreatDirection * retreatStartDistance);

        if (distanceTraveled >= retreatDistance ||
            (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance))
        {
            agent.isStopped = true;

            if (Time.time - stateEnterTime > retreatWaitTime)
            {
                if (distance > meleeComfortRange)
                {
                    SetState(State.Approach);
                    shotsMadeInCurrentPhase = 0;
                    shotsToMakeBeforeRetreat = Random.Range(minShotsBeforeRetreat, maxShotsBeforeRetreat + 1);
                }
                else
                {
                    StartRetreat();
                }
            }
        }
        else
        {
            agent.isStopped = false;
        }

        RotateTowardsPlayer();
    }

    void HandleBerserkState(float distance)
    {
        if (distance < meleeDangerRange * 1.5f)
        {
            Vector3 retreatDir = (transform.position - playerTarget.position).normalized;
            Vector3 retreatPos = transform.position + retreatDir * berserkRetreatDistance;

            NavMeshHit hit;
            if (NavMesh.SamplePosition(retreatPos, out hit, 10f, NavMesh.AllAreas))
            {
                agent.isStopped = false;
                agent.SetDestination(hit.position);
            }
        }
        else if (distance > optimalShootingRange)
        {
            agent.isStopped = false;

            Vector3 directionToPlayer = (playerTarget.position - transform.position).normalized;
            Vector3 targetPosition = playerTarget.position - directionToPlayer * optimalShootingRange;

            agent.SetDestination(targetPosition);
        }
        else
        {
            agent.isStopped = true;
        }

        RotateTowardsPlayer();

        if (canShoot && Time.time - lastShotTime > 1f && !isShootingBurst)
        {
            shootingCoroutine = StartCoroutine(ShootBurst());
        }
    }

    void HandleStunState()
    {
        agent.isStopped = true;

        if (animator != null)
        {
            animator.SetBool("IsStunned", true);
            animator.SetBool("IsWalking", false);
            animator.SetBool("IsShooting", false);
        }
    }

    IEnumerator ShootBurst()
    {
        isShootingBurst = true;
        canShoot = false;

        if (animator != null)
        {
            animator.SetBool("IsShooting", true);
        }

        yield return new WaitForSeconds(aimingTime);

        int burstCount = isBerserk ? berserkBurstCount : normalBurstCount;

        for (int i = 0; i < burstCount; i++)
        {
            if (enemyHealth.IsDead() || playerTarget == null)
                break;

            float distance = Vector3.Distance(transform.position, playerTarget.position);
            if (distance < meleeDangerRange)
                break;

            if (shooter != null)
            {
                shooter.Shoot();
                shotsMadeInCurrentPhase++;
                lastShotTime = Time.time;
            }

            if (i < burstCount - 1)
                yield return new WaitForSeconds(burstInterval);
        }

        float shootingTime = (burstCount - 1) * burstInterval;
        float remainingAnimationTime = 1.5f - shootingTime;

        if (remainingAnimationTime > 0)
            yield return new WaitForSeconds(remainingAnimationTime);

        if (animator != null)
        {
            animator.SetBool("IsShooting", false);
        }

        isShootingBurst = false;

        float waitTime = isBerserk ? 0.5f : timeBetweenShots;
        float timeAlreadyPassed = aimingTime + shootingTime + (remainingAnimationTime > 0 ? remainingAnimationTime : 0);
        if (waitTime > timeAlreadyPassed)
        {
            yield return new WaitForSeconds(waitTime - timeAlreadyPassed);
        }

        canShoot = true;
    }

    void StartRetreat()
    {
        SetState(State.Retreating);
        stateEnterTime = Time.time;

        retreatStartDistance = Vector3.Distance(transform.position, playerTarget.position);
        retreatDirection = (transform.position - playerTarget.position).normalized;
        retreatDirection.y = 0;

        Vector3 retreatPos = transform.position + retreatDirection * retreatDistance;

        NavMeshHit hit;
        if (NavMesh.SamplePosition(retreatPos, out hit, 10f, NavMesh.AllAreas))
        {
            agent.isStopped = false;
            agent.SetDestination(hit.position);
        }
        else
        {
            retreatPos = transform.position + retreatDirection * (retreatDistance * 0.5f);
            if (NavMesh.SamplePosition(retreatPos, out hit, 10f, NavMesh.AllAreas))
            {
                agent.isStopped = false;
                agent.SetDestination(hit.position);
            }
        }
    }

    void EmergencyRetreat()
    {
        SetState(State.Retreating);
        stateEnterTime = Time.time;

        retreatStartDistance = Vector3.Distance(transform.position, playerTarget.position);
        retreatDirection = (transform.position - playerTarget.position).normalized;
        retreatDirection.y = 0;

        Vector3 retreatPos = transform.position + retreatDirection * (retreatDistance * 1.5f);

        NavMeshHit hit;
        if (NavMesh.SamplePosition(retreatPos, out hit, 10f, NavMesh.AllAreas))
        {
            agent.isStopped = false;
            agent.SetDestination(hit.position);
        }

        if (shootingCoroutine != null)
        {
            StopCoroutine(shootingCoroutine);
            isShootingBurst = false;
            canShoot = true;

            if (animator != null)
            {
                animator.SetBool("IsShooting", false);
            }
        }
    }

    bool CheckForWall()
    {
        Vector3 checkPosition = transform.position + Vector3.up * 0.5f;
        RaycastHit hit;

        if (retreatDirection != Vector3.zero)
        {
            if (Physics.Raycast(checkPosition, retreatDirection, out hit, wallCheckDistance, wallLayerMask))
            {
                if (!hit.collider.CompareTag("Player") &&
                    !hit.collider.CompareTag("EnemyBullet"))
                {
                    return true;
                }
            }
            Vector3 rightCheck = Quaternion.Euler(0, 30, 0) * retreatDirection;
            Vector3 leftCheck = Quaternion.Euler(0, -30, 0) * retreatDirection;

            if (Physics.Raycast(checkPosition, rightCheck, wallCheckDistance * 0.8f, wallLayerMask) ||
                Physics.Raycast(checkPosition, leftCheck, wallCheckDistance * 0.8f, wallLayerMask))
            {
                return true;
            }
        }

        return false;
    }

    void RotateTowardsPlayer()
    {
        if (playerTarget == null) return;

        Vector3 direction = playerTarget.position - transform.position;
        direction.y = 0;

        if (direction.magnitude > 0.1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation,
                Time.deltaTime * rotationSpeed);
        }
    }

    void SetState(State newState)
    {
        if (currentState == newState) return;

        currentState = newState;
        stateEnterTime = Time.time;

        if (animator != null)
        {
            animator.SetBool("IsWalking", false);
            animator.SetBool("IsShooting", false);

            if (newState != State.Stun)
            {
                animator.SetBool("IsStunned", false);
            }
        }

        if (newState == State.Retreating)
        {
            shotsMadeInCurrentPhase = 0;
        }

        if (newState != State.Shooting && newState != State.Berserk)
        {
            if (shootingCoroutine != null)
            {
                StopCoroutine(shootingCoroutine);
                isShootingBurst = false;
                canShoot = true;
            }
        }
    }

    void UpdateAnimation()
    {
        if (animator == null) return;

        bool isMoving = agent.velocity.magnitude > 0.3f && !agent.isStopped;

        if (currentState != State.Stun && !animator.GetBool("IsShooting"))
        {
            if (currentState == State.Retreating && isMoving)
            {
                animator.SetBool("IsWalking", true);
            }
            else if (currentState != State.Retreating)
            {
                animator.SetBool("IsWalking", isMoving);
            }
        }
    }
    public bool IsStunned
    {
        get { return isStunned; }
    }
}