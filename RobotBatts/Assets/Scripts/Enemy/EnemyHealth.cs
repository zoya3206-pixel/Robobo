using UnityEngine;
using UnityEngine.AI;

public class EnemyHealth : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float currentHealth;

    [Header("Components")]
    [SerializeField] private Animator animator;
    [SerializeField] private EnemyAI enemyAI;

    void Start()
    {
        currentHealth = maxHealth;

        if (animator == null)
            animator = GetComponent<Animator>();

        if (enemyAI == null)
            enemyAI = GetComponent<EnemyAI>();
    }

    public void TakeDamage(float damage)
    {
        if (currentHealth <= 0) return;

        currentHealth -= damage;

        // Анимация получения урона
        if (animator != null)
            animator.SetTrigger("TakeDamage");

        Debug.Log($"Враг получил {damage} урона. Осталось HP: {currentHealth}");

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        Debug.Log("Враг умер!");

        // Отключаем AI
        if (enemyAI != null)
            enemyAI.SetDead(true);

        // Включаем анимацию смерти
        if (animator != null)
        {
            animator.SetBool("IsDead", true);
            animator.SetBool("IsShooting", false);
            animator.SetBool("IsStunned", false);
        }

        // Отключаем компоненты
        NavMeshAgent agent = GetComponent<NavMeshAgent>();
        if (agent != null)
            agent.isStopped = true;

        Collider collider = GetComponent<Collider>();
        if (collider != null)
            collider.enabled = false;

        // Отключаем этот скрипт
        enabled = false;

        // Уничтожаем объект через 5 секунд
        Destroy(gameObject, 5f);
    }

    public float GetHealthPercentage()
    {
        return currentHealth / maxHealth;
    }
}