using UnityEngine;

public class PlayerHandTrigger : MonoBehaviour
{
    // Ссылка на контроллер руки робота
    public RobotArmPunch robotArmController; // Замените RobotArmPunch на корректное имя класса

    // Определяет, левая это рука или правая
    public bool isLeftHand;

    // Пример метода, который может быть вызван при столкновении
    private void OnTriggerEnter(Collider other)
    {
        // Проверяем, что контроллер установлен
        if (robotArmController != null)
        {
            // Вызываем метод удара
            robotArmController.TriggerPunch(isLeftHand);
        }
    }
}