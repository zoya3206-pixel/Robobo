using UnityEngine;

public class EnemyShooter : MonoBehaviour
{
    [Header("Настройки стрельбы")]
    public GameObject bulletPrefab;
    public Transform firePoint;
    public float bulletSpeed = 20f;

    [Header("Урон (баланс для 5-минутного боя)")]
    public float normalDamage = 5f;       // Маленький урон для долгого боя
    public float berserkDamage = 5f;      // В бешенстве урон ещё меньше (но стреляет чаще)

    [Header("Время жизни пуль")]
    public float normalBulletLife = 3f;
    public float berserkBulletLife = 0.5f;

    private bool isBerserk = false;

    public void SetBerserkMode(bool berserk)
    {
        isBerserk = berserk;
        Debug.Log($"Режим бешенства: {berserk}, Урон: {(berserk ? berserkDamage : normalDamage)}");
    }

    public void Shoot()
    {
        if (bulletPrefab == null || firePoint == null)
        {
            Debug.LogError("Не назначены префаб пули или точка выстрела!");
            return;
        }

        // Создаем пулю
        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);

        // Задаем скорость
        Rigidbody rb = bullet.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.velocity = firePoint.forward * bulletSpeed;
        }
        else
        {
            Debug.LogError("У пули нет Rigidbody!");
            return;
        }

        // Назначаем урон (в зависимости от режима)
        EnemyBullet bulletScript = bullet.GetComponent<EnemyBullet>();
        if (bulletScript != null)
        {
            float damage = isBerserk ? berserkDamage : normalDamage;
            bulletScript.damage = damage;

            // Можно добавить дебаг
            if (isBerserk && Random.value < 0.1f) // 10% шанс логгирования в бешенстве
            {
                Debug.Log($"Бешеная пуля! Урон: {damage}");
            }
        }
        else
        {
            Debug.LogError("У пули нет скрипта EnemyBullet!");
        }

        // Уничтожаем в зависимости от режима
        float lifeTime = isBerserk ? berserkBulletLife : normalBulletLife;
        Destroy(bullet, lifeTime);
    }
}