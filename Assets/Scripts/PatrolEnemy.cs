using UnityEngine;

public class PatrolEnemy : MonoBehaviour
{
    [Header("Stats")]
    public int maxHealth = 5;
    public float moveSpeed = 2f;
    public float chaseSpeed = 4f;

    [Header("Drop Settings")]
    public GameObject coinPrefab;
    public int goldDropAmount = 3;

    [Header("Detection")]
    public Transform player;
    public float attackRange = 10f;
    public float retreatDistance = 2.5f;
    public bool inRange;

    [Header("Ground Check")]
    public Transform checkPoint;
    public float distance = 1f;
    public LayerMask layerMask;

    [Header("Attack")]
    public Transform attackPoint;
    public float attackRadius = 1f;
    public LayerMask attackLayer;
    public float attackCooldown = 1.5f;
    public int attackDamage = 3;

    [Header("State")]
    public bool facingLeft = true;
    public Animator animator;

    [Header("UI")]
    public EnemyHealthBar healthBar;

    private float lastAttackTime;
    private bool isAttacking = false;
    private bool isDead = false;
    private int currentHealth;
    private Rigidbody2D rb;
    private Collider2D col;

    void Start()
    {
        currentHealth = maxHealth;
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
        FaceLeft();
    }

    void Update()
    {
        if (isDead) return;

        UpdateRangeState();

        if (inRange)
            HandleChaseAndAttack();
        else
            Patrol();
    }

    // ===================== STATES =====================

    void UpdateRangeState()
    {
        inRange = Vector2.Distance(transform.position, player.position) < attackRange;
    }

    void HandleChaseAndAttack()
    {
        FlipToPlayer();

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        if (distanceToPlayer > retreatDistance)
        {
            ChasePlayer();
        }
        else
        {
            if (Time.time >= lastAttackTime + attackCooldown && !isAttacking)
                Attack();
        }
    }

    void Patrol()
    {
        animator.SetBool("Attack", false);
        transform.Translate(Vector2.left * moveSpeed * Time.deltaTime);

        if (!IsGroundAhead())
            Flip();
    }

    // ===================== MOVEMENT =====================

    void ChasePlayer()
    {
        animator.SetBool("Attack", false);

        if (!IsGroundAhead())
            return;

        transform.position = Vector2.MoveTowards(
            transform.position,
            player.position,
            chaseSpeed * Time.deltaTime
        );
    }

    // ===================== FLIP =====================

    void FlipToPlayer()
    {
        if (player.position.x < transform.position.x)
            FaceLeft();
        else if (player.position.x > transform.position.x)
            FaceRight();
    }

    void Flip()
    {
        if (facingLeft) FaceRight();
        else FaceLeft();
    }

    void FaceLeft()
    {
        transform.eulerAngles = new Vector3(0, 00, 0);
        facingLeft = true;
    }

    void FaceRight()
    {
        transform.eulerAngles = new Vector3(0, 180, 0);
        facingLeft = false;
    }

    // ===================== UTILS =====================

    bool IsGroundAhead()
    {
        RaycastHit2D hit = Physics2D.Raycast(
            checkPoint.position,
            Vector2.down,
            distance,
            layerMask
        );
        return hit.collider != null;
    }

    // ===================== COMBAT =====================

    void Attack()
    {
        isAttacking = true;
        lastAttackTime = Time.time;

        animator.SetBool("Attack", true);

        Invoke(nameof(DealDamage), 0.5f);
        Invoke(nameof(EndAttack), 1f);
    }

    void DealDamage()
    {
        if (isDead || player == null) return;

        if (Vector2.Distance(transform.position, player.position) <= retreatDistance * 1.3f)
        {
            HeroKnight p = player.GetComponent<HeroKnight>();
            if (p != null) p.TakeDamage(attackDamage);
        }
    }

    void EndAttack()
    {
        isAttacking = false;
        animator.SetBool("Attack", false);
    }

    // ===================== TAKE DAMAGE =====================

    public void TakeDamage(int damage)
    {
        if (isDead) return;

        currentHealth -= damage;

        if (animator != null)
            animator.SetTrigger("Hurt");

        if (healthBar != null)
            healthBar.SetHealth(currentHealth, maxHealth);

        if (currentHealth <= 0)
            Die();
    }

    void Die()
    {
        isDead = true;

        animator.SetBool("Attack", false);

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.gravityScale = 0;
        }

        if (col != null)
            col.enabled = false;

        if (animator != null)
            animator.SetTrigger("Death");

        DropGold();
        Destroy(gameObject, 2f);
    }

    void DropGold()
    {
        if (coinPrefab == null) return;

        for (int i = 0; i < goldDropAmount; i++)
        {
            Vector2 randomOffset = new Vector2(
                Random.Range(-0.5f, 0.5f),
                Random.Range(0f, 0.5f)
            );
            Instantiate(coinPrefab, (Vector2)transform.position + randomOffset, Quaternion.identity);
        }
    }

    // ===================== GIZMOS =====================

    void OnDrawGizmosSelected()
    {
        if (checkPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawRay(checkPoint.position, Vector2.down * distance);
        }

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        if (attackPoint != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(attackPoint.position, attackRadius);
        }
    }
}