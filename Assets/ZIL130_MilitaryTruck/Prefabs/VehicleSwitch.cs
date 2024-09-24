using UnityEngine;
using System.Collections.Generic;
using Cinemachine;

public class VehicleSwitch : MonoBehaviour
{
    public GameObject player; // ������ �� ������
    public GameObject vehicle; // ������ �� ����������
    public Transform driverSeat; // ������� �������� ������ ����������

    private bool isPlayerInVehicle = false; // ����, ��������� �� ����� � ����������
    private VehicleFree vehicleController; // ������ �� ������ ���������� �����������
    private Rigidbody vehicleRigidbody; // Rigidbody ����������
    private Rigidbody playerRigidbody; // Rigidbody ������

    // ������ ��������, ������� ���������� ��������� ��� ����� � ����������
    public List<MonoBehaviour> scriptsToDisable = new List<MonoBehaviour>();

    // ������ ��������, ������� ���������� �������� ��� ����� � ����������
    public List<GameObject> objectsToHide = new List<GameObject>();

    // ������ �� ������ ������
    public Camera playerCamera;

    // ������ �� ������ ����������
    public Camera vehicleCamera;

    // ������ �� ����������� ������ ����������
    public CinemachineVirtualCamera vehicleVirtualCamera;

    void Start()
    {
        // �������� ������ �� ����������
        vehicleController = vehicle.GetComponent<VehicleFree>();
        vehicleRigidbody = vehicle.GetComponent<Rigidbody>();
        playerRigidbody = player.GetComponent<Rigidbody>();

        // ��������� ���������� ����������� ��� ������
        if (vehicleController != null)
            vehicleController.enabled = false;

        // ������ Rigidbody ���������� �������������� ��� ������
        if (vehicleRigidbody != null)
            vehicleRigidbody.isKinematic = true;

        // ���������, ��� ������ ���������� � ����������� ������ ��������� ��� �������
        if (vehicleCamera != null)
            vehicleCamera.enabled = false;

        if (vehicleVirtualCamera != null)
            vehicleVirtualCamera.gameObject.SetActive(false);
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
        foreach (MonoBehaviour script in scriptsToDisable)
        {
            if (script != null)
                script.enabled = false;
        }

        // ������ Rigidbody ������ ��������������
        if (playerRigidbody != null)
            playerRigidbody.isKinematic = true;

        // ���������� ������ �� ����� ��������
        player.transform.position = driverSeat.position;
        player.transform.rotation = driverSeat.rotation;

        // �������� ���������� �����������
        if (vehicleController != null)
            vehicleController.enabled = true;

        // ������ Rigidbody ���������� ������������
        if (vehicleRigidbody != null)
            vehicleRigidbody.isKinematic = false;

        // ��������� ������ ������
        if (playerCamera != null)
            playerCamera.enabled = false;

        // �������� ������ ����������
        if (vehicleCamera != null)
            vehicleCamera.enabled = true;
        else
            Debug.LogWarning("Vehicle camera is not assigned.");

        // �������� ����������� ������ ����������
        if (vehicleVirtualCamera != null)
            vehicleVirtualCamera.gameObject.SetActive(true);
        else
            Debug.LogWarning("Vehicle virtual camera is not assigned.");

        // �������� ������� �� ������
        foreach (GameObject obj in objectsToHide)
        {
            if (obj != null)
                obj.SetActive(false);
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

        // ������ Rigidbody ���������� ��������������
        if (vehicleRigidbody != null)
            vehicleRigidbody.isKinematic = true;

        // �������� ������ ������
        if (playerCamera != null)
            playerCamera.enabled = true;

        // ��������� ������ ����������
        if (vehicleCamera != null)
            vehicleCamera.enabled = false;

        // ��������� ����������� ������ ����������
        if (vehicleVirtualCamera != null)
            vehicleVirtualCamera.gameObject.SetActive(false);

        // ���������� ������ ����� � �����������
        player.transform.position = driverSeat.position + vehicle.transform.right * 2f;
        player.transform.rotation = Quaternion.identity;

        // �������� ������� ���������� �������
        foreach (MonoBehaviour script in scriptsToDisable)
        {
            if (script != null)
                script.enabled = true;
        }

        // �������� Rigidbody ������
        if (playerRigidbody != null)
            playerRigidbody.isKinematic = false;

        // ���������� ������� �� ������
        foreach (GameObject obj in objectsToHide)
        {
            if (obj != null)
                obj.SetActive(true);
        }

        // ���������� ��������
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}
