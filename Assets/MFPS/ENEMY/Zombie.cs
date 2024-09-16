using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;

public class Zombie : MonoBehaviour
{
    // Параметры здоровья
    public float health = 50f;
    public GameObject deathEffect;
    public float destroyDelay = 2f;
    public bool isDead = false;

    // Параметры атаки и движения
    public Transform player;
    public float attackRange = 2.0f;
    public float attackCooldown = 1.5f;
    public float hitStopDuration = 0.5f;
    public float attackStopDuration = 1f;

    // Параметры `BoxCollider` после смерти
    public Vector3 deathColliderSize = new Vector3(1f, 0.5f, 2f); // Размер `BoxCollider`
    public Vector3 deathColliderCenter = new Vector3(0, 0.25f, 0); // Центр `BoxCollider`

    // Параметры для позиции зомби
    public float spacing = 2.0f; // Минимальное расстояние между зомби

    // Приватные компоненты
    private Animator animator;
    private NavMeshAgent agent;
    private Rigidbody rb;
    private CapsuleCollider capsuleCollider; // Ссылка на `CapsuleCollider`
    private BoxCollider boxCollider; // Новый `BoxCollider` для использования после смерти
    private float lastAttackTime;

    // Параметры конечностей
    [Header("Сегменты конечностей")]

    // Голова
    [Header("Голова")]
    public GameObject head;          // Голова зомби
    public Collider headCollider;    // Коллайдер головы

    private bool headRemoved = false;

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

    private bool leftArmRemoved = false;
    private bool rightArmRemoved = false;
    private bool leftLegRemoved = false;
    private bool rightLegRemoved = false;

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

    void Start()
    {
        animator = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();
        rb = GetComponent<Rigidbody>();
        capsuleCollider = GetComponent<CapsuleCollider>(); // Получаем компонент `CapsuleCollider`

        // Добавляем `BoxCollider`, но пока что его отключаем
        boxCollider = gameObject.AddComponent<BoxCollider>();
        boxCollider.enabled = false;
    }

