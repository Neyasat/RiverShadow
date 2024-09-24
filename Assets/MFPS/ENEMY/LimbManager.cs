using UnityEngine;
using System.Collections;

public class LimbManager : MonoBehaviour
{
    // Ссылка на скрипт Zombie для взаимодействия
    private Zombie zombie;

    // Параметры конечностей
    [Header("Сегменты конечностей")]

    // Голова
    [Header("Голова")]
    public GameObject head;          // Голова зомби
    public Collider headCollider;    // Коллайдер головы

    [HideInInspector]
    public bool headRemoved = false;

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

    [HideInInspector]
    public bool leftArmRemoved = false;
    [HideInInspector]
    public bool rightArmRemoved = false;
    [HideInInspector]
    public bool leftLegRemoved = false;
    [HideInInspector]
    public bool rightLegRemoved = false;

    [Header("Здоровье конечностей")]
    public float headHealth = 20f;
    public float leftArmHealth = 15f;
    public float rightArmHealth = 15f;
    public float leftLegHealth = 20f;
    public float rightLegHealth = 20f;

    // Звуковой эффект при отстреле конечности
    public AudioClip limbRemovalSound;

    [Header("Настройки крови")]
    public Color bloodColor = new Color(1f, 0f, 0f, 1f); // Чистый красный цвет крови

    // Добавляем коэффициенты количества крови
    [Header("Коэффициенты количества крови")]
    [Tooltip("Множитель количества частиц для фонтана крови")]
    public float bloodParticleMultiplierFountain = 50000f; // По умолчанию 50000
    [Tooltip("Множитель количества частиц для взрыва крови")]
    public float bloodParticleMultiplierBurst = 500000f;   // По умолчанию 500000

    void Start()
    {
        zombie = GetComponent<Zombie>();
    }

