using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class ZombieHealth : MonoBehaviour
{
    public float health = 50f;
    public GameObject deathEffect;
    public float destroyDelay = 2f; // Задержка перед уничтожением объекта
    public bool isDead = false; // Флаг, контролирующий состояние смерти

    private Animator animator; // Ссылка на компонент Animator
    private NavMeshAgent agent; // Ссылка на NavMeshAgent
    private Rigidbody rb; // Ссылка на Rigidbody

    public float hitStopDuration = 0.5f; // Время остановки зомби при получении урона

    void Start()
    {
        animator = GetComponent<Animator>(); // Получаем компонент Animator
        agent = GetComponent<NavMeshAgent>(); // Получаем компонент NavMeshAgent
        rb = GetComponent<Rigidbody>(); // Получаем компонент Rigidbody
    }

    // Метод, который будет вызываться для нанесения урона зомби
    public void TakeDamage(float amount)
    {
        if (isDead) return; // Если зомби уже мертв, больше не обрабатываем урон

        health -= amount;
        if (health <= 0f)
        {
            Die();
        }
        else
        {
            // Воспроизводим анимацию получения урона
            animator.SetTrigger("TakeDamageTrigger");

            // Останавливаем зомби на время анимации получения урона
            StartCoroutine(StopMovementDuringHit());
        }
    }

    void Die()
    {
        isDead = true; // Устанавливаем флаг смерти

        // Если есть эффект смерти, создаем его
        if (deathEffect != null)
        {
            Instantiate(deathEffect, transform.position, Quaternion.identity);
        }

        // Проигрываем анимацию смерти, если есть Animator
        if (animator != null)
        {
            animator.SetTrigger("Die");
        }

        // Останавливаем зомби и его движение
        Zombie zombie = GetComponent<Zombie>();
        if (zombie != null)
        {
            zombie.enabled = false; // Отключаем скрипт Zombie
        }

        if (agent != null)
        {
            agent.isStopped = true; // Останавливаем движение зомби
            agent.velocity = Vector3.zero; // Сбрасываем скорость агента
        }

        // Остановка Rigidbody
        if (rb != null)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true;
        }

        // Уничтожаем объект через заданное время
        Destroy(gameObject, destroyDelay);
    }

    // Коррутина для остановки движения во время анимации получения урона
    IEnumerator StopMovementDuringHit()
    {
        if (agent != null)
        {
            agent.isStopped = true; // Останавливаем агент на время анимации
            agent.velocity = Vector3.zero; // Сбрасываем скорость агента
        }

        if (rb != null)
        {
            rb.velocity = Vector3.zero; // Сбрасываем скорость Rigidbody
        }

        // Ждем указанное время, пока зомби находится в состоянии получения урона
        yield return new WaitForSeconds(hitStopDuration);

        if (agent != null && !isDead)
        {
            agent.isStopped = false; // Возобновляем движение, если зомби не мертв
        }
    }
}
