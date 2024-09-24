using UnityEngine;
using System.Collections.Generic;
using Cinemachine;

public class VehicleSwitch : MonoBehaviour
{
    public GameObject player; // Ссылка на игрока
    public GameObject vehicle; // Ссылка на автомобиль
    public Transform driverSeat; // Позиция водителя внутри автомобиля

    private bool isPlayerInVehicle = false; // Флаг, находится ли игрок в автомобиле
    private VehicleFree vehicleController; // Ссылка на скрипт управления автомобилем
    private Rigidbody vehicleRigidbody; // Rigidbody автомобиля
    private Rigidbody playerRigidbody; // Rigidbody игрока

    // Список скриптов, которые необходимо отключать при входе в автомобиль
    public List<MonoBehaviour> scriptsToDisable = new List<MonoBehaviour>();

    // Список объектов, которые необходимо скрывать при входе в автомобиль
    public List<GameObject> objectsToHide = new List<GameObject>();

    // Ссылка на камеру игрока
    public Camera playerCamera;

    // Ссылка на камеру автомобиля
    public Camera vehicleCamera;

    // Ссылка на виртуальную камеру автомобиля
    public CinemachineVirtualCamera vehicleVirtualCamera;

    void Start()
    {
        // Получаем ссылки на компоненты
        vehicleController = vehicle.GetComponent<VehicleFree>();
        vehicleRigidbody = vehicle.GetComponent<Rigidbody>();
        playerRigidbody = player.GetComponent<Rigidbody>();

        // Отключаем управление автомобилем при старте
        if (vehicleController != null)
            vehicleController.enabled = false;

        // Делаем Rigidbody автомобиля кинематическим при старте
        if (vehicleRigidbody != null)
            vehicleRigidbody.isKinematic = true;

        // Убедитесь, что камера автомобиля и виртуальная камера отключены при запуске
        if (vehicleCamera != null)
            vehicleCamera.enabled = false;

        if (vehicleVirtualCamera != null)
            vehicleVirtualCamera.gameObject.SetActive(false);
    }

    void Update()
    {
        if (!isPlayerInVehicle && Input.GetKeyDown(KeyCode.F))
        {
            // Проверяем, находится ли игрок рядом с автомобилем
            float distance = Vector3.Distance(player.transform.position, vehicle.transform.position);
            if (distance <= 3f) // Радиус взаимодействия
            {
                EnterVehicle();
            }
        }
        else if (isPlayerInVehicle && Input.GetKeyDown(KeyCode.F))
        {
            ExitVehicle();
        }
    }

    void EnterVehicle()
    {
        isPlayerInVehicle = true;

        // Отключаем скрипты управления игроком
        foreach (MonoBehaviour script in scriptsToDisable)
        {
            if (script != null)
                script.enabled = false;
        }

        // Делаем Rigidbody игрока кинематическим
        if (playerRigidbody != null)
            playerRigidbody.isKinematic = true;

        // Перемещаем игрока на место водителя
        player.transform.position = driverSeat.position;
        player.transform.rotation = driverSeat.rotation;

        // Включаем управление автомобилем
        if (vehicleController != null)
            vehicleController.enabled = true;

        // Делаем Rigidbody автомобиля динамическим
        if (vehicleRigidbody != null)
            vehicleRigidbody.isKinematic = false;

        // Отключаем камеру игрока
        if (playerCamera != null)
            playerCamera.enabled = false;

        // Включаем камеру автомобиля
        if (vehicleCamera != null)
            vehicleCamera.enabled = true;
        else
            Debug.LogWarning("Vehicle camera is not assigned.");

        // Включаем виртуальную камеру автомобиля
        if (vehicleVirtualCamera != null)
            vehicleVirtualCamera.gameObject.SetActive(true);
        else
            Debug.LogWarning("Vehicle virtual camera is not assigned.");

        // Скрываем объекты из списка
        foreach (GameObject obj in objectsToHide)
        {
            if (obj != null)
                obj.SetActive(false);
        }

        // Управление курсором
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void ExitVehicle()
    {
        isPlayerInVehicle = false;

        // Отключаем управление автомобилем
        if (vehicleController != null)
            vehicleController.enabled = false;

        // Делаем Rigidbody автомобиля кинематическим
        if (vehicleRigidbody != null)
            vehicleRigidbody.isKinematic = true;

        // Включаем камеру игрока
        if (playerCamera != null)
            playerCamera.enabled = true;

        // Отключаем камеру автомобиля
        if (vehicleCamera != null)
            vehicleCamera.enabled = false;

        // Отключаем виртуальную камеру автомобиля
        if (vehicleVirtualCamera != null)
            vehicleVirtualCamera.gameObject.SetActive(false);

        // Перемещаем игрока рядом с автомобилем
        player.transform.position = driverSeat.position + vehicle.transform.right * 2f;
        player.transform.rotation = Quaternion.identity;

        // Включаем скрипты управления игроком
        foreach (MonoBehaviour script in scriptsToDisable)
        {
            if (script != null)
                script.enabled = true;
        }

        // Включаем Rigidbody игрока
        if (playerRigidbody != null)
            playerRigidbody.isKinematic = false;

        // Показываем объекты из списка
        foreach (GameObject obj in objectsToHide)
        {
            if (obj != null)
                obj.SetActive(true);
        }

        // Управление курсором
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}
