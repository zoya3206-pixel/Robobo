using UnityEngine;

public class TestEnemyAnimations : MonoBehaviour
{
    void Update()
    {
        // Тестовая логика для проверки анимаций
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            GetComponent<Animator>().SetBool("IsWalking", true);
            Debug.Log("Анимация ходьбы включена");
        }

        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            GetComponent<Animator>().SetBool("IsWalking", false);
            Debug.Log("Анимация ходьбы выключена");
        }

        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            GetComponent<Animator>().SetBool("IsShooting", true);
            Debug.Log("Анимация стрельбы включена");
        }

        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            GetComponent<Animator>().SetBool("IsShooting", false);
            Debug.Log("Анимация стрельбы выключена");
        }

        if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            GetComponent<Animator>().SetTrigger("TakeDamage");
            Debug.Log("Анимация получения урона запущена");
        }
    }
}