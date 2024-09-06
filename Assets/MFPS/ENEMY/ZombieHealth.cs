using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class ZombieHealth : MonoBehaviour
{
    public float health = 50f;
    public GameObject deathEffect;
    public float destroyDelay = 2f; // �������� ����� ������������ �������
    public bool isDead = false; // ����, �������������� ��������� ������

    private Animator animator; // ������ �� ��������� Animator
    private NavMeshAgent agent; // ������ �� NavMeshAgent
    private Rigidbody rb; // ������ �� Rigidbody

    public float hitStopDuration = 0.5f; // ����� ��������� ����� ��� ��������� �����
    public float attackStopDuration = 1f; // ����� ��������� ����� ��� �����

    void Start()
    {
        animator = GetComponent<Animator>(); // �������� ��������� Animator
        agent = GetComponent<NavMeshAgent>(); // �������� ��������� NavMeshAgent
        rb = GetComponent<Rigidbody>(); // �������� ��������� Rigidbody
    }

    // �����, ������� ����� ���������� ��� ��������� ����� �����
    public void TakeDamage(float amount)
    {
        if (isDead) return; // ���� ����� ��� �����, ������ �� ������������ ����

        health -= amount;
        if (health <= 0f)
        {
            Die();
        }
        else
        {
            // ������������� �������� ��������� �����
            animator.SetTrigger("TakeDamageTrigger");

            // ������������� ����� �� ����� �������� ��������� �����
            StartCoroutine(StopMovementDuringHit());
        }
    }

    // ����� ��� ��������� ����� �����
    public void Attack()
    {
        if (isDead) return; // ���� ����� �����, �� �� ����� ���������

        // ������������� �������� �����
        animator.SetTrigger("AttackTrigger");

        // ������������� ����� �� ����� �����
        StartCoroutine(StopMovementDuringAttack());
    }

    void Die()
    {
        isDead = true; // ������������� ���� ������

        // ���� ���� ������ ������, ������� ���
        if (deathEffect != null)
        {
            Instantiate(deathEffect, transform.position, Quaternion.identity);
        }

        // ����������� �������� ������, ���� ���� Animator
        if (animator != null)
        {
            animator.SetTrigger("Die");
        }

        // ������������� ����� � ��� ��������
        Zombie zombie = GetComponent<Zombie>();
        if (zombie != null)
        {
            zombie.enabled = false; // ��������� ������ Zombie
        }

        if (agent != null)
        {
            agent.isStopped = true; // ������������� �������� �����
            agent.velocity = Vector3.zero; // ���������� �������� ������
            agent.enabled = false; // ��������� NavMeshAgent
        }

        // ��������� �������� ��� ���������� ������ � ���������
        StartCoroutine(DisableRigidbodyWithDelay());
    }

    // ��������� ��� ���������� Rigidbody � ���������
    IEnumerator DisableRigidbodyWithDelay()
    {
        yield return new WaitForSeconds(1.0f); // ����, ���� ����� ������ �� �����

        // ��������� Rigidbody
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        // ���������� ������ ����� �������� �����
        Destroy(gameObject, destroyDelay);
    }

    // ��������� ��� ��������� �������� �� ����� �������� ��������� �����
    IEnumerator StopMovementDuringHit()
    {
        if (agent != null)
        {
            agent.isStopped = true; // ������������� ����� �� ����� ��������
            agent.velocity = Vector3.zero; // ���������� �������� ������
        }

        if (rb != null)
        {
            rb.velocity = Vector3.zero; // ���������� �������� Rigidbody
        }

        // ���� ��������� �����, ���� ����� ��������� � ��������� ��������� �����
        yield return new WaitForSeconds(hitStopDuration);

        if (agent != null && !isDead)
        {
            agent.isStopped = false; // ������������ ��������, ���� ����� �� �����
        }
    }

    // ��������� ��� ��������� �������� �� ����� �������� �����
    IEnumerator StopMovementDuringAttack()
    {
        if (agent != null)
        {
            agent.isStopped = true; // ������������� ����� �� ����� �����
            agent.velocity = Vector3.zero; // ���������� �������� ������
        }

        if (rb != null)
        {
            rb.velocity = Vector3.zero; // ���������� �������� Rigidbody
        }

        // ���� ��������� �����, ���� ����� ��������� �����
        yield return new WaitForSeconds(attackStopDuration);

        if (agent != null && !isDead)
        {
            agent.isStopped = false; // ������������ ��������, ���� ����� �� �����
        }
    }
}
