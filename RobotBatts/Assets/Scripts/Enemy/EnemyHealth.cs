using UnityEngine;
using UnityEngine.AI;

public class EnemyHealth : MonoBehaviour
{
    [Header("Здоровье (баланс для 5-минутного боя)")]
    [SerializeField] private float maxHealth = 1000f; // Больше здоровья для долгого боя

    private float currentHealth;
    private Animator animator;
    private bool isDead = false;

    void Start()
    {
        currentHealth = maxHealth;

        // Ищем аниматор в дочерних объектах
        animator = GetComponentInChildren<Animator>();

        if (animator == null)
        {
            Debug.LogError("Animator не найден!");
        }

        Debug.Log($"Робот: здоровье {currentHealth}/{maxHealth}");
    }

    public void TakeDamage(float damage)
    {
        if (isDead) return;

        currentHealth -= damage;
        Debug.Log($"Робот получил {damage} урона. Осталось: {currentHealth}");

        // Анимация получения урона (только если урон значительный)
        if (animator != null && damage > maxHealth * 0.05f) // 5% от макс. здоровья
        {
            animator.SetTrigger("TakeDamage");
        }

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        isDead = true;
        Debug.Log("Робот уничтожен!");

        // Анимация смерти
        if (animator != null)
        {
            animator.SetBool("IsDead", true);
            animator.SetBool("IsWalking", false);
            animator.SetBool("IsShooting", false);
        }

        // Отключаем всё
        var controller = GetComponent<EnemyController>();
        if (controller != null) controller.enabled = false;

        var agent = GetComponent<NavMeshAgent>();
        if (agent != null) agent.enabled = false;

        var shooter = GetComponent<EnemyShooter>();
        if (shooter != null) shooter.enabled = false;

        var collider = GetComponent<Collider>();
        if (collider != null) collider.enabled = false;

        var rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
        }

        // Через 5 секунд можно уничтожить или оставить для анимации
        // Destroy(gameObject, 5f);
    }

    public bool IsDead()
    {
        return isDead;
    }

    public float GetHealthPercentage()
    {
        return currentHealth / maxHealth;
    }
}