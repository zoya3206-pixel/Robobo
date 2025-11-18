using UnityEngine;

public class SimpleRobotController : MonoBehaviour
{
    public Transform xrCamera;  // Перетащите Main Camera сюда
    public Transform robot;     // Перетащите вашего робота сюда

    void Update()
    {
        if (xrCamera == null || robot == null) return;

        // Просто копируем позицию (XZ) и поворот (Y) от камеры к роботу
        Vector3 newPosition = new Vector3(
            xrCamera.position.x,
            robot.position.y,  // Сохраняем исходную высоту робота
            xrCamera.position.z
        );

        robot.position = newPosition;

        // Копируем только горизонтальный поворот
        Vector3 cameraEuler = xrCamera.eulerAngles;
        robot.rotation = Quaternion.Euler(0, cameraEuler.y, 0);
    }
}