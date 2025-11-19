using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class ControllerHandManager : MonoBehaviour
{
    [Header("Настройки руки")]
    public GameObject handModel;
    public bool alwaysShowHand = true;

    private XRController xrController;
    private ActionBasedController actionController;

    void Start()
    {
        xrController = GetComponent<XRController>();
        actionController = GetComponent<ActionBasedController>();

        // Отключаем автоматическое скрытие контроллера
        if (xrController != null)
        {
            xrController.hideControllerModel = true;
        }

        // Настраиваем руку
        if (handModel != null)
        {
            SetupHandModel();
        }
        else
        {
            // Автоматически находим руку среди дочерних объектов
            FindAndSetupHand();
        }
    }

    void Update()
    {
        // Постоянно следим чтобы рука была видима
        if (alwaysShowHand && handModel != null && !handModel.activeInHierarchy)
        {
            handModel.SetActive(true);
        }
    }

    void SetupHandModel()
    {
        // Делаем руку дочерней этому контроллеру если еще не является
        if (handModel.transform.parent != transform)
        {
            handModel.transform.SetParent(transform);
            handModel.transform.localPosition = Vector3.zero;
            handModel.transform.localRotation = Quaternion.identity;
        }

        handModel.SetActive(true);

        // Включаем все рендереры
        Renderer[] renderers = handModel.GetComponentsInChildren<Renderer>(true);
        foreach (Renderer renderer in renderers)
        {
            renderer.enabled = true;
        }
    }

    void FindAndSetupHand()
    {
        // Ищем руку по имени среди дочерних объектов
        string handName = gameObject.name.Contains("Left") ? "LH" : "RH";
        Transform handTransform = transform.Find(handName);

        if (handTransform != null)
        {
            handModel = handTransform.gameObject;
            SetupHandModel();
        }
        else
        {
            Debug.LogWarning($"Не найдена модель руки для {gameObject.name}");
        }
    }
}