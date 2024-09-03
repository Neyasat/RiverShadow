using UnityEngine;
using UnityEngine.AI;

public class ZombieHealth : MonoBehaviour
{
    public float health = 100f; // Здоровье зомби

    private Animator animator;
    private bool isDead = false;

    void Start()
    {
        animator = GetComponent<Animator>(); // Получаем компонент Animator
    }

    // Метод для получения урона
    public void TakeDamage(float damage)
    {
        if (isDead) return; // Если зомби уже мертв, ничего не делать

        health -= damage;

        if (health <= 0f)
        {
            Die();
        }
    }

    // Метод для смерти зомби
    void Die()
    {
        isDead = true;
        animator.SetTrigger("Die"); // Запускаем анимацию смерти
        GetComponent<Collider>().enabled = false; // Отключаем коллайдер
        GetComponent<NavMeshAgent>().enabled = false; // Отключаем движение
        GetComponent<ZombieAI>().enabled = false; // Отключаем скрипт движения
    }
}
