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
        if (Time.time - spawnTime > lifeTime)
        {
            Destroy(gameObject);
        }
    }
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy") || other.CompareTag("EnemyBullet"))
            return;
        if (other.CompareTag("Player"))
        {
            PlayerHealth player = other.GetComponent<PlayerHealth>();
            if (player != null)
            {
                player.TakeDamage(damage);
            }
        }
        Destroy(gameObject);
    }
    public void SetLifeTime(float time)
    {
        lifeTime = time;
    }
}