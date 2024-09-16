using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;

public class Zombie : MonoBehaviour
{
    // ��������� ��������
    public float health = 50f;
    public GameObject deathEffect;
    public float destroyDelay = 2f;
    public bool isDead = false;

    // ��������� ����� � ��������
    public Transform player;
    public float attackRange = 2.0f;
    public float attackCooldown = 1.5f;
    public float hitStopDuration = 0.5f;
    public float attackStopDuration = 1f;

    // ��������� `BoxCollider` ����� ������
    public Vector3 deathColliderSize = new Vector3(1f, 0.5f, 2f); // ������ `BoxCollider`
    public Vector3 deathColliderCenter = new Vector3(0, 0.25f, 0); // ����� `BoxCollider`

    // ��������� ��� ������� �����
    public float spacing = 2.0f; // ����������� ���������� ����� �����

    // ��������� ����������
    private Animator animator;
    private NavMeshAgent agent;
    private Rigidbody rb;
    private CapsuleCollider capsuleCollider; // ������ �� `CapsuleCollider`
    private BoxCollider boxCollider; // ����� `BoxCollider` ��� ������������� ����� ������
    private float lastAttackTime;

    // ��������� �����������
    [Header("�������� �����������")]

    // ������
    [Header("������")]
    public GameObject head;          // ������ �����
    public Collider headCollider;    // ��������� ������

    private bool headRemoved = false;

    // ����� ����
    public GameObject leftUpperArm;    // ����� �����
    public GameObject leftForearm;     // ����� ����������
    public GameObject leftHand;        // ����� �����

    // ������ ����
    public GameObject rightUpperArm;   // ������ �����
    public GameObject rightForearm;    // ������ ����������
    public GameObject rightHand;       // ������ �����

    // ����� ����
    public GameObject leftThigh;       // ����� �����
    public GameObject leftCalf;        // ����� ������
    public GameObject leftFoot;        // ����� ������

    // ������ ����
    public GameObject rightThigh;      // ������ �����
    public GameObject rightCalf;       // ������ ������
    public GameObject rightFoot;       // ������ ������

    [Header("���������� �����������")]

    public Collider leftArmCollider;
    public Collider rightArmCollider;
    public Collider leftLegCollider;
    public Collider rightLegCollider;

    private bool leftArmRemoved = false;
    private bool rightArmRemoved = false;
    private bool leftLegRemoved = false;
    private bool rightLegRemoved = false;

    [Header("�������� �����������")]
    public float headHealth = 20f;
    public float leftArmHealth = 15f;
    public float rightArmHealth = 15f;
    public float leftLegHealth = 20f;
    public float rightLegHealth = 20f;

    // �������� ������ ��� �������� ����������
    public AudioClip limbRemovalSound;

    [Header("��������� �����")]
    public Color bloodColor = new Color(1f, 0f, 0f, 1f); // ������ ������� ���� �����

    void Start()
    {
        animator = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();
        rb = GetComponent<Rigidbody>();
        capsuleCollider = GetComponent<CapsuleCollider>(); // �������� ��������� `CapsuleCollider`

        // ��������� `BoxCollider`, �� ���� ��� ��� ���������
        boxCollider = gameObject.AddComponent<BoxCollider>();
        boxCollider.enabled = false;
    }

