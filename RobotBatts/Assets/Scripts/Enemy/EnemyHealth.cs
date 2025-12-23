using UnityEngine;
using UnityEngine.AI;

public class EnemyHealth : MonoBehaviour
{
    [SerializeField] private float maxHealth = 10000f;
    [SerializeField] private float damageReductionMultiplier = 0.8f;

    private float currentHealth;
    private Animator animator;
    private bool isDead = false;
    private bool isStunned = false;

    void Start()
    {
        currentHealth = maxHealth;
        animator = GetComponentInChildren<Animator>();
    }

    public void TakeDamage(float damage)
    {
        if (isDead || isStunned) return;

        float healthPercentage = currentHealth / maxHealth;
        float damageMultiplier = Mathf.Lerp(damageReductionMultiplier, 1f, healthPercentage);
        float actualDamage = damage * damageMultiplier;

        currentHealth -= actualDamage;

        if (animator != null && actualDamage > maxHealth * 0.02f)
        {
            animator.SetTrigger("TakeDamage");
        }

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public void SetStunned(bool stunned)
    {
        isStunned = stunned;
    }

    public bool IsStunned()
    {
        return isStunned;
    }

    void Die()
    {
        isDead = true;

        if (animator != null)
        {
            animator.SetBool("IsDead", true);
            animator.SetBool("IsWalking", false);
            animator.SetBool("IsShooting", false);
            animator.SetBool("IsStunned", false);
        }

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

        GameManager gameManager = FindObjectOfType<GameManager>();
        if (gameManager != null)
        {
            gameManager.WinGame();
        }
    }

    public void ResetEnemy()
    {
        currentHealth = maxHealth;
        isDead = false;
        isStunned = false;

        if (animator != null)
        {
            animator.SetBool("IsDead", false);
            animator.SetBool("IsWalking", false);
            animator.SetBool("IsShooting", false);
            animator.SetBool("IsStunned", false);
        }

        var controller = GetComponent<EnemyController>();
        if (controller != null) controller.enabled = true;

        var agent = GetComponent<NavMeshAgent>();
        if (agent != null)
        {
            agent.enabled = true;
            agent.isStopped = false;
        }

        var shooter = GetComponent<EnemyShooter>();
        if (shooter != null) shooter.enabled = true;

        var collider = GetComponent<Collider>();
        if (collider != null) collider.enabled = true;

        var rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.useGravity = true;
        }
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