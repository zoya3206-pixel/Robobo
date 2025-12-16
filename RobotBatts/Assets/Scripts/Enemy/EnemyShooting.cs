using UnityEngine;
using System.Collections;

public class EnemyShooting : MonoBehaviour
{
    [Header("Shooting Settings")]
    [SerializeField] private Transform firePoint;
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private float normalFireRate = 1f;
    [SerializeField] private float berserkFireRate = 0.2f;
    [SerializeField] private float bulletSpeed = 20f;
    [SerializeField] private float bulletDamage = 10f;

    [Header("References")]
    [SerializeField] private Transform player;

    private float nextFireTime = 0f;
    private bool isBerserk = false;
    private bool isShooting = false;

    void Start()
    {
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
                player = playerObj.transform;
        }
    }

    public void StartShooting(bool berserkMode)
    {
        isShooting = true;
        isBerserk = berserkMode;
    }

    public void StopShooting()
    {
        isShooting = false;
    }

    void Update()
    {
        if (!isShooting || player == null) return;

        // Стреляем с заданной скорострельностью
        if (Time.time >= nextFireTime)
        {
            Shoot();
            float fireRate = isBerserk ? berserkFireRate : normalFireRate;
            nextFireTime = Time.time + fireRate;
        }
    }

    private void Shoot()
    {
        if (firePoint == null || bulletPrefab == null || player == null) return;

        // Создаём пулю
        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);

        // Направляем пулю к игроку
        Vector3 direction = (player.position - firePoint.position).normalized;

        Rigidbody rb = bullet.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.velocity = direction * bulletSpeed;
        }

        // Настраиваем урон пули
        Bullet bulletScript = bullet.GetComponent<Bullet>();
        if (bulletScript == null)
        {
            bulletScript = bullet.AddComponent<Bullet>();
        }
        bulletScript.SetDamage(bulletDamage);

        // Уничтожаем пулю через 3 секунды
        Destroy(bullet, 3f);

        Debug.Log("Враг выстрелил!");
    }
}

// Дополнительный скрипт для пули
public class Bullet : MonoBehaviour
{
    private float damage = 10f;

    public void SetDamage(float dmg)
    {
        damage = dmg;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Наносим урон игроку
            PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damage);
            }

            Destroy(gameObject);
        }
        else if (!other.CompareTag("Enemy") && !other.isTrigger)
        {
            Destroy(gameObject);
        }
    }
}