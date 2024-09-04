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
        }

        // ��������� Rigidbody
        if (rb != null)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true;
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
}
