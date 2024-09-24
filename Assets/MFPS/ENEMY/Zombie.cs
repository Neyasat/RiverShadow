using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;

public class Zombie : MonoBehaviour
{
    // Параметры здоровья
    public float health = 50f;
    public GameObject deathEffect;
    public float destroyDelay = 2f;
    [HideInInspector]
    public bool isDead = false;

    // Параметры атаки и движения
    public Transform player;
    public float attackRange = 2.0f;
    public float attackCooldown = 1.5f;
    public float hitStopDuration = 0.5f;
    public float attackStopDuration = 1f;

    // Параметры `BoxCollider` после смерти
    public Vector3 deathColliderSize = new Vector3(1f, 0.5f, 2f); // Размер `BoxCollider`
    public Vector3 deathColliderCenter = new Vector3(0, 0.25f, 0); // Центр `BoxCollider`

    // Параметры для позиции зомби
    public float spacing = 2.0f; // Минимальное расстояние между зомби

    // Приватные компоненты
    private Animator animator;
    private NavMeshAgent agent;
    private Rigidbody rb;
    private CapsuleCollider capsuleCollider; // Ссылка на `CapsuleCollider`
    private BoxCollider boxCollider; // Новый `BoxCollider` для использования после смерти
    private float lastAttackTime;

    // Ссылка на скрипт LimbManager
    private LimbManager limbManager;

    void Start()
    {
        animator = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();
        rb = GetComponent<Rigidbody>();
        capsuleCollider = GetComponent<CapsuleCollider>(); // Получаем компонент `CapsuleCollider`

        // Добавляем `BoxCollider`, но пока что его отключаем
        boxCollider = gameObject.AddComponent<BoxCollider>();
        boxCollider.enabled = false;

        // Получаем ссылку на LimbManager
        limbManager = GetComponent<LimbManager>();
    }

    void Update()
    {
        if (isDead) return; // Если зомби мертв, не выполняем дальнейшие действия

        // Поддерживаем расстояние между зомби
        MaintainSpacing();

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (distanceToPlayer <= attackRange)
        {
            agent.isStopped = true;
            animator.SetBool("isRunning", false);

            if (Time.time >= lastAttackTime + attackCooldown)
            {
                Attack();
            }
        }
        else
        {
            agent.isStopped = false;
            agent.SetDestination(player.position);
            animator.SetBool("isRunning", true);
            animator.SetBool("isAttacking", false);

            float speedPercent = agent.velocity.magnitude / agent.speed;
            animator.SetFloat("MoveSpeed", speedPercent);
        }
    }

    // Метод для поддержания расстояния между зомби
    void MaintainSpacing()
    {
        Zombie[] allZombies = FindObjectsOfType<Zombie>();

        foreach (Zombie otherZombie in allZombies)
        {
            if (otherZombie != this)
            {
                float distance = Vector3.Distance(transform.position, otherZombie.transform.position);

                if (distance < spacing)
                {
                    Vector3 direction = (transform.position - otherZombie.transform.position).normalized;
                    transform.position = otherZombie.transform.position + direction * spacing;
                }
            }
        }
    }

    // Метод для нанесения урона зомби
    public void TakeDamage(float amount, Collider hitCollider)
    {
        if (isDead) return;

        // Проверяем, была ли поражена конечность через LimbManager
        bool limbHit = limbManager.CheckLimbHit(hitCollider, amount);

        if (!limbHit)
        {
            // Если не попали в конечность, отнимаем урон от общего здоровья
            health -= amount;
        }

        if (health <= 0f)
        {
            Die();
        }
        else
        {
            animator.SetTrigger("TakeDamageTrigger");
            StartCoroutine(StopMovementDuringHit());
        }
    }

    // Метод вызывается LimbManager при обновлении движения после потери ноги
    public void UpdateMovementAfterLegLoss()
    {
        // Если зомби потерял одну ногу
        if ((limbManager.leftLegRemoved && !limbManager.rightLegRemoved) || (!limbManager.leftLegRemoved && limbManager.rightLegRemoved))
        {
            agent.speed *= 0.5f;
        }
        // Если зомби потерял обе ноги
        else if (limbManager.leftLegRemoved && limbManager.rightLegRemoved)
        {
            agent.speed *= 0.2f;
            animator.SetBool("isCrawling", true);
        }
    }

    public void Die()
    {
        if (isDead) return;
        isDead = true;

        if (deathEffect != null)
        {
            Instantiate(deathEffect, transform.position, Quaternion.identity);
        }

        if (animator != null)
        {
            animator.SetTrigger("Die");
            StartCoroutine(DisableAnimatorAfterDelay()); // Отключаем анимацию после задержки
        }

        agent.isStopped = true;
        agent.velocity = Vector3.zero;
        agent.enabled = false;

        // Отключаем кинематику и включаем физику для нормального падения
        rb.isKinematic = false;
        rb.useGravity = true;

        // Отключаем `CapsuleCollider` и включаем `BoxCollider`
        capsuleCollider.enabled = false;
        boxCollider.size = deathColliderSize;
        boxCollider.center = deathColliderCenter;
        boxCollider.enabled = true; // Включаем `BoxCollider`

        StartCoroutine(DisableRigidbodyWithDelay());
    }

    IEnumerator DisableAnimatorAfterDelay()
    {
        yield return new WaitForSeconds(7.0f); // Задержка 7 секунд, чтобы анимация смерти успела проиграться
        animator.enabled = false; // Отключаем `Animator`
    }

    IEnumerator DisableRigidbodyWithDelay()
    {
        yield return new WaitForSeconds(1.0f);

        if (rb != null)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        Destroy(gameObject, destroyDelay);
    }

    IEnumerator StopMovementDuringHit()
    {
        agent.isStopped = true;
        agent.velocity = Vector3.zero;

        yield return new WaitForSeconds(hitStopDuration);

        if (!isDead)
        {
            agent.isStopped = false;
        }
    }

    IEnumerator StopMovementDuringAttack()
    {
        yield return new WaitForSeconds(attackStopDuration);

        if (!isDead)
        {
            agent.isStopped = false;
            animator.SetBool("isAttacking", false);
        }
    }

    void Attack()
    {
        agent.isStopped = true;
        agent.velocity = Vector3.zero;
        animator.SetBool("isAttacking", true);
        lastAttackTime = Time.time;
        StartCoroutine(StopMovementDuringAttack());
    }

    public void OnAttackEnd()
    {
        animator.SetBool("isAttacking", false);
        if (!isDead)
        {
            agent.isStopped = false;
        }
    }
}
