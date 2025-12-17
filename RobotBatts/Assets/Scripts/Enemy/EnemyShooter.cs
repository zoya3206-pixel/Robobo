using UnityEngine;

public class EnemyShooter : MonoBehaviour
{
    [Header("Настройки стрельбы")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float bulletSpeed = 20f;
    [SerializeField] private float bulletDamage = 10f;

    [Header("Время жизни пуль")]
    [SerializeField] private float normalBulletLifetime = 3f;
    [SerializeField] private float berserkBulletLifetime = 0.1f; // ОЧЕНЬ быстро исчезают в режиме бешенства

    private bool isBerserk = false;

    public void SetBerserkMode(bool berserk)
    {
        isBerserk = berserk;
        Debug.Log("Режим стрельбы: " + (berserk ? "БЕШЕНСТВО" : "НОРМАЛЬНЫЙ"));
    }

    public void Shoot()
    {
        if (bulletPrefab == null || firePoint == null)
        {
            Debug.LogError("Не присвоен префаб пули или точка выстрела!");
            return;
        }

        // Создаём пулю
        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);

        // Придаём скорость
        Rigidbody rb = bullet.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.velocity = firePoint.forward * bulletSpeed;
        }
        else
        {
            Debug.LogError("У пули нет Rigidbody!");
        }

        // Настраиваем урон пули
        EnemyBullet bulletScript = bullet.GetComponent<EnemyBullet>();
        if (bulletScript != null)
        {
            bulletScript.SetDamage(bulletDamage);
        }
        else
        {
            Debug.LogError("У префаба пули нет скрипта EnemyBullet!");
        }

        // Уничтожаем пулю в зависимости от режима
        float lifetime = isBerserk ? berserkBulletLifetime : normalBulletLifetime;
        Destroy(bullet, lifetime);

        // Опционально: для отладки
        if (isBerserk)
        {
            Debug.Log($"Пуля создана. Время жизни: {lifetime} сек (БЕШЕНСТВО)");
        }
    }
}