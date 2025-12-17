using UnityEngine;

public class EnemyBullet : MonoBehaviour
{
    private float damage;
    private GameObject owner;

    public void SetDamage(float dmg)
    {
        damage = dmg;
        // Находим врага в сцене
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        if (enemies.Length > 0)
        {
            owner = enemies[0]; // Берём первого найденного врага
        }
    }

    void OnTriggerEnter(Collider other)
    {
        // Не попадать во врагов
        if (other.CompareTag("Enemy"))
        {
            return;
        }

        // Если попали в игрока
        if (other.CompareTag("Player"))
        {
            PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damage);
            }
            Destroy(gameObject);
        }
        else
        {
            // Уничтожаем при столкновении с другими объектами
            Destroy(gameObject);
        }
    }
}