using UnityEngine;

public class EnemyBullet : MonoBehaviour
{
    public float damage = 10f;
    private float lifeTime = 3f;
    private float spawnTime;

    void Start()
    {
        spawnTime = Time.time;
    }

    void Update()
    {
        // Автоматическое уничтожение через заданное время
        if (Time.time - spawnTime > lifeTime)
        {
            Destroy(gameObject);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        // Не попадать во врагов и в самого себя
        if (other.CompareTag("Enemy") || other.CompareTag("EnemyBullet"))
            return;

        // Попадание в игрока
        if (other.CompareTag("Player"))
        {
            PlayerHealth player = other.GetComponent<PlayerHealth>();
            if (player != null)
            {
                player.TakeDamage(damage);
            }
        }

        // Уничтожаем пулю при столкновении
        Destroy(gameObject);
    }

    // Метод для установки времени жизни из EnemyShooter
    public void SetLifeTime(float time)
    {
        lifeTime = time;
    }
}