    void Update()
    {
        if (isDead) return; // ���� ����� �����, �� ��������� ���������� ��������

        // ������������ ���������� ����� �����
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

    // ����� ��� ����������� ���������� ����� �����
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

    // ����� ��� ��������� ����� �����
    public void TakeDamage(float amount, Collider hitCollider)
    {
        if (isDead) return;

        // ���������, ���� �� �������� ����������
        bool limbHit = CheckLimbHit(hitCollider, amount);

        if (!limbHit)
        {
            // ���� �� ������ � ����������, �������� ���� �� ������ ��������
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

        return false; // �� ������ � ����������
    }

    void RemoveLimb(string limbName)
    {
        switch (limbName)
        {
            case "Head":
                // ��������� ���������� � ���������� ���������� ������
                DisableLimb(head);

                // ��������� ��������� ������
                headCollider.enabled = false;

                // ����� ������� ��� ������ ������
                Die();
                break;

            case "LeftArm":
                // ��������� ���������� � ���������� ���������� ����� ����
                DisableLimb(leftUpperArm);
                DisableLimb(leftForearm);
                DisableLimb(leftHand);

                // ��������� ���������
                leftArmCollider.enabled = false;
                break;

            case "RightArm":
                // ��������� ���������� � ���������� ���������� ������ ����
                DisableLimb(rightUpperArm);
                DisableLimb(rightForearm);
                DisableLimb(rightHand);

                // ��������� ���������
                rightArmCollider.enabled = false;
                break;

            case "LeftLeg":
                // ��������� ���������� � ���������� ���������� ����� ����
                DisableLimb(leftThigh);
                DisableLimb(leftCalf);
                DisableLimb(leftFoot);

                // ��������� ���������
                leftLegCollider.enabled = false;
                break;

            case "RightLeg":
                // ��������� ���������� � ���������� ���������� ������ ����
                DisableLimb(rightThigh);
                DisableLimb(rightCalf);
                DisableLimb(rightFoot);

                // ��������� ���������
                rightLegCollider.enabled = false;
                break;
        }

        // ��������� ������� ��� �������� ����������
        PlayLimbRemovalEffects(limbName);
    }

    void DisableLimb(GameObject limb)
    {
        if (limb == null) return;

        // ��������� ��� Renderer �� ����������
        Renderer[] renderers = limb.GetComponentsInChildren<Renderer>();
        foreach (Renderer renderer in renderers)
        {
            renderer.enabled = false;
        }

        // ��������� ��� Collider �� ����������
        Collider[] colliders = limb.GetComponentsInChildren<Collider>();
        foreach (Collider collider in colliders)
        {
            collider.enabled = false;
        }
    }

    void UpdateMovementAfterLegLoss()
    {
        // ���� ����� ������� ���� ����
        if ((leftLegRemoved && !rightLegRemoved) || (!leftLegRemoved && rightLegRemoved))
        {
            agent.speed *= 0.5f;
        }
        // ���� ����� ������� ��� ����
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
        // ���������� ������� � ����������� ��� ������� �����
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
            // �������� ������� ������� ������ ���������� � ������ ��������
            bloodPosition = capsuleCollider.transform.TransformPoint(capsuleCollider.center);

            // �������� ����� �����
            bloodVolume = GetCapsuleColliderVolume(capsuleCollider);
        }
        else if (limbTransform != null)
        {
            // ���� ��������� �� CapsuleCollider, ���������� ������� ����������
            bloodPosition = limbTransform.position;
        }

        // ������� � ����������� ������ ����� � ������ ������ � ������������� ��������
        CreateBloodEffect(bloodPosition, bloodDirection, bloodVolume, limbTransform);

        // ����������� ���� (���� ��������)
        if (limbRemovalSound != null)
        {
            AudioSource.PlayClipAtPoint(limbRemovalSound, bloodPosition);
        }
    }

    void CreateBloodEffect(Vector3 position, Vector3 direction, float bloodVolume, Transform parent)
    {
        // ������� ����� ������� ������ ��� ������� �����
        GameObject bloodEffect = new GameObject("BloodEffect");

        // ������������� ������������ ������
        bloodEffect.transform.SetParent(parent);

        // ������������� ��������� ������� � �������� ������������ ��������
        bloodEffect.transform.localPosition = parent.InverseTransformPoint(position);
        bloodEffect.transform.localRotation = Quaternion.LookRotation(parent.InverseTransformDirection(direction));

        // ������� �������� ������ ��� ������� �����
        GameObject fountainBlood = new GameObject("FountainBlood");
        fountainBlood.transform.SetParent(bloodEffect.transform);
        fountainBlood.transform.localPosition = Vector3.zero;
        fountainBlood.transform.localRotation = Quaternion.identity;

        // ��������� � ����������� ������ ParticleSystem (������ �����)
        ParticleSystem psFountain = fountainBlood.AddComponent<ParticleSystem>();
        ParticleSystemRenderer psrFountain = fountainBlood.AddComponent<ParticleSystemRenderer>();

        var mainFountain = psFountain.main;
        mainFountain.duration = 5f;
        mainFountain.loop = false;
        mainFountain.startLifetime = new ParticleSystem.MinMaxCurve(1f, 2f);
        mainFountain.startSpeed = new ParticleSystem.MinMaxCurve(1f, 3f);
        mainFountain.startSize = new ParticleSystem.MinMaxCurve(0.1f, 0.2f); // ��������� ������ ������ � 5 ���
        mainFountain.startColor = bloodColor; // ���������� bloodColor ��� startColor
        mainFountain.gravityModifier = 0.5f;
        mainFountain.simulationSpace = ParticleSystemSimulationSpace.World;
        mainFountain.maxParticles = 500000; // ��������� ������������ ���������� ������ � 10 ���

        // ������� ��� �������
        var emissionFountain = psFountain.emission;
        emissionFountain.enabled = true;
        int totalParticlesFountain = Mathf.CeilToInt(bloodVolume * 5000 * 10);
        emissionFountain.rateOverTime = totalParticlesFountain / 5f; // ������ � �������

        // ����� ��� �������
        var shapeFountain = psFountain.shape;
        shapeFountain.enabled = true;
        shapeFountain.shapeType = ParticleSystemShapeType.Cone;
        shapeFountain.angle = 5f;
        shapeFountain.radius = 0.05f;

        // ���� �� ����� ����� ��� �������
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

        // ������ �� ����� ����� ��� �������
        var sizeOverLifetimeFountain = psFountain.sizeOverLifetime;
        sizeOverLifetimeFountain.enabled = true;
        sizeOverLifetimeFountain.size = new ParticleSystem.MinMaxCurve(1f, new AnimationCurve(
            new Keyframe(0f, 0.5f),
            new Keyframe(0.5f, 1f),
            new Keyframe(1f, 0.5f)
        ));

        // ������ ������ ��� �������
        var noiseFountain = psFountain.noise;
        noiseFountain.enabled = true;
        noiseFountain.strength = 0.5f;
        noiseFountain.frequency = 0.2f;
        noiseFountain.scrollSpeed = 0.5f;

        // �������� ��� �������
        psrFountain.material = new Material(Shader.Find("Particles/Standard Unlit"));
        psrFountain.material.color = Color.white; // ���������� ���� ��������� � �����
        psrFountain.renderMode = ParticleSystemRenderMode.Billboard;

        // ������� �������� ������ ��� ������� ������ �� 360 ��������
        GameObject burstBlood = new GameObject("BurstBlood");
        burstBlood.transform.SetParent(bloodEffect.transform);
        burstBlood.transform.localPosition = Vector3.zero;
        burstBlood.transform.localRotation = Quaternion.identity;

        // ��������� � ����������� ������ ParticleSystem (������ �� 360 ��������)
        ParticleSystem psBurst = burstBlood.AddComponent<ParticleSystem>();
        ParticleSystemRenderer psrBurst = burstBlood.AddComponent<ParticleSystemRenderer>();

        var mainBurst = psBurst.main;
        mainBurst.duration = 0.1f; // �������� �����
        mainBurst.loop = false;
        mainBurst.startLifetime = new ParticleSystem.MinMaxCurve(0.5f, 1.5f);
        mainBurst.startSpeed = new ParticleSystem.MinMaxCurve(1f, 3f);
        mainBurst.startSize = new ParticleSystem.MinMaxCurve(0.1f, 0.2f); // ����� �� ������ ������
        mainBurst.startColor = bloodColor; // ���������� bloodColor ��� startColor
        mainBurst.gravityModifier = 0.5f;
        mainBurst.simulationSpace = ParticleSystemSimulationSpace.World;
        mainBurst.maxParticles = 500000; // ��������� ������������ ���������� ������ � 10 ���

        // ������� ��� �������
        var emissionBurst = psBurst.emission;
        emissionBurst.enabled = true;
        emissionBurst.SetBursts(new ParticleSystem.Burst[] {
            new ParticleSystem.Burst(0f, totalParticlesFountain * 10) // ��������� ���������� ������ �� 10 ���
        });

        // ����� ��� �������
        var shapeBurst = psBurst.shape;
        shapeBurst.enabled = true;
        shapeBurst.shapeType = ParticleSystemShapeType.Sphere;
        shapeBurst.radius = 0.05f;

        // ���� �� ����� ����� ��� �������
        var colorOverLifetimeBurst = psBurst.colorOverLifetime;
        colorOverLifetimeBurst.enabled = true;
        colorOverLifetimeBurst.color = new ParticleSystem.MinMaxGradient(gradient);

        // ������ �� ����� ����� ��� �������
        var sizeOverLifetimeBurst = psBurst.sizeOverLifetime;
        sizeOverLifetimeBurst.enabled = true;
        sizeOverLifetimeBurst.size = new ParticleSystem.MinMaxCurve(1f, new AnimationCurve(
            new Keyframe(0f, 1f),
            new Keyframe(1f, 0.5f)
        ));

        // ������ ������ ��� �������
        var noiseBurst = psBurst.noise;
        noiseBurst.enabled = true;
        noiseBurst.strength = 0.5f;
        noiseBurst.frequency = 0.2f;
        noiseBurst.scrollSpeed = 0.5f;

        // �������� ��� �������
        psrBurst.material = new Material(Shader.Find("Particles/Standard Unlit"));
        psrBurst.material.color = Color.white; // ���������� ���� ��������� � �����
        psrBurst.renderMode = ParticleSystemRenderMode.Billboard;

        // ��������� ��� �������
        psFountain.Play();
        psBurst.Play();

        // ���������� ������ ����� ���������� �������
        Destroy(bloodEffect, Mathf.Max(mainFountain.duration + mainFountain.startLifetime.constantMax, mainBurst.duration + mainBurst.startLifetime.constantMax));
    }

    float GetCapsuleColliderVolume(CapsuleCollider capsule)
    {
        if (capsule == null) return 1f; // ���� ��������� �� �������� `CapsuleCollider`

        float radius = capsule.radius * Mathf.Max(capsule.transform.lossyScale.x, capsule.transform.lossyScale.z); // ��������� ������� �� X � Z
        float height = Mathf.Max(0f, capsule.height * capsule.transform.lossyScale.y - 2f * radius); // ��������� ������� �� Y
        float volume = Mathf.PI * radius * radius * height; // ����� ��������
        volume += (4f / 3f) * Mathf.PI * Mathf.Pow(radius, 3); // ����� ���� ��������

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
            StartCoroutine(DisableAnimatorAfterDelay()); // ��������� �������� ����� ��������
        }

        agent.isStopped = true;
        agent.velocity = Vector3.zero;
        agent.enabled = false;

        // ��������� ���������� � �������� ������ ��� ����������� �������
        rb.isKinematic = false;
        rb.useGravity = true;

        // ��������� `CapsuleCollider` � �������� `BoxCollider`
        capsuleCollider.enabled = false;
        boxCollider.size = deathColliderSize;
        boxCollider.center = deathColliderCenter;
        boxCollider.enabled = true; // �������� `BoxCollider`

        StartCoroutine(DisableRigidbodyWithDelay());
    }

    IEnumerator DisableAnimatorAfterDelay()
    {
        yield return new WaitForSeconds(7.0f); // �������� 7 ������, ����� �������� ������ ������ �����������
        animator.enabled = false; // ��������� `Animator`
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
