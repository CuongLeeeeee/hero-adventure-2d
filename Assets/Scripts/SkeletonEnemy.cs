using UnityEngine;

public class SkeletonEnemy : MonoBehaviour
{
    public enum EnemyState
    {
        Patrol,
        Chase,
        Attack,
        Dead
    }

    [Header("Stats")]
    public int maxHealth = 5;
    public float moveSpeed = 2f;
    public float chaseSpeed = 4f;

    [Header("Detection")]
    public Transform player;
    public float detectRange = 10f;
    public float attackRange = 2.5f;

    [Header("Ground Check")]
    public Transform checkPoint;
    public float groundDistance = 1f;
    public LayerMask groundLayer;

    [Header("Attack")]
    public Transform attackPoint;
    public float attackRadius = 1f;
    public LayerMask attackLayer;
    public float attackCooldown = 1.5f;
    public int attackDamage = 3;

    [Header("Drop")]
    public GameObject coinPrefab;
    public int goldDropAmount = 3;

    [Header("Animation")]
    public Animator animator;

    private EnemyState currentState;

    private Rigidbody2D rb;
    private Collider2D col;

    private int currentHealth;
    private bool facingLeft = true;

    private float attackTimer;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();

        currentHealth = maxHealth;
        currentState = EnemyState.Patrol;
    }

    void Update()
    {
        if (currentState == EnemyState.Dead) return;

        attackTimer += Time.deltaTime;

        float distance = Vector2.Distance(transform.position, player.position);

        switch (currentState)
        {
            case EnemyState.Patrol:
                Patrol();

                if (distance < detectRange)
                    currentState = EnemyState.Chase;
                break;

            case EnemyState.Chase:
                ChasePlayer();

                if (distance < attackRange)
                    currentState = EnemyState.Attack;

                if (distance > detectRange)
                    currentState = EnemyState.Patrol;
                break;

            case EnemyState.Attack:
                Attack();

                if (distance > attackRange)
                    currentState = EnemyState.Chase;
                break;
        }
    }

    // ================= PATROL =================

    void Patrol()
    {
        animator.SetBool("isMoving", true);

        float direction = facingLeft ? -1 : 1;

        rb.linearVelocity = new Vector2(direction * moveSpeed, rb.linearVelocity.y);

        if (!IsGroundAhead())
        {
            Flip();
            return;
        }   
    }

    // ================= CHASE =================

    void ChasePlayer()
    {
        animator.SetBool("Attack 1", false);

        if (!IsGroundAhead())
        {
            Flip();
            return;
        }

        transform.position = Vector2.MoveTowards(
            transform.position,
            player.position,
            chaseSpeed * Time.deltaTime
        );
    }

    // ================= ATTACK =================

    void Attack()
    {
        rb.linearVelocity = Vector2.zero;

        FlipToPlayer();

        if (attackTimer >= attackCooldown)
        {
            animator.SetTrigger("attack");

            Collider2D hit = Physics2D.OverlapCircle(
                attackPoint.position,
                attackRadius,
                attackLayer
            );

            if (hit != null)
            {
                HeroKnight hero = hit.GetComponent<HeroKnight>();

                if (hero != null)
                    hero.TakeDamage(attackDamage);
            }

            attackTimer = 0f;
        }
    }

    // ================= FLIP =================

    void FlipToPlayer()
    {
        if (player.position.x < transform.position.x && !facingLeft)
            Flip();

        if (player.position.x > transform.position.x && facingLeft)
            Flip();
    }

    void Flip()
    {
        facingLeft = !facingLeft;

        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }

    // ================= GROUND =================

    bool IsGroundAhead()
    {
        RaycastHit2D hit = Physics2D.Raycast(
            checkPoint.position,
            Vector2.down,
            groundDistance,
            groundLayer
        );

        return hit.collider != null;
    }

    // ================= DAMAGE =================

    public void TakeDamage(int damage)
    {
        if (currentState == EnemyState.Dead) return;

        currentHealth -= damage;

        animator.SetTrigger("hurt");

        if (currentHealth <= 0)
            Die();
    }

    void Die()
    {
        currentState = EnemyState.Dead;

        rb.linearVelocity = Vector2.zero;
        rb.gravityScale = 0;

        col.enabled = false;

        animator.SetTrigger("death");

        DropGold();

        Destroy(gameObject, 2f);
    }

    // ================= DROP =================

    void DropGold()
    {
        if (coinPrefab == null) return;

        for (int i = 0; i < goldDropAmount; i++)
        {
            Vector2 offset = new Vector2(
                Random.Range(-0.5f, 0.5f),
                Random.Range(0f, 0.5f)
            );

            Instantiate(
                coinPrefab,
                (Vector2)transform.position + offset,
                Quaternion.identity
            );
        }
    }

    // ================= DEBUG =================

    void OnDrawGizmosSelected()
    {
        if (checkPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawRay(checkPoint.position, Vector2.down * groundDistance);
        }

        if (attackPoint != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(attackPoint.position, attackRadius);
        }

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, detectRange);
    }
}