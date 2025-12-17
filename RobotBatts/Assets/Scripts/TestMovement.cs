using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class TestMovement : MonoBehaviour
{
    public Transform playerTarget;
    private NavMeshAgent agent;
    private Animator animator;

    void Start()
    {
        Debug.Log("=== ТЕСТ НАЧАЛСЯ ===");

        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();

        Debug.Log($"Agent: {agent != null}");
        Debug.Log($"Animator: {animator != null}");
        Debug.Log($"PlayerTarget: {playerTarget != null}");

        if (playerTarget == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerTarget = player.transform;
                Debug.Log($"Нашел игрока: {playerTarget.name}");
            }
        }

        StartCoroutine(MovementTest());
    }

    IEnumerator MovementTest()
    {
        Debug.Log("1. Стою 2 секунды...");
        yield return new WaitForSeconds(2f);

        Debug.Log("2. Включаю анимацию ходьбы...");
        animator.SetBool("IsWalking", true);

        Debug.Log("3. Иду к игроку...");
        if (agent != null && playerTarget != null)
        {
            agent.SetDestination(playerTarget.position);
            Debug.Log($"Цель: {playerTarget.position}");
        }

        yield return new WaitForSeconds(3f);

        Debug.Log("4. Стреляю...");
        animator.SetBool("IsWalking", false);
        animator.SetBool("IsShooting", true);

        yield return new WaitForSeconds(2f);

        Debug.Log("5. Возвращаюсь в idle...");
        animator.SetBool("IsShooting", false);

        Debug.Log("=== ТЕСТ ЗАВЕРШЕН ===");
    }

    void Update()
    {
        if (agent != null)
        {
            Debug.Log($"Скорость: {agent.velocity.magnitude}, Путь: {agent.hasPath}, Осталось: {agent.remainingDistance}");
        }
    }
}