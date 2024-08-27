using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float speed = 5f; // Скорость движения игрока
    public float mouseSensitivity = 100f; // Чувствительность мыши
    public bool invertMouseY = false; // Опция для инверсии управления по оси Y
    public float jumpForce = 5f; // Сила прыжка
    public LayerMask groundLayer; // Слой земли для определения касания поверхности

    private Rigidbody rb; // Переменная для хранения компонента Rigidbody
    private float verticalLookRotation = 0f; // Вращение камеры по вертикали
    private bool isGrounded; // Переменная для проверки, находится ли игрок на земле

    void Start()
    {
        // Инициализация компонента Rigidbody
        rb = GetComponent<Rigidbody>();

        // Проверка наличия Rigidbody
        if (rb == null)
        {
            Debug.LogError("Компонент Rigidbody не найден на объекте " + gameObject.name);
        }

        // Заморозка вращения по осям X и Z
        rb.freezeRotation = true;

        // Блокировка курсора мыши в центре экрана
        Cursor.lockState = CursorLockMode.Locked;

        // Установка начальной ориентации камеры
        verticalLookRotation = 0f;

        if (Camera.main != null)
        {
            Camera.main.transform.rotation = Quaternion.Euler(0f, transform.eulerAngles.y, 0f);
        }
        else
        {
            Debug.LogError("Main Camera не найдена!");
        }
    }

    void Update()
    {
        // Управление вращением камеры по оси X и Y
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        transform.Rotate(Vector3.up * mouseX);

        // Инверсия оси Y, если она включена
        if (invertMouseY)
        {
            verticalLookRotation += mouseY;
        }
        else
        {
            verticalLookRotation -= mouseY;
        }

        verticalLookRotation = Mathf.Clamp(verticalLookRotation, -90f, 90f);

        // Проверка наличия камеры и вращение
        if (Camera.main != null)
        {
            Camera.main.transform.localRotation = Quaternion.Euler(verticalLookRotation, 0f, 0f);
        }
        else
        {
            Debug.LogError("Main Camera не найдена!");
        }

        // Управление движением игрока
        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");

        Vector3 move = transform.right * moveX + transform.forward * moveZ;

        // Перемещение игрока с учетом физики
        rb.MovePosition(rb.position + move * speed * Time.deltaTime);

        // Проверка, находится ли игрок на земле
        isGrounded = Physics.CheckSphere(transform.position, 0.1f, groundLayer);

        // Прыжок
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
    }
}
