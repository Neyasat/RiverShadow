using UnityEngine;
using UnityEngine.AI;

public class ZombieHealth : MonoBehaviour
{
    public float health = 100f; // �������� �����

    private Animator animator;
    private bool isDead = false;

    void Start()
    {
        animator = GetComponent<Animator>(); // �������� ��������� Animator
    }

    // ����� ��� ��������� �����
    public void TakeDamage(float damage)
    {
        if (isDead) return; // ���� ����� ��� �����, ������ �� ������

        health -= damage;

        if (health <= 0f)
        {
            Die();
        }
    }

    // ����� ��� ������ �����
    void Die()
    {
        isDead = true;
        animator.SetTrigger("Die"); // ��������� �������� ������
        GetComponent<Collider>().enabled = false; // ��������� ���������
        GetComponent<NavMeshAgent>().enabled = false; // ��������� ��������
        GetComponent<ZombieAI>().enabled = false; // ��������� ������ ��������
    }
}
