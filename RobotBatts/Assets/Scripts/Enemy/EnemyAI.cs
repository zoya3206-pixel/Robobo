using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class EnemyAI : MonoBehaviour
{
    [Header("Ссылки")]
    [SerializeField] private Transform playerTarget;
    private NavMeshAgent agent;
    private Animator animator;
    private EnemyShooter shooter;
    private EnemyHealth enemyHealth;

    [Header("Настройки AI")]
    [SerializeField] private float startDelay = 5f;
    [SerializeField] private float shootingRange = 15f;
    [SerializeField] private float timeBetweenShots = 2f;
    [SerializeField] private float berserkStartTime = 60f;
    [SerializeField] private float berserkDuration = 20f;
    [SerializeField] private float restDuration = 30f;

    private bool isShooting = false;
    private bool isBerserk = false;
    private bool canShoot = true; // Может ли робот стрелять
    private bool gameStarted = false;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        shooter = GetComponent<EnemyShooter>();
        enemyHealth = GetComponent<EnemyHealth>();

        if (playerTarget == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null) playerTarget = playerObj.transform;
        }

        StartCoroutine(InitialDelay());
        StartCoroutine(BerserkTimer());
    }

    IEnumerator InitialDelay()
    {
        Debug.Log("Робот загрузился, стоит 5 секунд.");
        agent.isStopped = true;
        SetIdleState();

        yield return new WaitForSeconds(startDelay);
        gameStarted = true;
        agent.isStopped = false;
        Debug.Log("Робот активирован!");
    }

    IEnumerator BerserkTimer()
    {
        // Ждём 60 секунд до начала бешенства
        yield return new WaitForSeconds(berserkStartTime);

        // Начинаем бешенство
        StartBerserkMode();

        // Ждём 20 секунд бешенства
        yield return new WaitForSeconds(berserkDuration);

        // Останавливаем бешенство
        EndBerserkMode();

        // Ждём 30 секунд отдыха
        yield return new WaitForSeconds(restDuration);

        // Возвращаемся к обычному режиму
        ReturnToNormalMode();
    }

    void StartBerserkMode()
    {
        isBerserk = true;
        canShoot = true;
        shooter.SetBerserkMode(true);
        Debug.Log("БЕШЕНСТВО АКТИВИРОВАНО! 20 секунд непрерывного огня!");
    }

    void EndBerserkMode()
    {
        isBerserk = false;
        canShoot = false; // Не может стрелять во время отдыха
        shooter.SetBerserkMode(false);
        Debug.Log("Робот устал. 30 секунд отдыха (не стреляет).");
    }

    void ReturnToNormalMode()
    {
        canShoot = true; // Снова может стрелять
        Debug.Log("Робот отдохнул. Возврат к обычному режиму.");
    }

    void Update()
    {
        // Проверяем условия для обновления
        if (!gameStarted || playerTarget == null ||
            (enemyHealth != null && enemyHealth.IsDead()))
            return;

        float distanceToPlayer = Vector3.Distance(transform.position, playerTarget.position);

        // Логика перемещения
        if (distanceToPlayer <= shootingRange && distanceToPlayer > agent.stoppingDistance)
        {
            MoveToPlayer();
        }
        else if (distanceToPlayer <= agent.stoppingDistance)
        {
            StopMoving();
            // Стреляем только если можем
            if (!isShooting && canShoot)
            {
                StartCoroutine(ShootingRoutine());
            }
        }
        else
        {
            MoveToPlayer();
        }

        // Обновляем анимацию ходьбы
        UpdateWalkAnimation();
    }

    void SetIdleState()
    {
        if (animator != null)
        {
            animator.SetBool("IsWalking", false);
            animator.SetBool("IsShooting", false);
        }
    }

    void UpdateWalkAnimation()
    {
        if (animator != null && !isShooting)
        {
            bool isMoving = agent.velocity.magnitude > 0.1f && agent.isStopped == false;
            animator.SetBool("IsWalking", isMoving);
        }
    }

    void MoveToPlayer()
    {
        agent.isStopped = false;
        agent.SetDestination(playerTarget.position);
    }

    void StopMoving()
    {
        agent.isStopped = true;
        if (animator != null)
        {
            animator.SetBool("IsWalking", false);
        }
    }

    IEnumerator ShootingRoutine()
    {
        isShooting = true;

        while (Vector3.Distance(transform.position, playerTarget.position) <= agent.stoppingDistance &&
               canShoot && // Добавляем проверку canShoot
               (enemyHealth == null || !enemyHealth.IsDead()))
        {
            // Поворачиваемся к игроку
            Vector3 direction = (playerTarget.position - transform.position).normalized;
            direction.y = 0;
            if (direction != Vector3.zero)
            {
                Quaternion lookRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 10f);
            }

            // Включаем анимацию стрельбы
            animator.SetBool("IsShooting", true);
            animator.SetBool("IsWalking", false);

            // Стреляем
            if (shooter != null) shooter.Shoot();

            // Ждём между выстрелами в зависимости от режима
            if (isBerserk)
            {
                // Бешенство: стреляем очень быстро
                yield return new WaitForSeconds(0.1f);
            }
            else
            {
                // Обычный режим: даём игроку время для атаки
                yield return new WaitForSeconds(0.5f);
                animator.SetBool("IsShooting", false);
                yield return new WaitForSeconds(timeBetweenShots - 0.5f);
            }
        }

        // Заканчиваем стрельбу
        isShooting = false;
        animator.SetBool("IsShooting", false);
    }
}