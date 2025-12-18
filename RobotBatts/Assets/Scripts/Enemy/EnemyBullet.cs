using UnityEngine;

public class EnemyBullet : MonoBehaviour
{
    public float damage = 10f;

    void OnTriggerEnter(Collider other)
    {
        // Не попадать во врагов
        if (other.CompareTag("Enemy")) return;

        // Попадание в игрока
        if (other.CompareTag("Player"))
        {
            PlayerHealth player = other.GetComponent<PlayerHealth>();
            if (player != null)
            {
                player.TakeDamage(damage);
            }
        }

        // Уничтожаем пулю при любом столкновении
        Destroy(gameObject);
    }
}