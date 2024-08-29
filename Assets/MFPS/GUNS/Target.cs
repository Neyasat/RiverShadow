using UnityEngine;

public class Target : MonoBehaviour
{
    public float health = 50f; // �������� �������
    public float impactForce = 500f; // ���� ����� ����

    private Rigidbody rb;

    void Start()
    {
        // �������� ��������� Rigidbody ��� ������������ ��������������
        rb = GetComponent<Rigidbody>();
    }

    // ����� ��� ��������� ����� � ����������� �����
    public void TakeDamage(float amount, Vector3 hitPoint, Vector3 hitDirection, Vector3 hitNormal)
    {
        health -= amount; // ��������� �������� �� �������� �����
        if (health <= 0f) // ���� �������� ���������� �� 0 ��� ����
        {
            Die(); // �������� ����� ����������� �������
        }
        else
        {
            ApplyForce(hitPoint, hitDirection, hitNormal); // ��������� ���� � ������� � ����� ���������
        }
    }

    // ���������� ���� � ������� � ����������� ���������
    void ApplyForce(Vector3 hitPoint, Vector3 hitDirection, Vector3 hitNormal)
    {
        if (rb != null)
        {
            // ��������� ���� � ����� ���������, �������� ����������� ����� � ������� �����������
            Vector3 forceDirection = (hitDirection + hitNormal).normalized; // ��������� ����������� ���� � �������
            rb.AddForceAtPosition(forceDirection * impactForce, hitPoint, ForceMode.Impulse);
        }
    }

    // ����� ����������� �������
    void Die()
    {
        Destroy(gameObject); // ���������� ������
    }
}
