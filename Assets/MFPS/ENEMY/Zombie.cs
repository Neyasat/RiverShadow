using UnityEngine;
using UnityEngine.AI;

public class Zombie : MonoBehaviour
{
    public Transform player;
    public float attackRange = 2.0f;
    public float attackCooldown = 1.5f;

    private NavMeshAgent agent;
    private Animator animator;
    private float lastAttackTime;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
    }

    void Update()
    {
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

            // Синхронизация скорости анимации с реальной скоростью
            float speedPercent = agent.velocity.magnitude / agent.speed;
            animator.SetFloat("MoveSpeed", speedPercent);
        }
    }

    void Attack()
    {
        agent.isStopped = true;
        agent.velocity = Vector3.zero; // Останавливаем движение полностью
        animator.SetBool("isAttacking", true);
        lastAttackTime = Time.time;
    }

    public void OnAttackEnd()
    {
        animator.SetBool("isAttacking", false);
        agent.isStopped = false; // Возобновляем движение после завершения атаки
    }
}