    // Метод для обработки попадания в конечность
    public bool CheckLimbHit(Collider hitCollider, float damage)
    {
        if (zombie.isDead) return false;

        if (hitCollider == headCollider && !headRemoved)
        {
            headHealth -= damage;
            if (headHealth <= 0f)
            {
                RemoveLimb("Head");
                headRemoved = true;
                zombie.Die(); // Зомби умирает при потере головы
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
                zombie.UpdateMovementAfterLegLoss();
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
                zombie.UpdateMovementAfterLegLoss();
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
                // Отключаем визуальные и физические компоненты головы
                DisableLimb(head);

                // Отключаем коллайдер головы
                headCollider.enabled = false;
                break;

            case "LeftArm":
                // Отключаем визуальные и физические компоненты левой руки
                DisableLimb(leftUpperArm);
                DisableLimb(leftForearm);
                DisableLimb(leftHand);

                // Отключаем коллайдер
                leftArmCollider.enabled = false;
                break;

            case "RightArm":
                // Отключаем визуальные и физические компоненты правой руки
                DisableLimb(rightUpperArm);
                DisableLimb(rightForearm);
                DisableLimb(rightHand);

                // Отключаем коллайдер
                rightArmCollider.enabled = false;
                break;

            case "LeftLeg":
                // Отключаем визуальные и физические компоненты левой ноги
                DisableLimb(leftThigh);
                DisableLimb(leftCalf);
                DisableLimb(leftFoot);

                // Отключаем коллайдер
                leftLegCollider.enabled = false;
                break;

            case "RightLeg":
                // Отключаем визуальные и физические компоненты правой ноги
                DisableLimb(rightThigh);
                DisableLimb(rightCalf);
                DisableLimb(rightFoot);

                // Отключаем коллайдер
                rightLegCollider.enabled = false;
                break;
        }

        // Добавляем эффекты при отстреле конечности
        PlayLimbRemovalEffects(limbName);
    }

    void DisableLimb(GameObject limb)
    {
        if (limb == null) return;

        // Отключаем все Renderer на конечности
        Renderer[] renderers = limb.GetComponentsInChildren<Renderer>();
        foreach (Renderer renderer in renderers)
        {
            renderer.enabled = false;
        }

        // Отключаем все Collider на конечности
        Collider[] colliders = limb.GetComponentsInChildren<Collider>();
        foreach (Collider collider in colliders)
        {
            collider.enabled = false;
        }
    }

    Vector3 GetBloodDirection(string limbName)
    {
        switch (limbName)
        {
            case "Head":
                return Vector3.up;
            case "LeftArm":
                return Vector3.left;
            case "RightArm":
                return Vector3.right;
            case "LeftLeg":
                return Vector3.down;
            case "RightLeg":
                return Vector3.down;
            default:
                return Vector3.up;
        }
    }

    void PlayLimbRemovalEffects(string limbName)
    {
        // Определяем позицию и направление для эффекта крови
        Vector3 bloodPosition = Vector3.zero;
        Vector3 bloodDirection = GetBloodDirection(limbName);
        float bloodVolume = 1f;
        Transform limbTransform = null;

        CapsuleCollider capsuleCollider = null;

        switch (limbName)
        {
            case "Head":
                capsuleCollider = headCollider as CapsuleCollider;
                limbTransform = headCollider.transform;
                break;
            case "LeftArm":
                capsuleCollider = leftArmCollider as CapsuleCollider;
                limbTransform = leftArmCollider.transform;
                break;
            case "RightArm":
                capsuleCollider = rightArmCollider as CapsuleCollider;
                limbTransform = rightArmCollider.transform;
                break;
            case "LeftLeg":
                capsuleCollider = leftLegCollider as CapsuleCollider;
                limbTransform = leftLegCollider.transform;
                break;
            case "RightLeg":
                capsuleCollider = rightLegCollider as CapsuleCollider;
                limbTransform = rightLegCollider.transform;
                break;
        }

        if (capsuleCollider != null)
        {
            // Получаем мировую позицию центра коллайдера с учетом смещения
            bloodPosition = capsuleCollider.transform.TransformPoint(capsuleCollider.center);

            // Получаем объем крови
            bloodVolume = GetCapsuleColliderVolume(capsuleCollider);
        }
        else if (limbTransform != null)
        {
            // Если коллайдер не CapsuleCollider, используем позицию трансформа
            bloodPosition = limbTransform.position;
        }

        // Создаем и настраиваем эффект крови с учетом объема и устанавливаем родителя
        CreateBloodEffect(bloodPosition, bloodDirection, bloodVolume, limbTransform);

        // Проигрываем звук (если настроен)
        if (limbRemovalSound != null)
        {
            AudioSource.PlayClipAtPoint(limbRemovalSound, bloodPosition);
        }
    }

    void CreateBloodEffect(Vector3 position, Vector3 direction, float bloodVolume, Transform parent)
    {
        // Создаем новый игровой объект для эффекта крови
        GameObject bloodEffect = new GameObject("BloodEffect");

        // Устанавливаем родительский объект
        bloodEffect.transform.SetParent(parent);

        // Устанавливаем локальную позицию и вращение относительно родителя
        bloodEffect.transform.localPosition = parent.InverseTransformPoint(position);
        bloodEffect.transform.localRotation = Quaternion.LookRotation(parent.InverseTransformDirection(direction));

        // Создаем дочерний объект для фонтана крови
        GameObject fountainBlood = new GameObject("FountainBlood");
        fountainBlood.transform.SetParent(bloodEffect.transform);
        fountainBlood.transform.localPosition = Vector3.zero;
        fountainBlood.transform.localRotation = Quaternion.identity;

        // Добавляем и настраиваем первый ParticleSystem (фонтан крови)
        ParticleSystem psFountain = fountainBlood.AddComponent<ParticleSystem>();
        ParticleSystemRenderer psrFountain = fountainBlood.GetComponent<ParticleSystemRenderer>();

        var mainFountain = psFountain.main;
        mainFountain.duration = 5f;
        mainFountain.loop = false;
        mainFountain.startLifetime = new ParticleSystem.MinMaxCurve(1f, 2f);
        mainFountain.startSpeed = new ParticleSystem.MinMaxCurve(1f, 3f);
        mainFountain.startSize = new ParticleSystem.MinMaxCurve(0.1f, 0.2f);
        mainFountain.startColor = bloodColor;
        mainFountain.gravityModifier = 0.5f;
        mainFountain.simulationSpace = ParticleSystemSimulationSpace.World;
        mainFountain.maxParticles = Mathf.CeilToInt(bloodVolume * bloodParticleMultiplierFountain);

        // Эмиссия для фонтана
        var emissionFountain = psFountain.emission;
        emissionFountain.enabled = true;
        int totalParticlesFountain = Mathf.CeilToInt(bloodVolume * bloodParticleMultiplierFountain);
        emissionFountain.rateOverTime = totalParticlesFountain / mainFountain.duration;

        // Форма для фонтана
        var shapeFountain = psFountain.shape;
        shapeFountain.enabled = true;
        shapeFountain.shapeType = ParticleSystemShapeType.Cone;
        shapeFountain.angle = 5f;
        shapeFountain.radius = 0.05f;

        // Цвет за время жизни для фонтана
        var colorOverLifetimeFountain = psFountain.colorOverLifetime;
        colorOverLifetimeFountain.enabled = true;
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] {
                new GradientColorKey(bloodColor, 0f),
                new GradientColorKey(bloodColor * 0.8f, 0.5f),
                new GradientColorKey(bloodColor * 0.6f, 1f)
            },
            new GradientAlphaKey[] {
                new GradientAlphaKey(1f, 0f),
                new GradientAlphaKey(0.8f, 0.5f),
                new GradientAlphaKey(0f, 1f)
            }
        );
        colorOverLifetimeFountain.color = new ParticleSystem.MinMaxGradient(gradient);

