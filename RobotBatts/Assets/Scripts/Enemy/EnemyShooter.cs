using UnityEngine;

public class EnemyShooter : MonoBehaviour
{
    [Header("Настройки стрельбы")]
    public GameObject bulletPrefab; // Изменил на public
    public Transform firePoint;     // Изменил на public
    public float bulletSpeed = 25f;

    [Header("Урон (баланс для 5-минутного боя)")]
    public float normalDamage = 5f;
    public float berserkDamage = 5f;

    [Header("Время жизни пуль")]
    public float normalBulletLife = 5f;
    public float berserkBulletLife = 3f;

    private bool isBerserk = false;

    void Start()
    {
        // Проверка назначения компонентов при старте
        if (bulletPrefab == null)
        {
            Debug.LogError($"Не назначен префаб пули на {gameObject.name}! Пожалуйста, назначьте префаб пули в инспекторе.", this);
        }

        if (firePoint == null)
        {
            Debug.LogError($"Не назначена точка выстрела на {gameObject.name}! Пожалуйста, назначьте Transform точки выстрела.", this);
        }
    }

    public void SetBerserkMode(bool berserk)
    {
        isBerserk = berserk;
        Debug.Log($"Режим бешенства: {berserk}, Урон: {(berserk ? berserkDamage : normalDamage)}");
    }

    public void Shoot()
    {
        // Проверяем наличие необходимых компонентов
        if (bulletPrefab == null)
        {
            Debug.LogError($"Не назначен префаб пули на {gameObject.name}! Пожалуйста, назначьте префаб пули в инспекторе.", this);
            return;
        }

        if (firePoint == null)
        {
            Debug.LogError($"Не назначена точка выстрела на {gameObject.name}! Пожалуйста, назначьте Transform точки выстрела.", this);
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
            Debug.LogError($"У префаба пули {bulletPrefab.name} нет компонента Rigidbody! Добавьте Rigidbody к префабу пули.", this);
            Destroy(bullet);
            return;
        }

        // Назначаем урон
        EnemyBullet bulletScript = bullet.GetComponent<EnemyBullet>();
        if (bulletScript != null)
        {
            float damage = isBerserk ? berserkDamage : normalDamage;
            bulletScript.damage = damage;

            // Назначаем время жизни пули
            bulletScript.SetLifeTime(isBerserk ? berserkBulletLife : normalBulletLife);

            // Дебаг
            if (isBerserk && Random.value < 0.1f)
            {
                Debug.Log($"Бешеная пуля! Урон: {damage}, Время жизни: {(isBerserk ? berserkBulletLife : normalBulletLife)}с");
            }
        }
        else
        {
            Debug.LogError($"У префаба пули {bulletPrefab.name} нет скрипта EnemyBullet! Добавьте компонент EnemyBullet к префабу пули.", this);
        }
    }
}