using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public GameObject Follower;
    public GameObject RotationFollower;
    public Camera MainCamera;
    public float rotationSmoothSpeed = 2f;

    public float smoothSpeed = 17f;

    private void Update()
    {
        if (Follower != null && MainCamera != null)
        {
            Vector3 targetPosition = new Vector3(
                MainCamera.transform.position.x,
                MainCamera.transform.position.y + 7.81f,
                MainCamera.transform.position.z
            );
            Follower.transform.position = Vector3.Lerp(
                Follower.transform.position,
                targetPosition,
                smoothSpeed
            );
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
