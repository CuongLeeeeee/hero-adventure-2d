using UnityEngine;

public class FireDemon : MonoBehaviour
{
    [Header("Stats")]
    public int maxHealth = 500;
    public float moveSpeed = 3f;
    public float chaseSpeed = 4f;

    [Header("Drop Settings")]
    public GameObject coinPrefab;
    public int goldDropAmount = 1;

    [Header("Detection")]
    public Transform player;
    public float attackRange = 10f;
    public float retreatDistance = 4f;
    public bool inRange;

    [Header("Ground Check")]
    public Transform checkPoint;
    public float distance = 1f;
    public LayerMask layerMask;

    [Header("Attack")]
    public float attackCooldown = 1.5f;
    public int attackDamage = 5;
    public int cleaveDamage = 36;
    private int attackCount = 60;

    [Header("State")]
    public bool facingLeft = false;
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

        inRange = Vector2.Distance(transform.position, player.position) < attackRange;

        if (inRange)
            HandleChaseAndAttack();
        else
            Patrol();
    }

    // ===================== STATES =====================

    void HandleChaseAndAttack()
    {
        FlipToPlayer();

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        if (distanceToPlayer > retreatDistance)
            ChasePlayer();
        else if (Time.time >= lastAttackTime + attackCooldown && !isAttacking)
            Attack();
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

        if (!IsGroundAhead()) return;

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
        transform.eulerAngles = Vector3.zero;
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
        RaycastHit2D hit = Physics2D.Raycast(checkPoint.position, Vector2.down, distance, layerMask);
        return hit.collider != null;
    }

    // ===================== COMBAT =====================

    void Attack()
    {
        isAttacking = true;
        lastAttackTime = Time.time;
        attackCount++;
        Debug.Log( "Attack Count: " + attackCount);
        if (attackCount >= 3)
        {
            attackCount = 0;
            animator.SetBool("Cleave", true);
            Invoke(nameof(DealDamageCleave), 1f);
            Debug.Log("Cleave ");
            Invoke(nameof(EndAttack), 2.5f);
        }
        else
        {
            animator.SetBool("Attack", true);
            Invoke(nameof(DealDamage), 0.5f);
            Debug.Log("Atack ");
            Invoke(nameof(EndAttack), 1.5f);
        }
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

    void DealDamageCleave()
    {
        if (isDead || player == null) return;

        if (Vector2.Distance(transform.position, player.position) <= retreatDistance * 1.3f)
        {
            HeroKnight p = player.GetComponent<HeroKnight>();
            if (p != null) p.TakeDamage(cleaveDamage);
        }
    }

    void EndAttack()
    {
        isAttacking = false;
        animator.SetBool("Attack", false);
        animator.SetBool("Cleave", false);
    }

    // ===================== TAKE DAMAGE =====================

    public void TakeDamage(int damage)
    {
        if (isDead) return;

        currentHealth -= damage;
        animator.SetTrigger("Hurt");

        if (healthBar != null)
            healthBar.SetHealth(currentHealth, maxHealth);

        if (currentHealth <= 0)
            Die();
    }

    // ===================== DIE =====================

    void Die()
    {
        isDead = true;
        animator.SetBool("Attack", false);

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.gravityScale = 0;
        }

        if (col != null) col.enabled = false;

        animator.SetTrigger("Die");
        DropGold();
        Destroy(gameObject, 2f);
    }

    void DropGold()
    {
        if (coinPrefab == null) return;

        for (int i = 0; i < goldDropAmount; i++)
        {
            Vector2 offset = new Vector2(Random.Range(-0.5f, 0.5f), Random.Range(0f, 0.5f));
            Instantiate(coinPrefab, (Vector2)transform.position + offset, Quaternion.identity);
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

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, retreatDistance);
    }
}