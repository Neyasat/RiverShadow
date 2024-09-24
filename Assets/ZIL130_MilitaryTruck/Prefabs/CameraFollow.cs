using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target; // ����, �� ������� ������� ������
    public Vector3 offset; // �������� �� ����
    public float positionSmoothTime = 0.3f; // ����� ����������� �������
    public float rotationSmoothTime = 0.3f; // ����� ����������� ��������

    private Vector3 velocity = Vector3.zero;

    void LateUpdate()
    {
        if (target == null)
            return;

        // ������ ���������� ������ � ������� �������
        Vector3 targetPosition = target.position + target.TransformDirection(offset);
        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, positionSmoothTime);

        // ������ ������� ������ � ����������� ����
        Quaternion targetRotation = Quaternion.LookRotation(target.position - transform.position);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSmoothTime * Time.deltaTime);
    }
}
