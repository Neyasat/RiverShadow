using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;

public class Zombie : MonoBehaviour
{
    // ��������� ��������
    public float health = 50f;
    public GameObject deathEffect;
    public float destroyDelay = 2f;
    [HideInInspector]
    public bool isDead = false;

    // ��������� ����� � ��������
    public Transform player;
    public float attackRange = 2.0f;
    public float attackCooldown = 1.5f;
    public float hitStopDuration = 0.5f;
    public float attackStopDuration = 1f;

    // ��������� `BoxCollider` ����� ������
    public Vector3 deathColliderSize = new Vector3(1f, 0.5f, 2f); // ������ `BoxCollider`
    public Vector3 deathColliderCenter = new Vector3(0, 0.25f, 0); // ����� `BoxCollider`

    // ��������� ��� ������� �����
    public float spacing = 2.0f; // ����������� ���������� ����� �����

    // ��������� ����������
    private Animator animator;
    private NavMeshAgent agent;
    private Rigidbody rb;
    private CapsuleCollider capsuleCollider; // ������ �� `CapsuleCollider`
    private BoxCollider boxCollider; // ����� `BoxCollider` ��� ������������� ����� ������
    private float lastAttackTime;

    // ������ �� ������ LimbManager
    private LimbManager limbManager;

    void Start()
    {
        animator = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();
        rb = GetComponent<Rigidbody>();
        capsuleCollider = GetComponent<CapsuleCollider>(); // �������� ��������� `CapsuleCollider`

        // ��������� `BoxCollider`, �� ���� ��� ��� ���������
        boxCollider = gameObject.AddComponent<BoxCollider>();
        boxCollider.enabled = false;

        // �������� ������ �� LimbManager
        limbManager = GetComponent<LimbManager>();
    }

    void Update()
    {
        if (isDead) return; // ���� ����� �����, �� ��������� ���������� ��������

        // ������������ ���������� ����� �����
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

    // ����� ��� ����������� ���������� ����� �����
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

    // ����� ��� ��������� ����� �����
    public void TakeDamage(float amount, Collider hitCollider)
    {
        if (isDead) return;

        // ���������, ���� �� �������� ���������� ����� LimbManager
        bool limbHit = limbManager.CheckLimbHit(hitCollider, amount);

        if (!limbHit)
        {
            // ���� �� ������ � ����������, �������� ���� �� ������ ��������
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

    // ����� ���������� LimbManager ��� ���������� �������� ����� ������ ����
    public void UpdateMovementAfterLegLoss()
    {
        // ���� ����� ������� ���� ����
        if ((limbManager.leftLegRemoved && !limbManager.rightLegRemoved) || (!limbManager.leftLegRemoved && limbManager.rightLegRemoved))
        {
            agent.speed *= 0.5f;
        }
        // ���� ����� ������� ��� ����
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
            StartCoroutine(DisableAnimatorAfterDelay()); // ��������� �������� ����� ��������
        }

        agent.isStopped = true;
        agent.velocity = Vector3.zero;
        agent.enabled = false;

        // ��������� ���������� � �������� ������ ��� ����������� �������
        rb.isKinematic = false;
        rb.useGravity = true;

        // ��������� `CapsuleCollider` � �������� `BoxCollider`
        capsuleCollider.enabled = false;
        boxCollider.size = deathColliderSize;
        boxCollider.center = deathColliderCenter;
        boxCollider.enabled = true; // �������� `BoxCollider`

        StartCoroutine(DisableRigidbodyWithDelay());
    }

    IEnumerator DisableAnimatorAfterDelay()
    {
        yield return new WaitForSeconds(7.0f); // �������� 7 ������, ����� �������� ������ ������ �����������
        animator.enabled = false; // ��������� `Animator`
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
