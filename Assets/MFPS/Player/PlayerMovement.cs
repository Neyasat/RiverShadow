using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float speed = 5f; // �������� �������� ������
    public float mouseSensitivity = 100f; // ���������������� ����
    public bool invertMouseY = false; // ����� ��� �������� ���������� �� ��� Y
    public float jumpForce = 5f; // ���� ������
    public LayerMask groundLayer; // ���� ����� ��� ����������� ������� �����������

    private Rigidbody rb; // ���������� ��� �������� ���������� Rigidbody
    private float verticalLookRotation = 0f; // �������� ������ �� ���������
    private bool isGrounded; // ���������� ��� ��������, ��������� �� ����� �� �����

    void Start()
    {
        // ������������� ���������� Rigidbody
        rb = GetComponent<Rigidbody>();

        // �������� ������� Rigidbody
        if (rb == null)
        {
            Debug.LogError("��������� Rigidbody �� ������ �� ������� " + gameObject.name);
        }

        // ��������� �������� �� ���� X � Z
        rb.freezeRotation = true;

        // ���������� ������� ���� � ������ ������
        Cursor.lockState = CursorLockMode.Locked;

        // ��������� ��������� ���������� ������
        verticalLookRotation = 0f;

        if (Camera.main != null)
        {
            Camera.main.transform.rotation = Quaternion.Euler(0f, transform.eulerAngles.y, 0f);
        }
        else
        {
            Debug.LogError("Main Camera �� �������!");
        }
    }

    void Update()
    {
        // ���������� ��������� ������ �� ��� X � Y
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        transform.Rotate(Vector3.up * mouseX);

        // �������� ��� Y, ���� ��� ��������
        if (invertMouseY)
        {
            verticalLookRotation += mouseY;
        }
        else
        {
            verticalLookRotation -= mouseY;
        }

        verticalLookRotation = Mathf.Clamp(verticalLookRotation, -90f, 90f);

        // �������� ������� ������ � ��������
        if (Camera.main != null)
        {
            Camera.main.transform.localRotation = Quaternion.Euler(verticalLookRotation, 0f, 0f);
        }
        else
        {
            Debug.LogError("Main Camera �� �������!");
        }

        // ���������� ��������� ������
        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");

        Vector3 move = transform.right * moveX + transform.forward * moveZ;

        // ����������� ������ � ������ ������
        rb.MovePosition(rb.position + move * speed * Time.deltaTime);

        // ��������, ��������� �� ����� �� �����
        isGrounded = Physics.CheckSphere(transform.position, 0.1f, groundLayer);

        // ������
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
    }
}