    void Update()
    {
        if (isDead) return; // Если зомби мертв, не выполняем дальнейшие действия

        // Поддерживаем расстояние между зомби
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

    // Метод для поддержания расстояния между зомби
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

    // Метод для нанесения урона зомби
    public void TakeDamage(float amount, Collider hitCollider)
    {
        if (isDead) return;

        // Проверяем, была ли поражена конечность
        bool limbHit = CheckLimbHit(hitCollider, amount);

        if (!limbHit)
        {
            // Если не попали в конечность, отнимаем урон от общего здоровья
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

    bool CheckLimbHit(Collider hitCollider, float damage)
    {
        if (hitCollider == headCollider && !headRemoved)
        {
            headHealth -= damage;
            if (headHealth <= 0f)
            {
                RemoveLimb("Head");
                headRemoved = true;
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
                UpdateMovementAfterLegLoss();
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
                UpdateMovementAfterLegLoss();
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

                // Зомби умирает при потере головы
                Die();
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

    void UpdateMovementAfterLegLoss()
    {
        // Если зомби потерял одну ногу
        if ((leftLegRemoved && !rightLegRemoved) || (!leftLegRemoved && rightLegRemoved))
        {
            agent.speed *= 0.5f;
        }
        // Если зомби потерял обе ноги
        else if (leftLegRemoved && rightLegRemoved)
        {
            agent.speed *= 0.2f;
            animator.SetBool("isCrawling", true);
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
        ParticleSystemRenderer psrFountain = fountainBlood.AddComponent<ParticleSystemRenderer>();

        var mainFountain = psFountain.main;
        mainFountain.duration = 5f;
        mainFountain.loop = false;
        mainFountain.startLifetime = new ParticleSystem.MinMaxCurve(1f, 2f);
        mainFountain.startSpeed = new ParticleSystem.MinMaxCurve(1f, 3f);
        mainFountain.startSize = new ParticleSystem.MinMaxCurve(0.1f, 0.2f); // Увеличили размер частиц в 5 раз
        mainFountain.startColor = bloodColor; // Используем bloodColor для startColor
        mainFountain.gravityModifier = 0.5f;
        mainFountain.simulationSpace = ParticleSystemSimulationSpace.World;
        mainFountain.maxParticles = 500000; // Увеличили максимальное количество частиц в 10 раз

        // Эмиссия для фонтана
        var emissionFountain = psFountain.emission;
        emissionFountain.enabled = true;
        int totalParticlesFountain = Mathf.CeilToInt(bloodVolume * 5000 * 10);
        emissionFountain.rateOverTime = totalParticlesFountain / 5f; // Частиц в секунду

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
        psrFountain.material.color = Color.white; // Установить цвет материала в белый
        psrFountain.renderMode = ParticleSystemRenderMode.Billboard;

        // Создаем дочерний объект для разлета частиц на 360 градусов
        GameObject burstBlood = new GameObject("BurstBlood");
        burstBlood.transform.SetParent(bloodEffect.transform);
        burstBlood.transform.localPosition = Vector3.zero;
        burstBlood.transform.localRotation = Quaternion.identity;

        // Добавляем и настраиваем второй ParticleSystem (разлет на 360 градусов)
        ParticleSystem psBurst = burstBlood.AddComponent<ParticleSystem>();
        ParticleSystemRenderer psrBurst = burstBlood.AddComponent<ParticleSystemRenderer>();

        var mainBurst = psBurst.main;
        mainBurst.duration = 0.1f; // Короткий взрыв
        mainBurst.loop = false;
        mainBurst.startLifetime = new ParticleSystem.MinMaxCurve(0.5f, 1.5f);
        mainBurst.startSpeed = new ParticleSystem.MinMaxCurve(1f, 3f);
        mainBurst.startSize = new ParticleSystem.MinMaxCurve(0.1f, 0.2f); // Такой же размер частиц
        mainBurst.startColor = bloodColor; // Используем bloodColor для startColor
        mainBurst.gravityModifier = 0.5f;
        mainBurst.simulationSpace = ParticleSystemSimulationSpace.World;
        mainBurst.maxParticles = 500000; // Увеличили максимальное количество частиц в 10 раз

        // Эмиссия для разлета
        var emissionBurst = psBurst.emission;
        emissionBurst.enabled = true;
        emissionBurst.SetBursts(new ParticleSystem.Burst[] {
            new ParticleSystem.Burst(0f, totalParticlesFountain * 10) // Увеличили количество частиц на 10 раз
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
        psrBurst.material.color = Color.white; // Установить цвет материала в белый
        psrBurst.renderMode = ParticleSystemRenderMode.Billboard;

        // Запускаем оба эффекта
        psFountain.Play();
        psBurst.Play();

        // Уничтожаем объект после завершения эффекта
        Destroy(bloodEffect, Mathf.Max(mainFountain.duration + mainFountain.startLifetime.constantMax, mainBurst.duration + mainBurst.startLifetime.constantMax));
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

    void Attack()
    {
        agent.isStopped = true;
        agent.velocity = Vector3.zero;
        animator.SetBool("isAttacking", true);
        lastAttackTime = Time.time;
        StartCoroutine(StopMovementDuringAttack());
    }

    void Die()
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
            StartCoroutine(DisableAnimatorAfterDelay()); // Отключаем анимацию после задержки
        }

        agent.isStopped = true;
        agent.velocity = Vector3.zero;
        agent.enabled = false;

        // Отключаем кинематику и включаем физику для нормального падения
        rb.isKinematic = false;
        rb.useGravity = true;

        // Отключаем `CapsuleCollider` и включаем `BoxCollider`
        capsuleCollider.enabled = false;
        boxCollider.size = deathColliderSize;
        boxCollider.center = deathColliderCenter;
        boxCollider.enabled = true; // Включаем `BoxCollider`

        StartCoroutine(DisableRigidbodyWithDelay());
    }

    IEnumerator DisableAnimatorAfterDelay()
    {
        yield return new WaitForSeconds(7.0f); // Задержка 7 секунд, чтобы анимация смерти успела проиграться
        animator.enabled = false; // Отключаем `Animator`
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

    public void OnAttackEnd()
    {
        animator.SetBool("isAttacking", false);
        if (!isDead)
        {
            agent.isStopped = false;
        }
    }
}
