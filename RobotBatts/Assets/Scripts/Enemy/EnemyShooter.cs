using UnityEngine;

public class EnemyShooter : MonoBehaviour
{
    [Header("Настройки стрельбы")]
    public GameObject bulletPrefab; 
    public Transform firePoint; 
    public float bulletSpeed = 25f;

    [Header("Урон")]
    public float normalDamage = 8f;
    public float berserkDamage = 12f;

    [Header("Время жизни пуль")]
    public float normalBulletLife = 5f;
    public float berserkBulletLife = 3f;

    private bool isBerserk = false;

    public void SetBerserkMode(bool berserk)
    {
        isBerserk = berserk;
    }

    public void Shoot()
    {
        if (bulletPrefab == null)
        {
            return;
        }

        if (firePoint == null)
        {
            return;
        }

        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);

        Rigidbody rb = bullet.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.velocity = firePoint.forward * bulletSpeed;
        }
        else
        {
            Destroy(bullet);
            return;
        }

        EnemyBullet bulletScript = bullet.GetComponent<EnemyBullet>();
        if (bulletScript != null)
        {
            float damage = isBerserk ? berserkDamage : normalDamage;
            bulletScript.damage = damage;

            bulletScript.SetLifeTime(isBerserk ? berserkBulletLife : normalBulletLife);
        }
    }
}