        // Размер за время жизни для фонтана
        var sizeOverLifetimeFountain = psFountain.sizeOverLifetime;
        sizeOverLifetimeFountain.enabled = true;
        sizeOverLifetimeFountain.size = new ParticleSystem.MinMaxCurve(1f, new AnimationCurve(
            new Keyframe(0f, 0.5f),
            new Keyframe(0.5f, 1f),
            new Keyframe(1f, 0.5f)
        ));

        // Разлет частиц для фонтана
        var noiseFountain = psFountain.noise;
        noiseFountain.enabled = true;
        noiseFountain.strength = 0.5f;
        noiseFountain.frequency = 0.2f;
        noiseFountain.scrollSpeed = 0.5f;

        // Рендерер для фонтана
        psrFountain.material = new Material(Shader.Find("Particles/Standard Unlit"));
        psrFountain.material.color = bloodColor;
        psrFountain.renderMode = ParticleSystemRenderMode.Billboard;

        // Создаем дочерний объект для разлета частиц на 360 градусов
        GameObject burstBlood = new GameObject("BurstBlood");
        burstBlood.transform.SetParent(bloodEffect.transform);
        burstBlood.transform.localPosition = Vector3.zero;
        burstBlood.transform.localRotation = Quaternion.identity;

        // Добавляем и настраиваем второй ParticleSystem (разлет на 360 градусов)
        ParticleSystem psBurst = burstBlood.AddComponent<ParticleSystem>();
        ParticleSystemRenderer psrBurst = burstBlood.GetComponent<ParticleSystemRenderer>();

        var mainBurst = psBurst.main;
        mainBurst.duration = 0.1f;
        mainBurst.loop = false;
        mainBurst.startLifetime = new ParticleSystem.MinMaxCurve(0.5f, 1.5f);
        mainBurst.startSpeed = new ParticleSystem.MinMaxCurve(1f, 3f);
        mainBurst.startSize = new ParticleSystem.MinMaxCurve(0.1f, 0.2f);
        mainBurst.startColor = bloodColor;
        mainBurst.gravityModifier = 0.5f;
        mainBurst.simulationSpace = ParticleSystemSimulationSpace.World;
        mainBurst.maxParticles = Mathf.CeilToInt(bloodVolume * bloodParticleMultiplierBurst);

        // Эмиссия для разлета
        var emissionBurst = psBurst.emission;
        emissionBurst.enabled = true;
        int totalParticlesBurst = Mathf.CeilToInt(bloodVolume * bloodParticleMultiplierBurst);
        emissionBurst.SetBursts(new ParticleSystem.Burst[] {
            new ParticleSystem.Burst(0f, totalParticlesBurst)
        });

        // Форма для разлета
        var shapeBurst = psBurst.shape;
        shapeBurst.enabled = true;
        shapeBurst.shapeType = ParticleSystemShapeType.Sphere;
        shapeBurst.radius = 0.05f;

        // Цвет за время жизни для разлета
        var colorOverLifetimeBurst = psBurst.colorOverLifetime;
        colorOverLifetimeBurst.enabled = true;
        colorOverLifetimeBurst.color = new ParticleSystem.MinMaxGradient(gradient);

        // Размер за время жизни для разлета
        var sizeOverLifetimeBurst = psBurst.sizeOverLifetime;
        sizeOverLifetimeBurst.enabled = true;
        sizeOverLifetimeBurst.size = new ParticleSystem.MinMaxCurve(1f, new AnimationCurve(
            new Keyframe(0f, 1f),
            new Keyframe(1f, 0.5f)
        ));

        // Разлет частиц для разлета
        var noiseBurst = psBurst.noise;
        noiseBurst.enabled = true;
        noiseBurst.strength = 0.5f;
        noiseBurst.frequency = 0.2f;
        noiseBurst.scrollSpeed = 0.5f;

        // Рендерер для разлета
        psrBurst.material = new Material(Shader.Find("Particles/Standard Unlit"));
        psrBurst.material.color = bloodColor;
        psrBurst.renderMode = ParticleSystemRenderMode.Billboard;

        // Запускаем оба эффекта
        psFountain.Play();
        psBurst.Play();

        // Уничтожаем объект после завершения эффекта
        float fountainLifetime = mainFountain.duration + mainFountain.startLifetime.constantMax;
        float burstLifetime = mainBurst.duration + mainBurst.startLifetime.constantMax;
        Destroy(bloodEffect, Mathf.Max(fountainLifetime, burstLifetime));
    }

    float GetCapsuleColliderVolume(CapsuleCollider capsule)
    {
        if (capsule == null) return 1f; // Если коллайдер не является `CapsuleCollider`

        float radius = capsule.radius * Mathf.Max(capsule.transform.lossyScale.x, capsule.transform.lossyScale.z); // Учитываем масштаб по X и Z
        float height = Mathf.Max(0f, capsule.height * capsule.transform.lossyScale.y - 2f * radius); // Учитываем масштаб по Y
        float volume = Mathf.PI * radius * radius * height; // Объем цилиндра
        volume += (4f / 3f) * Mathf.PI * Mathf.Pow(radius, 3); // Объем двух полусфер

        return volume;
    }
}
