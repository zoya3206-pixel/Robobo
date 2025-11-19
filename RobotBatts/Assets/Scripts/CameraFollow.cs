using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public GameObject Follower;
    public GameObject RotationFollower;
    public Camera MainCamera;
    public float rotationSmoothSpeed = 2f;

    public float smoothSpeed = 17f; // Скорость плавного следования

    private void Update()
    {
        if (Follower != null && MainCamera != null)
        {
            Vector3 targetPosition = new Vector3(
                MainCamera.transform.position.x,
                MainCamera.transform.position.y + 3.83f,
                MainCamera.transform.position.z
            );

            // Плавное перемещение
            Follower.transform.position = Vector3.Lerp(
                Follower.transform.position,
                targetPosition,
                smoothSpeed
            );

            // Получаем текущий и целевой поворот
            //Quaternion currentRotation = RotationFollower.transform.rotation;
            //Quaternion targetRotation = Quaternion.Euler(
            // 0,
            // MainCamera.transform.eulerAngles.y,
            // 0
            //);

            // Плавное вращение
            //RotationFollower.transform.rotation = Quaternion.Lerp(
            //currentRotation,
            //targetRotation,
            //smoothSpeed/10 * Time.deltaTime

            Vector3 cameraEuler = MainCamera.transform.eulerAngles;
            Quaternion targetRotation = Quaternion.Euler(0, cameraEuler.y, 0);

            Follower.transform.rotation = Quaternion.Lerp(
                Follower.transform.rotation,
                targetRotation,
                rotationSmoothSpeed * Time.deltaTime
            );
        }
    }
}
