using UnityEngine;

public class VehicleSwitch : MonoBehaviour
{
    public GameObject player; // ������ �� ������
    public GameObject vehicle; // ������ �� ����������
    public Transform driverSeat; // ������� �������� ������ ����������
    public Transform vehicleCameraPosition; // ������� ������ ��� ���������� �����������

    private bool isPlayerInVehicle = false; // ����, ��������� �� ����� � ����������
    private VehicleFree vehicleController; // ������ �� ������ ���������� �����������
    private PlayerMovement playerMovement; // ������ ���������� �������
    private GunController gunController; // ������ ������ ������
    private Camera playerCamera; // ������ ������
    private Rigidbody playerRigidbody; // Rigidbody ������
    private Vector3 originalCameraPosition; // �������� ������� ������
    private Quaternion originalCameraRotation; // �������� ������� ������

    void Start()
    {
        // �������� ������ �� ����������
        vehicleController = vehicle.GetComponent<VehicleFree>();
        playerMovement = player.GetComponent<PlayerMovement>();
        gunController = player.GetComponentInChildren<GunController>();
        playerCamera = player.GetComponentInChildren<Camera>();
        playerRigidbody = player.GetComponent<Rigidbody>();

        // ��������� �������� ������� � ������� ������
        originalCameraPosition = playerCamera.transform.localPosition;
        originalCameraRotation = playerCamera.transform.localRotation;

        // ��������� ���������� ����������� ��� ������
        if (vehicleController != null)
            vehicleController.enabled = false;
    }

    void Update()
    {
        if (!isPlayerInVehicle && Input.GetKeyDown(KeyCode.F))
        {
            // ���������, ��������� �� ����� ����� � �����������
            float distance = Vector3.Distance(player.transform.position, vehicle.transform.position);
            if (distance <= 3f) // ������ ��������������
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

        // ��������� ������� ���������� �������
        if (playerMovement != null)
            playerMovement.enabled = false;

        if (gunController != null)
            gunController.enabled = false;

        // ������ Rigidbody ������ ��������������
        if (playerRigidbody != null)
            playerRigidbody.isKinematic = true;

        // �� ��������� ��������� ������
        // Collider playerCollider = player.GetComponent<Collider>();
        // if (playerCollider != null)
        //     playerCollider.enabled = false;

        // ���������� ������ �� ����� ��������
        player.transform.position = driverSeat.position;
        player.transform.rotation = driverSeat.rotation;
        // �� ������������� ������������ �����
        // player.transform.SetParent(vehicle.transform);

        // �������� ���������� �����������
        if (vehicleController != null)
            vehicleController.enabled = true;

        // ����������� ������ ��� ����������
        if (vehicleCameraPosition != null)
        {
            playerCamera.transform.position = vehicleCameraPosition.position;
            playerCamera.transform.rotation = vehicleCameraPosition.rotation;
            // ��� ������������� ������������� ������ ��� �������� ����������
            playerCamera.transform.SetParent(vehicle.transform);
        }

        // �������� ������ ������
        Renderer[] renderers = player.GetComponentsInChildren<Renderer>();
        foreach (Renderer rend in renderers)
        {
            rend.enabled = false;
        }

        // ���������� ��������
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void ExitVehicle()
    {
        isPlayerInVehicle = false;

        // ��������� ���������� �����������
        if (vehicleController != null)
            vehicleController.enabled = false;

        // ��������������� ������
        playerCamera.transform.SetParent(player.transform);
        playerCamera.transform.localPosition = originalCameraPosition;
        playerCamera.transform.localRotation = originalCameraRotation;

        // ����������� ������ �� ���������� (���� ������������� ������������ �����)
        // player.transform.SetParent(null);

        // ���������� ������ ����� � �����������
        player.transform.position = driverSeat.position + vehicle.transform.right * 2f;
        player.transform.rotation = Quaternion.identity;

        // �������� ������� ���������� �������
        if (playerMovement != null)
            playerMovement.enabled = true;

        if (gunController != null)
            gunController.enabled = true;

        // �������� Rigidbody ������
        if (playerRigidbody != null)
            playerRigidbody.isKinematic = false;

        // �������� ��������� ������ (���� ���������)
        // Collider playerCollider = player.GetComponent<Collider>();
        // if (playerCollider != null)
        //     playerCollider.enabled = true;

        // ���������� ������ ������
        Renderer[] renderers = player.GetComponentsInChildren<Renderer>();
        foreach (Renderer rend in renderers)
        {
            rend.enabled = true;
        }

        // ���������� ��������
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}
