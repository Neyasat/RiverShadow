using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class Zombie : MonoBehaviour
{
    // Параметры здоровья
    public float health = 50f;
    public GameObject deathEffect;
    public float destroyDelay = 2f;
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
    private CapsuleCollider capsuleCollider; // Ссылка на капсульный коллайдер
    private BoxCollider boxCollider; // Новый BoxCollider для использования после смерти
    private float lastAttackTime;

    // Параметры конечностей
    [Header("Сегменты конечностей")]

    // Голова
    [Header("Голова")]
    public GameObject head;          // Голова зомби
    public Collider headCollider;    // Коллайдер головы

    private bool headRemoved = false;

    // Левая рука
    public GameObject leftUpperArm;    // Левое плечо
    public GameObject leftForearm;     // Левое предплечье
    public GameObject leftHand;        // Левая кисть

    // Правая рука
    public GameObject rightUpperArm;   // Правое плечо
    public GameObject rightForearm;    // Правое предплечье
    public GameObject rightHand;       // Правая кисть

    // Левая нога
    public GameObject leftThigh;       // Левое бедро
    public GameObject leftCalf;        // Левая голень
    public GameObject leftFoot;        // Левая ступня

    // Правая нога
    public GameObject rightThigh;      // Правое бедро
    public GameObject rightCalf;       // Правая голень
    public GameObject rightFoot;       // Правая ступня

    [Header("Коллайдеры конечностей")]

    public Collider leftArmCollider;
    public Collider rightArmCollider;
    public Collider leftLegCollider;
    public Collider rightLegCollider;

    private bool leftArmRemoved = false;
    private bool rightArmRemoved = false;
    private bool leftLegRemoved = false;
    private bool rightLegRemoved = false;

    [Header("Здоровье конечностей")]
    public float headHealth = 20f;
    public float leftArmHealth = 15f;
    public float rightArmHealth = 15f;
    public float leftLegHealth = 20f;
    public float rightLegHealth = 20f;

    void Start()
    {
        animator = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();
        rb = GetComponent<Rigidbody>();
        capsuleCollider = GetComponent<CapsuleCollider>(); // Получаем компонент CapsuleCollider

        // Добавляем `BoxCollider`, но пока что его отключаем
        boxCollider = gameObject.AddComponent<BoxCollider>();
        boxCollider.enabled = false;
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

        // Проверяем, была ли поражена конечность
        bool limbHit = CheckLimbHit(hitCollider, amount);

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

    bool CheckLimbHit(Collider hitCollider, float damage)
    {
        if (hitCollider == headCollider && !headRemoved)
        {
            headHealth -= damage;
            if (headHealth <= 0f)
            {
                RemoveLimb("Head");
                headRemoved = true;
            }
            return true;
        }
        else if (hitCollider == leftArmCollider && !leftArmRemoved)
        {
            leftArmHealth -= damage;
            if (leftArmHealth <= 0f)
            {
                RemoveLimb("LeftArm");
                leftArmRemoved = true;
            }
            return true;
        }
        else if (hitCollider == rightArmCollider && !rightArmRemoved)
        {
            rightArmHealth -= damage;
            if (rightArmHealth <= 0f)
            {
                RemoveLimb("RightArm");
                rightArmRemoved = true;
            }
            return true;
        }
        else if (hitCollider == leftLegCollider && !leftLegRemoved)
        {
            leftLegHealth -= damage;
            if (leftLegHealth <= 0f)
            {
                RemoveLimb("LeftLeg");
                leftLegRemoved = true;
                UpdateMovementAfterLegLoss();
            }
            return true;
        }
        else if (hitCollider == rightLegCollider && !rightLegRemoved)
        {
            rightLegHealth -= damage;
            if (rightLegHealth <= 0f)
            {
                RemoveLimb("RightLeg");
                rightLegRemoved = true;
                UpdateMovementAfterLegLoss();
            }
            return true;
        }

        return false; // Не попали в конечность
    }

    void RemoveLimb(string limbName)
    {
        switch (limbName)
        {
            case "Head":
                // Отключаем голову
                head.SetActive(false);

                // Отключаем коллайдер
                headCollider.enabled = false;

                // Зомби умирает при потере головы
                Die();
                break;

            case "LeftArm":
                // Отключаем сегменты левой руки
                leftUpperArm.SetActive(false);
                leftForearm.SetActive(false);
                leftHand.SetActive(false);

                // Отключаем коллайдер
                leftArmCollider.enabled = false;
                break;

            case "RightArm":
                // Отключаем сегменты правой руки
                rightUpperArm.SetActive(false);
                rightForearm.SetActive(false);
                rightHand.SetActive(false);

                // Отключаем коллайдер
                rightArmCollider.enabled = false;
                break;

            case "LeftLeg":
                // Отключаем сегменты левой ноги
                leftThigh.SetActive(false);
                leftCalf.SetActive(false);
                leftFoot.SetActive(false);

                // Отключаем коллайдер
                leftLegCollider.enabled = false;
                break;

            case "RightLeg":
                // Отключаем сегменты правой ноги
                rightThigh.SetActive(false);
                rightCalf.SetActive(false);
                rightFoot.SetActive(false);

                // Отключаем коллайдер
                rightLegCollider.enabled = false;
                break;
        }

        // Добавляем дополнительные эффекты при отстреле конечности
        PlayLimbRemovalEffects(limbName);
    }

    void UpdateMovementAfterLegLoss()
    {
        // Если зомби потерял одну ногу
        if ((leftLegRemoved && !rightLegRemoved) || (!leftLegRemoved && rightLegRemoved))
        {
            agent.speed *= 0.5f;
        }
        // Если зомби потерял обе ноги
        else if (leftLegRemoved && rightLegRemoved)
        {
            agent.speed *= 0.2f;
            animator.SetBool("isCrawling", true);
        }
    }

    void PlayLimbRemovalEffects(string limbName)
    {
        // Реализуйте ваши эффекты здесь
        // Например:
        // Instantiate(bloodEffect, transform.position, Quaternion.identity);
        // AudioSource.PlayClipAtPoint(limbRemovalSound, transform.position);
    }

    void Attack()
    {
        agent.isStopped = true;
        agent.velocity = Vector3.zero;
        animator.SetBool("isAttacking", true);
        lastAttackTime = Time.time;
        StartCoroutine(StopMovementDuringAttack());
    }

    void Die()
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

        // Отключаем капсульный коллайдер и включаем коробочный
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

    public void OnAttackEnd()
    {
        animator.SetBool("isAttacking", false);
        if (!isDead)
        {
            agent.isStopped = false;
        }
    }
}
