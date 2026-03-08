using UnityEngine;

public class PatrolEnemy : MonoBehaviour
{
    [Header("Stats")]
    [SerializeField] private int maxHealth = 5;
    [SerializeField] private float moveSpeed = 1.5f;
    [SerializeField] private float chaseSpeed = 2f;

    [Header("Detection")]
    [SerializeField] private Transform player;
    [SerializeField] private float detectRange = 10f;
    [SerializeField] private float attackRange = 2.5f;
    [SerializeField] private LayerMask heroLayer;

    [Header("Ground Check")]
    [SerializeField] private Transform checkPoint;
    [SerializeField] private float groundDistance = 1f;
    [SerializeField] private LayerMask groundLayer;

    [Header("Wall Check")]
    public Transform wallCheck;
    public float wallDistance = 0.5f;
    public LayerMask wallLayer;

    [Header("Attack")]
    [SerializeField] private Transform attackPoint;
    [SerializeField] private float attackRadius = 1f;
    [SerializeField] private LayerMask attackLayer;
    [SerializeField] private float attackCooldown = 1.5f;
    [SerializeField] private int attackDamage = 3;

    [Header("Drop")]
    [SerializeField] private GameObject coinPrefab;
    [SerializeField] private int goldDropAmount = 3;

    [Header("Animation")]
    [SerializeField] private Animator animator;

    private Rigidbody2D rb;
    private Collider2D col;

    private bool facingLeft = true;
    private bool isDead;
    private bool isAttacking;

    private int currentHealth;
    private float lastAttackTime;

    private static readonly int Attack = Animator.StringToHash("Attack 1");
    private static readonly int Hurt = Animator.StringToHash("Hurt");
    private static readonly int Death = Animator.StringToHash("Death");

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();

        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        currentHealth = maxHealth;
    }

    void Update()
    {
        if (isDead || player == null) return;

        float distance = Vector2.Distance(transform.position, player.position);

        if (distance < attackRange)
        {
            TryAttack();
        }
        else if (DetectHero())
        {
            Chase();
        }
        else
        {
            Patrol();
        }
    }

    void Patrol()
    {
        if (!IsGroundAhead() || IsWallAhead())
        {
            Flip();
            return;
        }

        Move(moveSpeed);
    }

    void Chase()
    {
        FlipToPlayer();

        if (!IsGroundAhead() || IsWallAhead())
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            return;
        }

        Move(chaseSpeed);
    }

    void Move(float speed)
    {
        if (isAttacking) return;

        float direction = facingLeft ? -1 : 1;

        rb.linearVelocity = new Vector2(direction * speed, rb.linearVelocity.y);
    }

    void TryAttack()
    {
        if (Time.time < lastAttackTime + attackCooldown) return;

        lastAttackTime = Time.time;
        isAttacking = true;

        rb.linearVelocity = Vector2.zero;

        animator.SetTrigger(Attack);

        Invoke(nameof(DealDamage), 0.4f);
        Invoke(nameof(ResetAttack), 0.8f);
    }

    void ResetAttack()
    {
        isAttacking = false;
    }

    void DealDamage()
    {
        Collider2D hit = Physics2D.OverlapCircle(
            attackPoint.position,
            attackRadius,
            attackLayer
        );

        if (hit != null && hit.TryGetComponent(out HeroKnight hero))
        {
            hero.TakeDamage(attackDamage);
        }
    }

    bool IsGroundAhead()
    {
        float offset = 0.3f;

        Vector2 dir = facingLeft ? Vector2.left : Vector2.right;

        Vector2 origin1 = checkPoint.position;
        Vector2 origin2 = checkPoint.position + (Vector3)(dir * offset);
        Vector2 origin3 = checkPoint.position + (Vector3)(dir * offset * 2);

        RaycastHit2D hit1 = Physics2D.Raycast(origin1, Vector2.down, groundDistance, groundLayer);
        RaycastHit2D hit2 = Physics2D.Raycast(origin2, Vector2.down, groundDistance, groundLayer);
        RaycastHit2D hit3 = Physics2D.Raycast(origin3, Vector2.down, groundDistance, groundLayer);

        Debug.DrawRay(origin1, Vector2.down * groundDistance, Color.red);
        Debug.DrawRay(origin2, Vector2.down * groundDistance, Color.yellow);
        Debug.DrawRay(origin3, Vector2.down * groundDistance, Color.blue);

        return hit1.collider != null && hit2.collider != null && hit3.collider != null;
    }

    bool IsWallAhead()
    {
        Vector2 direction = facingLeft ? Vector2.left : Vector2.right;

        RaycastHit2D hit = Physics2D.Raycast(
            wallCheck.position,
            direction,
            wallDistance,
            wallLayer
        );

        Debug.DrawRay(wallCheck.position, direction * wallDistance, Color.yellow);

        return hit.collider != null;
    }

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

    public void TakeDamage(int damage)
    {
        if (isDead) return;

        currentHealth -= damage;

        animator.SetTrigger(Hurt);

        if (currentHealth <= 0)
            Die();
    }

    void Die()
    {
        isDead = true;

        rb.linearVelocity = Vector2.zero;
        rb.gravityScale = 0;

        col.enabled = false;

        animator.SetTrigger(Death);

        DropGold();

        Destroy(gameObject, 2f);
    }

    void DropGold()
    {
        if (coinPrefab == null) return;

        for (int i = 0; i < goldDropAmount; i++)
        {
            Vector2 offset = new Vector2(
                Random.Range(-0.5f, 0.5f),
                Random.Range(0, 0.5f)
            );

            Instantiate(
                coinPrefab,
                (Vector2)transform.position + offset,
                Quaternion.identity
            );
        }
    }

    bool DetectHero()
    {
        Vector2 dir = facingLeft ? Vector2.left : Vector2.right;

        Vector2 origin1 = wallCheck.position;
        Vector2 origin2 = wallCheck.position + Vector3.up * 0.5f;

        RaycastHit2D hit1 = Physics2D.Raycast(origin1, dir, detectRange, heroLayer);
        RaycastHit2D hit2 = Physics2D.Raycast(origin2, dir, detectRange, heroLayer);

        Debug.DrawRay(origin1, dir * detectRange, Color.green);
        Debug.DrawRay(origin2, dir * detectRange, Color.cyan);

        return hit1.collider != null || hit2.collider != null;
    }

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