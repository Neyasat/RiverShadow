using UnityEngine;

public class Target : MonoBehaviour
{
    public float health = 50f; // Здоровье объекта

    // Метод для получения урона
    public void TakeDamage(float amount)
    {
        health -= amount;
        if (health <= 0f)
        {
            Die();
        }
    }

    // Метод уничтожения объекта
    void Die()
    {
        Destroy(gameObject); // Уничтожение объекта
    }
}
