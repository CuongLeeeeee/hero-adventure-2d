using System.Collections;
using UnityEngine;

public class PatrolEnemy : MonoBehaviour
{
    private enum EnemyState
    {
        Patrol,
        Chase,
        Attack,
        Hurt,
        Dead
    }

    [Header("Stats")]
    [SerializeField] private int maxHealth = 5;
    [SerializeField] private float moveSpeed = 1.5f;
    [SerializeField] private float chaseSpeed = 2f;
        
    [Header("Detection")]
    [SerializeField] private Transform player;
    [SerializeField] private float detectRange = 8f;
    [SerializeField] private float attackRange = 1.5f;
    [SerializeField] private LayerMask heroLayer;

    [Header("Ground Check")]
    [SerializeField] private Transform checkPoint;
    [SerializeField] private float groundDistance = 0.8f;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float groundRayOffset = 0.25f;

    [Header("Wall Check")]
    [SerializeField] private Transform wallCheck;
    [SerializeField] private float wallDistance = 0.4f;
    [SerializeField] private LayerMask wallLayer;
    [SerializeField] private float wallRayHeight = 0.5f;

    [Header("Attack")]
    [SerializeField] private Transform attackPoint;
    [SerializeField] private float attackRadius = 1f;
    [SerializeField] private LayerMask attackLayer;
    [SerializeField] private float attackCooldown = 1.5f;
    [SerializeField] private int attackDamage = 3;

    [Header("Hurt")]
    [SerializeField] private float hurtStunTime = 0.3f;

    [Header("Respawn")]
    [SerializeField] private float respawnDelay = 5f;

    [Header("Drop")]
    [SerializeField] private GameObject coinPrefab;
    [SerializeField] private int goldDropAmount = 3;

    [Header("Animation")]
    [SerializeField] private Animator animator;

    private Rigidbody2D rb;
    private Collider2D col;
    private SpriteRenderer sr;

    private bool facingLeft = true;
    private bool isAttacking;
    private bool isDead;
    private bool isHurting;

    private int currentHealth;
    private float lastAttackTime;

    private Vector3 spawnPosition;
    private Vector3 spawnScale;
    private float originalGravity;

    private EnemyState currentState;

    private static readonly int Attack = Animator.StringToHash("Attack 1");
    private static readonly int Hurt = Animator.StringToHash("Hurt");
    private static readonly int Death = Animator.StringToHash("Death");

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
        sr = GetComponentInChildren<SpriteRenderer>();

        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        currentHealth = maxHealth;
        spawnPosition = transform.position;
        spawnScale = transform.localScale;
        originalGravity = rb.gravityScale;

