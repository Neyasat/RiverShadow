using UnityEngine;
using UnityEngine.AI;
using System.Collections;

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
    private CapsuleCollider capsuleCollider; // ������ �� ���������� ���������
    private BoxCollider boxCollider; // ����� BoxCollider ��� ������������� ����� ������
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

    void Start()
    {
        animator = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();
        rb = GetComponent<Rigidbody>();
        capsuleCollider = GetComponent<CapsuleCollider>(); // �������� ��������� CapsuleCollider

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
                // ��������� ������
                head.SetActive(false);

                // ��������� ���������
                headCollider.enabled = false;

                // ����� ������� ��� ������ ������
                Die();
                break;

            case "LeftArm":
                // ��������� �������� ����� ����
                leftUpperArm.SetActive(false);
                leftForearm.SetActive(false);
                leftHand.SetActive(false);

                // ��������� ���������
                leftArmCollider.enabled = false;
                break;

            case "RightArm":
                // ��������� �������� ������ ����
                rightUpperArm.SetActive(false);
                rightForearm.SetActive(false);
                rightHand.SetActive(false);

                // ��������� ���������
                rightArmCollider.enabled = false;
                break;

            case "LeftLeg":
                // ��������� �������� ����� ����
                leftThigh.SetActive(false);
                leftCalf.SetActive(false);
                leftFoot.SetActive(false);

                // ��������� ���������
                leftLegCollider.enabled = false;
                break;

            case "RightLeg":
                // ��������� �������� ������ ����
                rightThigh.SetActive(false);
                rightCalf.SetActive(false);
                rightFoot.SetActive(false);

                // ��������� ���������
                rightLegCollider.enabled = false;
                break;
        }

        // ��������� �������������� ������� ��� �������� ����������
        PlayLimbRemovalEffects(limbName);
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

    void PlayLimbRemovalEffects(string limbName)
    {
        // ���������� ���� ������� �����
        // ��������:
        // Instantiate(bloodEffect, transform.position, Quaternion.identity);
        // AudioSource.PlayClipAtPoint(limbRemovalSound, transform.position);
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

        // ��������� ���������� ��������� � �������� ����������
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
