using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [SerializeField] private float maxHealth = 200f;
    private float currentHealth;

    [SerializeField] private Renderer cockpitGlassRenderer;
    [SerializeField] private Material[] glassMaterials;

    [SerializeField] private float crackStage1Health = 150f;
    [SerializeField] private float crackStage2Health = 100f;
    [SerializeField] private float crackStage3Health = 50f;

    private int currentCrackStage = 0;
    private bool isDead = false;

    void Start()
    {
        currentHealth = maxHealth;
        UpdateGlassMaterial(0);
    }

    public void TakeDamage(float damage)
    {
        if (isDead) return;

        currentHealth -= damage;
        UpdateGlassCracks();

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void UpdateGlassCracks()
    {
        int newCrackStage = CalculateCrackStage();

        if (newCrackStage != currentCrackStage)
        {
            currentCrackStage = newCrackStage;
            UpdateGlassMaterial(currentCrackStage);
        }
    }

    private int CalculateCrackStage()
    {
        if (currentHealth <= 0) return 3;
        if (currentHealth <= crackStage3Health) return 3;
        if (currentHealth <= crackStage2Health) return 2;
        if (currentHealth <= crackStage1Health) return 1;
        return 0;
    }

    private void UpdateGlassMaterial(int crackStage)
    {
        if (cockpitGlassRenderer == null || glassMaterials == null || glassMaterials.Length < 4)
            return;

        if (crackStage >= 0 && crackStage < glassMaterials.Length && glassMaterials[crackStage] != null)
        {
            cockpitGlassRenderer.material = glassMaterials[crackStage];
        }
    }

    private void Die()
    {
        if (isDead) return;

        isDead = true;
        UpdateGlassMaterial(3);

        GameManager gameManager = FindObjectOfType<GameManager>();
        if (gameManager != null)
        {
            gameManager.PlayerDied();
        }
    }

    public void ResetHealthAndGlass()
    {
        currentHealth = maxHealth;
        isDead = false;
        currentCrackStage = 0;
        UpdateGlassMaterial(0);
    }

    public float GetCurrentHealth()
    {
        return currentHealth;
    }
}