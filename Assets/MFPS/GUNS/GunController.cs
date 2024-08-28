using UnityEngine;
using System.Collections;

public class GunController : MonoBehaviour
{
    public float damage = 10f;
    public float range = 100f;
    public float fireRate = 0.1f;
    public Camera fpsCamera;
    public AudioSource gunAudio;
    public GameObject impactEffect;
    public Transform firePoint; // Точка для стрельбы

    // Ссылки на эффекты
    public Light muzzleFlashLight;
    public SpriteRenderer muzzleFireSprite; // Основной спрайт огня
    public Sprite[] fireSprites; // Массив спрайтов для чередования
    private int currentSpriteIndex = 0; // Текущий индекс спрайта

    // Параметры отдачи
    public Transform gunTransform; // Объект, который будет двигаться при отдаче
    public float recoilAmount = 0.1f; // Сила отдачи
    public float recoilSpeed = 6f; // Скорость возврата

    // Параметры прицеливания
    public Transform aimPosition; // Позиция для прицеливания
    public float aimSpeed = 10f; // Скорость перехода к прицеливанию
    private Vector3 originalPosition; // Исходное положение оружия
    public float normalFOV = 60f; // Обычное поле зрения камеры
    public float aimFOV = 40f; // Поле зрения при прицеливании

    private float nextTimeToFire = 0f;
    private bool isAiming = false; // Флаг прицеливания

    void Start()
    {
        // Сохраняем исходное положение оружия
        if (gunTransform != null)
        {
            originalPosition = gunTransform.localPosition;
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

        // Постепенное возвращение оружия в исходное положение
        if (gunTransform != null && !isAiming)
        {
            gunTransform.localPosition = Vector3.Lerp(gunTransform.localPosition, originalPosition, Time.deltaTime * recoilSpeed);
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
            gunTransform.localPosition -= new Vector3(0, 0, recoilAmount); // Смещение оружия назад
        }

        // Проверка на попадание
        RaycastHit hit;
        if (Physics.Raycast(fpsCamera.transform.position, fpsCamera.transform.forward, out hit, range))
        {
            Debug.Log(hit.transform.name);
            Target target = hit.transform.GetComponent<Target>();
            if (target != null)
            {
                target.TakeDamage(damage);
            }

            // Создание эффекта попадания
            if (impactEffect != null)
            {
                GameObject impactGO = Instantiate(impactEffect, hit.point, Quaternion.LookRotation(hit.normal));
                Destroy(impactGO, 2f);
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
}
