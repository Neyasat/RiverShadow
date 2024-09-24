using UnityEngine;

public class VehicleSwitch : MonoBehaviour
{
    public GameObject player; // Ссылка на игрока
    public GameObject vehicle; // Ссылка на автомобиль
    public Transform driverSeat; // Позиция водителя внутри автомобиля
    public Transform vehicleCameraPosition; // Позиция камеры при управлении автомобилем

    private bool isPlayerInVehicle = false; // Флаг, находится ли игрок в автомобиле
    private VehicleFree vehicleController; // Ссылка на скрипт управления автомобилем
    private PlayerMovement playerMovement; // Скрипт управления игроком
    private GunController gunController; // Скрипт оружия игрока
    private Camera playerCamera; // Камера игрока
    private Rigidbody playerRigidbody; // Rigidbody игрока
    private Vector3 originalCameraPosition; // Исходная позиция камеры
    private Quaternion originalCameraRotation; // Исходный поворот камеры

    void Start()
    {
        // Получаем ссылки на компоненты
        vehicleController = vehicle.GetComponent<VehicleFree>();
        playerMovement = player.GetComponent<PlayerMovement>();
        gunController = player.GetComponentInChildren<GunController>();
        playerCamera = player.GetComponentInChildren<Camera>();
        playerRigidbody = player.GetComponent<Rigidbody>();

        // Сохраняем исходную позицию и поворот камеры
        originalCameraPosition = playerCamera.transform.localPosition;
        originalCameraRotation = playerCamera.transform.localRotation;

        // Отключаем управление автомобилем при старте
        if (vehicleController != null)
            vehicleController.enabled = false;
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
        if (playerMovement != null)
            playerMovement.enabled = false;

        if (gunController != null)
            gunController.enabled = false;

        // Делаем Rigidbody игрока кинематическим
        if (playerRigidbody != null)
            playerRigidbody.isKinematic = true;

        // НЕ отключаем коллайдер игрока
        // Collider playerCollider = player.GetComponent<Collider>();
        // if (playerCollider != null)
        //     playerCollider.enabled = false;

        // Перемещаем игрока на место водителя
        player.transform.position = driverSeat.position;
        player.transform.rotation = driverSeat.rotation;
        // НЕ устанавливаем родительскую связь
        // player.transform.SetParent(vehicle.transform);

        // Включаем управление автомобилем
        if (vehicleController != null)
            vehicleController.enabled = true;

        // Настраиваем камеру для автомобиля
        if (vehicleCameraPosition != null)
        {
            playerCamera.transform.position = vehicleCameraPosition.position;
            playerCamera.transform.rotation = vehicleCameraPosition.rotation;
            // При необходимости устанавливаем камеру как дочернюю автомобилю
            playerCamera.transform.SetParent(vehicle.transform);
        }

        // Скрываем модель игрока
        Renderer[] renderers = player.GetComponentsInChildren<Renderer>();
        foreach (Renderer rend in renderers)
        {
            rend.enabled = false;
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

        // Восстанавливаем камеру
        playerCamera.transform.SetParent(player.transform);
        playerCamera.transform.localPosition = originalCameraPosition;
        playerCamera.transform.localRotation = originalCameraRotation;

        // Отсоединяем игрока от автомобиля (если устанавливали родительскую связь)
        // player.transform.SetParent(null);

        // Перемещаем игрока рядом с автомобилем
        player.transform.position = driverSeat.position + vehicle.transform.right * 2f;
        player.transform.rotation = Quaternion.identity;

        // Включаем скрипты управления игроком
        if (playerMovement != null)
            playerMovement.enabled = true;

        if (gunController != null)
            gunController.enabled = true;

        // Включаем Rigidbody игрока
        if (playerRigidbody != null)
            playerRigidbody.isKinematic = false;

        // Включаем коллайдер игрока (если отключали)
        // Collider playerCollider = player.GetComponent<Collider>();
        // if (playerCollider != null)
        //     playerCollider.enabled = true;

        // Показываем модель игрока
        Renderer[] renderers = player.GetComponentsInChildren<Renderer>();
        foreach (Renderer rend in renderers)
        {
            rend.enabled = true;
        }

        // Управление курсором
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}
