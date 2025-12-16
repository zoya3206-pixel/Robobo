using UnityEngine;
using System.Collections;

public class EnemyShooting : MonoBehaviour
{
    [SerializeField] private Transform firePoint;
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private float normalFireRate = 1f;
    [SerializeField] private float berserkFireRate = 0.2f;

    private float nextFireTime = 0f;
    private bool isBerserk = false;

    void Update()
    {
        // Стрельба управляется из EnemyAI через SetState
    }

    public void StartShooting(bool berserkMode)
    {
        isBerserk = berserkMode;

        if (Time.time >= nextFireTime)
        {
            Shoot();
            nextFireTime = Time.time + (isBerserk ? berserkFireRate : normalFireRate);
        }
    }

    private void Shoot()
    {
        if (firePoint == null || bulletPrefab == null) return;

        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
        // Настройка пули...
    }

    public void StopShooting()
    {
        nextFireTime = Time.time;
    }
}