using UnityEngine;
using UnityEngine.AI;

public class EnemyHealth : MonoBehaviour
{
    [Header("Настройки здоровья")]
    [SerializeField] private float maxHealth = 100f;
    private float currentHealth;

    private Animator animator;
    private EnemyAI enemyAI;
    private bool isDead = false;

    void Start()
    {
        currentHealth = maxHealth;
        animator = GetComponent<Animator>();
        enemyAI = GetComponent<EnemyAI>();

        if (animator == null) Debug.LogError("Animator не найден!");
    }

    public void TakeDamage(float damageAmount)
    {
        if (currentHealth <= 0 || isDead) return;

        currentHealth -= damageAmount;
        Debug.Log("Робот получил урон! Осталось здоровья: " + currentHealth);

        // Проигрываем анимацию получения урона
        animator.SetTrigger("TakeDamage");

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        if (isDead) return;
        isDead = true;

        Debug.Log("Робот уничтожен!");
        animator.SetBool("IsDead", true);

        // Отключаем AI и компоненты
        if (enemyAI != null)
        {
            enemyAI.enabled = false;
            enemyAI.StopAllCoroutines();
        }

        NavMeshAgent agent = GetComponent<NavMeshAgent>();
        if (agent != null) agent.enabled = false;

        CapsuleCollider collider = GetComponent<CapsuleCollider>();
        if (collider != null) collider.enabled = false;

        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
        }

        // Отключаем стрельбу
        EnemyShooter shooter = GetComponent<EnemyShooter>();
        if (shooter != null) shooter.enabled = false;

        // ВАЖНО: НИКАКОГО Destroy() НЕ ДОЛЖНО БЫТЬ!
        Debug.Log("Робот остался на сцене для анимации смерти");
    }

    public bool IsDead()
    {
        return isDead;
    }
}