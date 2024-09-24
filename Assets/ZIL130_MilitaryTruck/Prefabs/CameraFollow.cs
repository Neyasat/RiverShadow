using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target; // Цель, за которой следует камера
    public Vector3 offset; // Смещение от цели
    public float positionSmoothTime = 0.3f; // Время сглаживания позиции
    public float rotationSmoothTime = 0.3f; // Время сглаживания вращения

    private Vector3 velocity = Vector3.zero;

    void LateUpdate()
    {
        if (target == null)
            return;

        // Плавно перемещаем камеру к целевой позиции
        Vector3 targetPosition = target.position + target.TransformDirection(offset);
        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, positionSmoothTime);

        // Плавно вращаем камеру в направлении цели
        Quaternion targetRotation = Quaternion.LookRotation(target.position - transform.position);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSmoothTime * Time.deltaTime);
    }
}