        currentState = EnemyState.Patrol;
    }

    void Update()
    {
        if (isDead || isHurting || player == null)
            return;

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        if (distanceToPlayer <= attackRange)
        {
            currentState = EnemyState.Attack;
            TryAttack();
            return;
        }

        if (DetectHero())
        {
            currentState = EnemyState.Chase;
            Chase();
            return;
        }

        currentState = EnemyState.Patrol;
        Patrol();
    }

    void Patrol()
    {
        if (isAttacking) return;

        if (!IsGroundAhead() || IsWallAhead())
        {
            StopMove();
            Flip();
            return;
        }

        Move(moveSpeed);
    }

    void Chase()
    {
        if (isAttacking) return;

        FacePlayer();

        if (!IsGroundAhead() || IsWallAhead())
        {
            StopMove();
            return;
        }

        Move(chaseSpeed);
    }

    void Move(float speed)
    {
        float direction = facingLeft ? -1f : 1f;
        rb.linearVelocity = new Vector2(direction * speed, rb.linearVelocity.y);
    }

    void StopMove()
    {
        rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
    }

    void TryAttack()
    {
        if (isAttacking) return;
        if (Time.time < lastAttackTime + attackCooldown) return;

        lastAttackTime = Time.time;
        isAttacking = true;

        StopMove();

        if (animator != null)
            animator.SetTrigger(Attack);

        StartCoroutine(AttackRoutine());
    }

    IEnumerator AttackRoutine()
    {
        yield return new WaitForSeconds(0.25f);
        DealDamage();

        yield return new WaitForSeconds(0.45f);
        isAttacking = false;
    }

    void DealDamage()
    {
        if (attackPoint == null) return;

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

    public void TakeDamage(int damage)
    {
        if (isDead) return;

        currentHealth -= damage;

        if (currentHealth <= 0)
        {
            Die();
            return;
        }

        StartCoroutine(HurtRoutine());
    }

    IEnumerator HurtRoutine()
    {
        isHurting = true;
        currentState = EnemyState.Hurt;

        StopMove();

        if (animator != null)
            animator.SetTrigger(Hurt);

        yield return new WaitForSeconds(hurtStunTime);

        isHurting = false;
    }

    void Die()
    {
        if (isDead) return;

        isDead = true;
        currentState = EnemyState.Dead;
        isAttacking = false;
        isHurting = false;

        StopMove();
        rb.gravityScale = 0f;
        col.enabled = false;

        if (animator != null)
            animator.SetTrigger(Death);

        DropGold();
        StartCoroutine(RespawnRoutine());
    }

    IEnumerator RespawnRoutine()
    {
        yield return new WaitForSeconds(respawnDelay);

        transform.position = spawnPosition;
        transform.localScale = spawnScale;

        currentHealth = maxHealth;
        isDead = false;
        isAttacking = false;
        isHurting = false;
        currentState = EnemyState.Patrol;

        rb.gravityScale = originalGravity;
        rb.linearVelocity = Vector2.zero;
        col.enabled = true;

        if (animator != null)
        {
            animator.Rebind();
            animator.Update(0f);
        }
    }

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

    bool DetectHero()
    {
        Vector2 dir = facingLeft ? Vector2.left : Vector2.right;

        Vector2 origin1 = wallCheck != null ? (Vector2)wallCheck.position : (Vector2)transform.position;
        Vector2 origin2 = origin1 + Vector2.up * wallRayHeight;

        RaycastHit2D hit1 = Physics2D.Raycast(origin1, dir, detectRange, heroLayer);
        RaycastHit2D hit2 = Physics2D.Raycast(origin2, dir, detectRange, heroLayer);

        Debug.DrawRay(origin1, dir * detectRange, Color.green);
        Debug.DrawRay(origin2, dir * detectRange, Color.cyan);

        return hit1.collider != null || hit2.collider != null;
    }

    bool IsGroundAhead()
    {
        if (checkPoint == null) return false;

        Vector2 dir = facingLeft ? Vector2.left : Vector2.right;

        Vector2 origin1 = checkPoint.position;
        Vector2 origin2 = (Vector2)checkPoint.position + dir * groundRayOffset;
        Vector2 origin3 = (Vector2)checkPoint.position + dir * (groundRayOffset * 2f);

        RaycastHit2D hit1 = Physics2D.Raycast(origin1, Vector2.down, groundDistance, groundLayer);
        RaycastHit2D hit2 = Physics2D.Raycast(origin2, Vector2.down, groundDistance, groundLayer);
        RaycastHit2D hit3 = Physics2D.Raycast(origin3, Vector2.down, groundDistance, groundLayer);

        Debug.DrawRay(origin1, Vector2.down * groundDistance, Color.red);
        Debug.DrawRay(origin2, Vector2.down * groundDistance, Color.yellow);
        Debug.DrawRay(origin3, Vector2.down * groundDistance, Color.blue);

        return hit1.collider != null || hit2.collider != null || hit3.collider != null;
    }

    bool IsWallAhead()
    {
        if (wallCheck == null) return false;

        Vector2 direction = facingLeft ? Vector2.left : Vector2.right;

        RaycastHit2D hit = Physics2D.Raycast(
            wallCheck.position,
            direction,
            wallDistance,
            wallLayer
        );

        Debug.DrawRay(wallCheck.position, direction * wallDistance, Color.magenta);

        return hit.collider != null;
    }

    void FacePlayer()
    {
        if (player.position.x < transform.position.x && !facingLeft)
            Flip();

        else if (player.position.x > transform.position.x && facingLeft)
            Flip();
    }

    void Flip()
    {
        facingLeft = !facingLeft;

        Vector3 scale = transform.localScale;
        scale.x *= -1f;
        transform.localScale = scale;
    }

    void OnDrawGizmosSelected()
    {
        if (attackPoint != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(attackPoint.position, attackRadius);
        }

        if (checkPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawRay(checkPoint.position, Vector2.down * groundDistance);
        }

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, detectRange);
    }
}