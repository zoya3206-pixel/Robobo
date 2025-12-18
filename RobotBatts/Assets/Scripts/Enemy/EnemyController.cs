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

    [Header("Боевые настройки")]
    [SerializeField] private float shootingRange = 15f;
    [SerializeField] private float stoppingDistance = 10f;
    [SerializeField] private float timeBetweenShots = 2f;
    [SerializeField] private float retreatDistance = 25f;
    [SerializeField] private int maxShotsBeforeRetreat = 3;

    [Header("Режим бешенства")]
    [SerializeField] private float berserkStartTime = 60f;
    [SerializeField] private float berserkDuration = 20f;
    [SerializeField] private float stunDuration = 20f;
    [SerializeField] private float berserkRetreatDistance = 15f;

    [Header("Ожидание после отхода")]
    [SerializeField] private float waitAfterRetreatTime = 10f;

    // Компоненты
    private NavMeshAgent agent;
    private Animator animator;
    private EnemyShooter shooter;
    private EnemyHealth enemyHealth;

    // Состояния
    private enum State { Idle, Chase, Shooting, Retreat, Wait, Berserk, Stun }
    private State currentState = State.Idle;

    // Переменные
    private bool gameStarted = false;
    private bool canShoot = true;
    private bool isBerserk = false;
    private bool isStunned = false;
    private int shotsFired = 0;
    private float stateTimer = 0f;
    private float waitTimer = 0f;

    void Start()
    {
        // Получаем компоненты
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponentInChildren<Animator>();
        shooter = GetComponent<EnemyShooter>();
        enemyHealth = GetComponent<EnemyHealth>();

        // Настраиваем агента
        agent.speed = moveSpeed;
        agent.stoppingDistance = stoppingDistance;
        agent.angularSpeed = 360f;
        agent.acceleration = 8f;
        agent.updateRotation = false;

        // Находим игрока если не назначен
        if (playerTarget == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null) playerTarget = player.transform;
        }

        Debug.Log("Робот инициализирован. Начинаю задержку.");

        // Запускаем корутины
        StartCoroutine(StartDelay());
        StartCoroutine(BerserkModeTimer());
    }

    IEnumerator StartDelay()
    {
        SetState(State.Idle);
        agent.isStopped = true;

        // Сброс анимаций
        if (animator != null)
        {
            animator.SetBool("IsWalking", false);
            animator.SetBool("IsShooting", false);
            animator.SetBool("IsStunned", false);
        }

        yield return new WaitForSeconds(startDelay);

        gameStarted = true;
        SetState(State.Chase);
        agent.isStopped = false;

        Debug.Log("Робот активирован, начинаю преследование.");
    }

    IEnumerator BerserkModeTimer()
    {
        yield return new WaitForSeconds(berserkStartTime);

        // Включаем режим бешенства
        isBerserk = true;
        SetState(State.Berserk);
        shooter.SetBerserkMode(true);
        Debug.Log("РЕЖИМ БЕШЕНСТВА! 20 секунд непрерывного огня и отхода!");

        // Бешенство длится berserkDuration секунд
        yield return new WaitForSeconds(berserkDuration);

        // Выключаем бешенство, включаем оглушение
        isBerserk = false;
        isStunned = true;
        SetState(State.Stun);
        shooter.SetBerserkMode(false);
        Debug.Log($"Робот перегрелся. Оглушение на {stunDuration} секунд.");

        // Включаем анимацию оглушения
        if (animator != null)
        {
            animator.SetBool("IsStunned", true);
            animator.SetBool("IsWalking", false);
            animator.SetBool("IsShooting", false);
        }

        // Оглушение длится stunDuration секунд
        yield return new WaitForSeconds(stunDuration);

        // Возвращаемся к нормальному режиму
        isStunned = false;
        if (animator != null)
        {
            animator.SetBool("IsStunned", false);
        }

        SetState(State.Chase);
        Debug.Log("Робот пришел в себя. Возврат к обычному режиму.");
    }

    void Update()
    {
        if (!gameStarted || playerTarget == null || enemyHealth.IsDead())
            return;

        float distance = Vector3.Distance(transform.position, playerTarget.position);
        stateTimer += Time.deltaTime;

        switch (currentState)
        {
            case State.Chase:
                HandleChaseState(distance);
                break;

            case State.Shooting:
                HandleShootingState(distance);
                break;

            case State.Retreat:
                HandleRetreatState(distance);
                break;

            case State.Wait:
                HandleWaitState(distance);
                break;

            case State.Berserk:
                HandleBerserkState(distance);
                break;

            case State.Stun:
                HandleStunState();
                break;
        }

        UpdateAnimation();
    }

    void HandleChaseState(float distance)
    {
        // Если игрок в зоне стрельбы - начинаем стрелять
        if (distance <= shootingRange)
        {
            SetState(State.Shooting);
            return;
        }

        // Двигаемся к игроку
        agent.isStopped = false;
        agent.SetDestination(playerTarget.position);

        // Поворачиваемся в направлении движения
        if (agent.velocity.magnitude > 0.1f)
        {
            Vector3 direction = agent.velocity.normalized;
            direction.y = 0;
            if (direction != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation,
                    Time.deltaTime * rotationSpeed);
            }
        }
    }

    void HandleShootingState(float distance)
    {
        // Останавливаем движение
        agent.isStopped = true;

        // Поворачиваемся к игроку
        RotateTowardsPlayer();

        // Стреляем если можем
        if (canShoot && distance <= shootingRange)
        {
            StartCoroutine(ShootSequence());
        }

        // Если игрок убежал - преследуем
        if (distance > shootingRange + 3f)
        {
            SetState(State.Chase);
        }

        // Если выстрелили достаточно - отходим
        if (shotsFired >= maxShotsBeforeRetreat)
        {
            StartRetreat();
        }
    }

    void HandleRetreatState(float distance)
    {
        // Во время отхода поворачиваемся к игроку
        RotateTowardsPlayer();

        // Проверяем, достигли ли точки отхода
        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            // Достигли точки отхода - начинаем ожидание
            SetState(State.Wait);
            waitTimer = 0f;
            agent.isStopped = true;
            Debug.Log("Робот достиг точки отхода. Ожидание 10 секунд.");
        }

        // Можем стрелять во время отхода (опционально)
        if (canShoot && distance <= shootingRange)
        {
            StartCoroutine(RetreatShootSequence());
        }
    }

    void HandleWaitState(float distance)
    {
        // Стоим на месте и поворачиваемся к игроку
        RotateTowardsPlayer();

        // Увеличиваем таймер ожидания
        waitTimer += Time.deltaTime;

        // Если время ожидания прошло, сбрасываем счетчик выстрелов и снова преследуем
        if (waitTimer >= waitAfterRetreatTime)
        {
            shotsFired = 0;
            SetState(State.Chase);
            agent.isStopped = false;
            Debug.Log("Ожидание завершено. Возвращаюсь к преследованию.");
        }
    }

    void HandleBerserkState(float distance)
    {
        // В режиме бешенства постоянно отходим от игрока и непрерывно стреляем

        // Отходим от игрока
        Vector3 retreatDir = (transform.position - playerTarget.position).normalized;
        Vector3 retreatPos = transform.position + retreatDir * berserkRetreatDistance;

        // Ищем валидную позицию на навмеше
        NavMeshHit hit;
        if (NavMesh.SamplePosition(retreatPos, out hit, 10f, NavMesh.AllAreas))
        {
            agent.isStopped = false;
            agent.SetDestination(hit.position);
        }

        // Поворачиваемся к игроку
        RotateTowardsPlayer();

        // Непрерывно стреляем
        if (canShoot)
        {
            StartCoroutine(BerserkShootSequence());
        }
    }

    void HandleStunState()
    {
        // В состоянии оглушения стоим на месте
        agent.isStopped = true;

        // Не двигаемся и не стреляем
        // Анимация оглушения управляется через IsStunned
    }

    IEnumerator ShootSequence()
    {
        canShoot = false;

        // Включаем анимацию стрельбы
        if (animator != null)
        {
            animator.SetBool("IsShooting", true);
        }

        // Выстрел
        if (shooter != null) shooter.Shoot();
        shotsFired++;

        // Ждем завершения анимации
        yield return new WaitForSeconds(0.5f);

        // Выключаем анимацию стрельбы
        if (animator != null)
        {
            animator.SetBool("IsShooting", false);
        }

        // Ждем между выстрелами
        float waitTime = isBerserk ? 0.1f : timeBetweenShots;
        yield return new WaitForSeconds(waitTime);

        canShoot = true;
    }

    IEnumerator RetreatShootSequence()
    {
        canShoot = false;

        // Включаем анимацию стрельбы при отходе
        if (animator != null)
        {
            animator.SetBool("IsShooting", true);
        }

        // Выстрел
        if (shooter != null) shooter.Shoot();

        // Ждем завершения анимации
        yield return new WaitForSeconds(0.5f);

        // Выключаем анимацию стрельбы
        if (animator != null)
        {
            animator.SetBool("IsShooting", false);
        }

        // В режиме отхода стреляем реже
        yield return new WaitForSeconds(1.5f);

        canShoot = true;
    }

    IEnumerator BerserkShootSequence()
    {
        canShoot = false;

        // Включаем анимацию стрельбы
        if (animator != null)
        {
            animator.SetBool("IsShooting", true);
        }

        // Выстрел
        if (shooter != null) shooter.Shoot();

        // В режиме бешенства стреляем очень часто, но делаем небольшую паузу для анимации
        yield return new WaitForSeconds(0.3f);

        // Выключаем анимацию стрельбы на короткое время
        if (animator != null)
        {
            animator.SetBool("IsShooting", false);
        }

        // Очень короткая пауза между выстрелами в режиме бешенства
        yield return new WaitForSeconds(0.1f);

        canShoot = true;
    }

    void StartRetreat()
    {
        SetState(State.Retreat);

        // Отходим назад от игрока
        Vector3 retreatDir = (transform.position - playerTarget.position).normalized;
        Vector3 retreatPos = transform.position + retreatDir * retreatDistance;

        // Ищем валидную позицию на навмеше
        NavMeshHit hit;
        if (NavMesh.SamplePosition(retreatPos, out hit, 10f, NavMesh.AllAreas))
        {
            agent.isStopped = false;
            agent.SetDestination(hit.position);
            Debug.Log($"Робот отступает на {retreatDistance} единиц!");
        }
        else
        {
            // Если не нашли - отходим на меньшее расстояние
            agent.SetDestination(transform.position + retreatDir * 10f);
        }
    }

    void RotateTowardsPlayer()
    {
        if (playerTarget == null) return;

        Vector3 direction = playerTarget.position - transform.position;
        direction.y = 0;

        if (direction != Vector3.zero)
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
        Debug.Log($"Новое состояние: {currentState}");

        // Сбрасываем анимации при смене состояния (кроме оглушения)
        if (animator != null && newState != State.Stun)
        {
            animator.SetBool("IsWalking", false);
            animator.SetBool("IsShooting", false);
            animator.SetBool("IsStunned", false);
        }
    }

    void UpdateAnimation()
    {
        if (animator == null) return;

        // Определяем движение по скорости агента
        bool isMoving = agent.velocity.magnitude > 0.1f;

        switch (currentState)
        {
            case State.Chase:
                animator.SetBool("IsWalking", isMoving);
                break;

            case State.Retreat:
            case State.Berserk:
                animator.SetBool("IsWalking", isMoving);
                break;

            case State.Shooting:
                // Анимация стрельбы управляется в корутинах
                break;

            case State.Wait:
            case State.Idle:
                animator.SetBool("IsWalking", false);
                animator.SetBool("IsStunned", false);
                break;

            case State.Stun:
                // Анимация оглушения уже включена
                break;
        }
    }
}