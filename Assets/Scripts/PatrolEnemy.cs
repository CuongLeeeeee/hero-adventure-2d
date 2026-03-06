using UnityEngine;

public class PatrolEnemy : MonoBehaviour
{
    [Header("Stats")]
    [SerializeField] private int maxHealth = 5;
    [SerializeField] private float moveSpeed = 0.7f;
    [SerializeField] private float chaseSpeed = 2f;

    [Header("Drop Settings")]
    [SerializeField] private GameObject coinPrefab;
    [SerializeField] private int goldDropAmount = 3;

    [Header("Detection")]
    [SerializeField] private Transform player;
    [SerializeField] private float attackRange = 10f;
    [SerializeField] private float retreatDistance = 2.5f;

    [Header("Ground Check")]
    [SerializeField] private Transform checkPoint;
    [SerializeField] private float distance = 1f;
    [SerializeField] private LayerMask layerMask;

    [Header("Attack")]
    [SerializeField] private Transform attackPoint;
    [SerializeField] private float attackRadius = 1f;
    [SerializeField] private LayerMask attackLayer;
    [SerializeField] private float attackCooldown = 1.5f;
    [SerializeField] private int attackDamage = 3;

    // đưa timing về Inspector (1 nơi)
    [SerializeField] private float hitDelay = 0.5f;
    [SerializeField] private float endAttackDelay = 1f;

    [Header("Animation")]
    [SerializeField] private bool startFacingLeft = true;
    [SerializeField] private Animator animator;

    // ===== Runtime state (không config ở Inspector) =====
    private float lastAttackTime;
    private bool isAttacking;
    private bool isDead;
    private bool inRange;
    private bool facingLeft;
    private int currentHealth;

    private Rigidbody2D rb;
    private Collider2D col;

    // Animator params (tránh sai chính tả)
    private static readonly int Attack1 = Animator.StringToHash("Attack 1");
    private static readonly int Hurt = Animator.StringToHash("Hurt");
    private static readonly int Death = Animator.StringToHash("Death");

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
        if (animator == null) animator = GetComponentInChildren<Animator>();

        currentHealth = maxHealth;
        if (startFacingLeft) FaceLeft(); else FaceRight();
    }

    void Update()
    {
        if (isDead) return;
        if (player == null) return; // tránh NullReference nếu quên kéo player

        UpdateRangeState();

        if (inRange) HandleChaseAndAttack();
        else Patrol();
    }

    void UpdateRangeState()
    {
        // tối ưu: không dùng sqrt
        float sqr = (player.position - transform.position).sqrMagnitude;
        inRange = sqr < attackRange * attackRange;
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
        animator.SetBool(Attack1, false);
        transform.Translate(Vector2.right * moveSpeed * Time.deltaTime); // local space

        if (!IsGroundAhead())
            Flip();
    }

    void ChasePlayer()
    {
        animator.SetBool(Attack1, false);

        if (!IsGroundAhead())
            return;

        transform.position = Vector2.MoveTowards(
            transform.position,
            player.position,
            chaseSpeed * Time.deltaTime
        );
    }

    void FlipToPlayer()
    {
        if (player.position.x < transform.position.x) FaceLeft();
        else if (player.position.x > transform.position.x) FaceRight();
    }

    void Flip()
    {
        if (facingLeft) FaceRight();
        else FaceLeft();
    }

    void FaceLeft()
    {
        transform.eulerAngles = new Vector3(0, 180, 0);
        facingLeft = true;
    }

    void FaceRight()
    {
        transform.eulerAngles = new Vector3(0, 0, 0);
        facingLeft = false;
    }

    bool IsGroundAhead()
    {
        if (checkPoint == null) return true;

        RaycastHit2D hit = Physics2D.Raycast(
            checkPoint.position,
            Vector2.down,
            distance,
            layerMask
        );
        return hit.collider != null;
    }

    void Attack()
    {
        isAttacking = true;
        lastAttackTime = Time.time;

        animator.SetBool(Attack1, true);

        Invoke(nameof(DealDamage), hitDelay);
        Invoke(nameof(EndAttack), endAttackDelay);
    }

    void DealDamage()
    {
        if (isDead) return;
        if (attackPoint == null) return;

        // DÙNG ĐÚNG giá trị config trong Inspector: attackPoint/attackRadius/attackLayer
        Collider2D hit = Physics2D.OverlapCircle(attackPoint.position, attackRadius, attackLayer);
        if (hit != null && hit.TryGetComponent(out HeroKnight p))
        {
            p.TakeDamage(attackDamage);
        }
    }

    void EndAttack()
    {
        isAttacking = false;
        animator.SetBool(Attack1, false);
    }

    public void TakeDamage(int damage)
    {
        if (isDead) return;

        currentHealth -= damage;

        if (animator != null)
            animator.SetTrigger(Hurt);

        if (currentHealth <= 0)
            Die();
    }

    void Die()
    {
        isDead = true;

        animator.SetBool(Attack1, false);

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.gravityScale = 0;
        }

        if (col != null)
            col.enabled = false;

        if (animator != null)
            animator.SetTrigger(Death);

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