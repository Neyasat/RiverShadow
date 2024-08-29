using UnityEngine;

public class Target : MonoBehaviour
{
    public float health = 50f; // Здоровье объекта
    public float impactForce = 500f; // Сила удара пули

    private Rigidbody rb;

    void Start()
    {
        // Получаем компонент Rigidbody для последующего взаимодействия
        rb = GetComponent<Rigidbody>();
    }

    // Метод для получения урона и направления удара
    public void TakeDamage(float amount, Vector3 hitPoint, Vector3 hitDirection, Vector3 hitNormal)
    {
        health -= amount; // Уменьшаем здоровье на значение урона
        if (health <= 0f) // Если здоровье опускается до 0 или ниже
        {
            Die(); // Вызываем метод уничтожения объекта
        }
        else
        {
            ApplyForce(hitPoint, hitDirection, hitNormal); // Применяем силу к объекту в точке попадания
        }
    }

    // Применение силы к объекту в направлении попадания
    void ApplyForce(Vector3 hitPoint, Vector3 hitDirection, Vector3 hitNormal)
    {
        if (rb != null)
        {
            // Применяем силу в точке попадания, смешивая направление удара и нормаль поверхности
            Vector3 forceDirection = (hitDirection + hitNormal).normalized; // Смешиваем направление пули и нормаль
            rb.AddForceAtPosition(forceDirection * impactForce, hitPoint, ForceMode.Impulse);
        }
    }

    // Метод уничтожения объекта
    void Die()
    {
        Destroy(gameObject); // Уничтожаем объект
    }
}
