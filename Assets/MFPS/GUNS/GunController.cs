using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class GunController : MonoBehaviour
{
    public float damage = 10f;
    public float range = 100f;
    public float fireRate = 0.1f;
    public Camera fpsCamera; // Основная камера
    public AudioSource gunAudio;
    public GameObject impactEffect;
    public GameObject sparksPrefab; // Префаб искр
    public GameObject smokePrefab; // Префаб дыма
    public Transform firePoint; // Точка для стрельбы
    public Image crosshair; // Ссылка на перекрестие прицела

    // Ссылки на эффекты
    public Light muzzleFlashLight;
    public SpriteRenderer muzzleFireSprite; // Основной спрайт огня
    public Sprite[] fireSprites; // Массив спрайтов для чередования
    private int currentSpriteIndex = 0; // Текущий индекс спрайта

    // Параметры отдачи
    public Transform gunTransform; // Объект, который будет двигаться при отдаче
    public float recoilAmount = 0.1f; // Сила отдачи назад
    public float recoilSpeed = 6f; // Скорость возврата позиции
    public float recoilRotationAmountX = 2f; // Сила поворота вверх
    public float recoilRotationAmountY = 1f; // Сила поворота вправо
    public float recoilRotationSpeed = 4f; // Скорость возврата поворота

    // Параметры смещения камеры
    public float cameraRecoilAmount = 0.05f; // Сила смещения камеры
    public float cameraRecoilSpeed = 8f; // Скорость возврата камеры

    // Параметры прицеливания
    public Transform aimPosition; // Позиция для прицеливания
    public float aimSpeed = 10f; // Скорость перехода к прицеливанию
    private Vector3 originalPosition; // Исходное положение оружия
    private Quaternion originalRotation; // Исходный поворот оружия
    private Vector3 originalCameraPosition; // Исходное положение камеры
    public float normalFOV = 60f; // Обычное поле зрения камеры
    public float aimFOV = 40f; // Поле зрения при прицеливании

    private float nextTimeToFire = 0f;
    private bool isAiming = false; // Флаг прицеливания

    void Start()
    {
        // Сохраняем исходное положение и поворот оружия и камеры
        if (gunTransform != null)
        {
            originalPosition = gunTransform.localPosition;
            originalRotation = gunTransform.localRotation;
        }

        if (fpsCamera != null)
        {
            originalCameraPosition = fpsCamera.transform.localPosition;
        }

        // Изначально выключаем эффекты вспышки
        if (muzzleFlashLight != null)
        {
            muzzleFlashLight.enabled = false; // Выключаем свет при старте
        }

        if (muzzleFireSprite != null)
        {
            muzzleFireSprite.enabled = false; // Выключаем спрайт вспышки при старте
        }

        // Устанавливаем стандартное поле зрения камеры
        fpsCamera.fieldOfView = normalFOV;

        // Убедитесь, что перекрестие включено при старте
        if (crosshair != null)
        {
            crosshair.enabled = true;
        }
    }

    void Update()
    {
        // Проверка нажатия кнопки стрельбы и времени до следующего выстрела
        if (Input.GetButton("Fire1") && Time.time >= nextTimeToFire)
        {
            nextTimeToFire = Time.time + fireRate;
            Shoot();
        }

        // Управление прицеливанием
        if (Input.GetMouseButton(1)) // Правая кнопка мыши для прицеливания
        {
            isAiming = true;
            AimDownSights();
        }
        else
        {
            isAiming = false;
            ReturnToNormal();
        }

        // Постепенное возвращение оружия и камеры в исходное положение и поворот
        if (gunTransform != null && !isAiming)
        {
            gunTransform.localPosition = Vector3.Lerp(gunTransform.localPosition, originalPosition, Time.deltaTime * recoilSpeed);
            gunTransform.localRotation = Quaternion.Slerp(gunTransform.localRotation, originalRotation, Time.deltaTime * recoilRotationSpeed);
        }

        if (fpsCamera != null)
        {
            fpsCamera.transform.localPosition = Vector3.Lerp(fpsCamera.transform.localPosition, originalCameraPosition, Time.deltaTime * cameraRecoilSpeed);
        }

        // Управление отображением перекрестия
        if (crosshair != null)
        {
            crosshair.enabled = !isAiming; // Скрываем перекрестие при прицеливании
        }
    }

    void Shoot()
    {
        // Запуск вспышки света
        if (muzzleFlashLight != null)
        {
            StartCoroutine(MuzzleFlashEffect());
        }

        // Показать текстуру огня и чередовать спрайты
        if (muzzleFireSprite != null && fireSprites.Length > 0)
        {
            muzzleFireSprite.sprite = fireSprites[currentSpriteIndex];
            currentSpriteIndex = (currentSpriteIndex + 1) % fireSprites.Length; // Циклически переключаем спрайты
            StartCoroutine(MuzzleFireEffect());
        }

        // Воспроизведение звука стрельбы
        if (gunAudio != null)
        {
            gunAudio.Play();
        }

        // Добавление эффекта отдачи
        if (gunTransform != null)
        {
            // Смещение оружия назад
            gunTransform.localPosition -= new Vector3(0, 0, recoilAmount);

            // Поворот оружия, только если не прицеливаемся
            if (!isAiming)
            {
                gunTransform.localRotation *= Quaternion.Euler(-recoilRotationAmountX, recoilRotationAmountY, 0); // Поворот вверх и вправо
            }
        }

        // Смещение камеры при отдаче
        if (fpsCamera != null)
        {
            fpsCamera.transform.localPosition += new Vector3(cameraRecoilAmount, cameraRecoilAmount / 2, 0); // Смещение камеры вверх и вправо
        }

        // Проверка на попадание
        RaycastHit hit;
        if (Physics.Raycast(fpsCamera.transform.position, fpsCamera.transform.forward, out hit, range))
        {
            Debug.Log(hit.transform.name);

            // Попадание по объекту с Target.cs
            Target target = hit.transform.GetComponent<Target>();
            if (target != null)
            {
                // Передаем урон, точку попадания, направление попадания и нормаль
                target.TakeDamage(damage, hit.point, fpsCamera.transform.forward, hit.normal);
            }

            // Попадание по зомби с Zombie.cs
            Zombie zombie = hit.transform.GetComponentInParent<Zombie>();
            if (zombie != null)
            {
                // Передаем урон и коллайдер попадания
                zombie.TakeDamage(damage, hit.collider);
            }

            // Создание эффекта попадания
            if (impactEffect != null)
            {
                GameObject impactGO = Instantiate(impactEffect, hit.point, Quaternion.LookRotation(hit.normal));
                Destroy(impactGO, 2f);
            }

            // Создание эффекта искр в месте попадания
            if (sparksPrefab != null)
            {
                GameObject sparks = Instantiate(sparksPrefab, hit.point, Quaternion.identity); // Устанавливаем без вращения
                sparks.transform.forward = fpsCamera.transform.forward; // Направляем по направлению к камере
                StartCoroutine(DisableSparks(sparks, 0.1f)); // Включаем на короткое время
            }

            // Создание эффекта дыма в месте попадания
            if (smokePrefab != null)
            {
                GameObject smoke = Instantiate(smokePrefab, hit.point, Quaternion.identity); // Устанавливаем без вращения
                smoke.transform.forward = fpsCamera.transform.forward; // Направляем по направлению к камере
                StartCoroutine(FadeOutSmoke(smoke, 1.5f)); // Включаем и плавно скрываем дым
            }
        }
    }

    // Функция для управления прицеливанием
    void AimDownSights()
    {
        // Перемещаем оружие в позицию прицеливания
        gunTransform.localPosition = Vector3.Lerp(gunTransform.localPosition, aimPosition.localPosition, Time.deltaTime * aimSpeed);
        // Уменьшаем поле зрения камеры для эффекта приближения
        fpsCamera.fieldOfView = Mathf.Lerp(fpsCamera.fieldOfView, aimFOV, Time.deltaTime * aimSpeed);
    }

    // Функция для возврата к обычному виду
    void ReturnToNormal()
    {
        // Возвращаем оружие в исходное положение
        gunTransform.localPosition = Vector3.Lerp(gunTransform.localPosition, originalPosition, Time.deltaTime * aimSpeed);
        // Возвращаем поле зрения камеры в обычное состояние
        fpsCamera.fieldOfView = Mathf.Lerp(fpsCamera.fieldOfView, normalFOV, Time.deltaTime * aimSpeed);
    }

    // Коррутина для управления вспышкой света
    IEnumerator MuzzleFlashEffect()
    {
        muzzleFlashLight.enabled = true; // Включаем свет
        yield return new WaitForSeconds(0.05f); // Оставляем свет включенным на короткое время
        muzzleFlashLight.enabled = false; // Отключаем свет
    }

    // Коррутина для управления спрайтом огня
    IEnumerator MuzzleFireEffect()
    {
        muzzleFireSprite.enabled = true; // Включаем отображение спрайта огня
        yield return new WaitForSeconds(0.05f); // Оставляем огонь включенным на короткое время
        muzzleFireSprite.enabled = false; // Отключаем отображение спрайта
    }

    // Коррутина для отключения искр
    IEnumerator DisableSparks(GameObject sparks, float delay)
    {
        yield return new WaitForSeconds(delay);
        sparks.SetActive(false);
        Destroy(sparks); // Уничтожаем объект
    }

    // Коррутина для плавного исчезновения дыма с уменьшением масштаба
    IEnumerator FadeOutSmoke(GameObject smoke, float duration)
    {
        SpriteRenderer smokeRenderer = smoke.GetComponent<SpriteRenderer>();
        Color originalColor = smokeRenderer.color;
        Vector3 originalScale = smoke.transform.localScale; // Исходный масштаб
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0, 1, elapsed / duration); // Используем сглаживание
            smokeRenderer.color = Color.Lerp(originalColor, new Color(originalColor.r, originalColor.g, originalColor.b, 0), t);
            smoke.transform.localScale = Vector3.Lerp(originalScale, Vector3.zero, t); // Уменьшаем масштаб
            yield return null;
        }

        Destroy(smoke); // Уничтожаем объект после исчезновения
    }
}
