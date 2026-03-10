using System.Collections;
using UnityEngine;

public class Bat : MonoBehaviour
{
    [Header("Patrol Settings")]
    public float patrolRadius = 2f;         // Bán kính vòng tròn bay
    public float patrolSpeed = 2f;         // Tốc độ bay tuần tra
    public bool clockwise = true;       // Chiều bay

    [Header("Detection Settings")]
    public float detectionRange = 2f;
    public float attackRange = 1.0f;

    [Header("Combat Settings")]
    public float attackCooldown = 1.5f;
    public int maxHealth = 40;
    public int attackDamage = 8;

    [Header("Drop Settings")]
    public GameObject coinPrefab;
    public int goldDropAmount = 2;

    [Header("UI")]
    public EnemyHealthBar healthBar;

    [Header("References")]
    public Transform player;

    // ── private ──────────────────────────────────
    private Animator animator;
    private Rigidbody2D rb;
    private int currentHealth;

    private bool isDead = false;
    private bool isHit = false;
    private bool isAttacking = false;

    private Vector3 centerPoint;       // Tâm vòng tròn tuần tra
    private float patrolAngle = 0f;  // Góc hiện tại trên vòng tròn
    private bool facingRight = true;

    private float attackTimer = 0f;

    private Coroutine patrolCoroutine;
    private Coroutine attackCoroutine;

    // Animator hashes – đặt tên đúng với Animator của Bat
    private static readonly int AnimFly = Animator.StringToHash("Run");     // bool
    private static readonly int AnimHit = Animator.StringToHash("Hit");     // trigger
    private static readonly int AnimDeath = Animator.StringToHash("Death");   // trigger
    private static readonly int AnimAttack = Animator.StringToHash("Attack");  // trigger

    // ─────────────────────────────────────────────
    void Start()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        currentHealth = maxHealth;
        centerPoint = transform.position;

        // Tính góc ban đầu từ vị trí hiện tại so với tâm
        patrolAngle = Mathf.Atan2(
            transform.position.y - centerPoint.y,
            transform.position.x - centerPoint.x
        );

        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p) player = p.transform;
        }

        // Bat không bị ảnh hưởng bởi gravity
        if (rb != null) rb.gravityScale = 0f;

        patrolCoroutine = StartCoroutine(PatrolRoutine());
    }

    void Update()
    {
        if (isDead) return;

        attackTimer += Time.deltaTime;

        if (isAttacking || isHit) return;

        float dist = player != null
            ? Vector2.Distance(transform.position, player.position)
            : Mathf.Infinity;

        if (dist <= detectionRange)
        {
            StopPatrol();
            ChaseAndAttack(dist);
        }
        else
        {
            animator.SetBool(AnimFly, true);
            if (patrolCoroutine == null)
                patrolCoroutine = StartCoroutine(PatrolRoutine());
        }
    }

    // ─────────────────────────────────────────────
    //  PATROL – bay theo vòng tròn
    // ─────────────────────────────────────────────
    IEnumerator PatrolRoutine()
    {
        animator.SetBool(AnimFly, true);

        while (!isDead)
        {
            float sign = clockwise ? -1f : 1f;
            float angularSpeed = (patrolSpeed / patrolRadius) * sign;

            patrolAngle += angularSpeed * Time.deltaTime;

            Vector3 target = centerPoint + new Vector3(
                Mathf.Cos(patrolAngle) * patrolRadius,
                Mathf.Sin(patrolAngle) * patrolRadius,
                0f
            );

            // Di chuyển mượt về vị trí trên vòng tròn
            transform.position = Vector3.MoveTowards(
                transform.position, target, patrolSpeed * Time.deltaTime
            );

            // Mặt theo hướng di chuyển
            float dx = target.x - transform.position.x;
            if (Mathf.Abs(dx) > 0.01f)
                FaceDirection(dx > 0);

            yield return null;
        }
    }

    void StopPatrol()
    {
        if (patrolCoroutine != null)
        {
            StopCoroutine(patrolCoroutine);
            patrolCoroutine = null;
        }
        if (rb != null) rb.linearVelocity = Vector2.zero;
    }

    // ─────────────────────────────────────────────
    //  CHASE & ATTACK
    // ─────────────────────────────────────────────
    void ChaseAndAttack(float dist)
    {
        if (dist <= attackRange)
        {
            if (rb != null) rb.linearVelocity = Vector2.zero;
            animator.SetBool(AnimFly, false);

            if (!isAttacking && attackTimer >= attackCooldown)
                attackCoroutine = StartCoroutine(DoAttack());
        }
        else
        {
            // Đuổi thẳng về phía player
            animator.SetBool(AnimFly, true);
            Vector2 dir = (player.position - transform.position).normalized;
            if (rb != null)
                rb.linearVelocity = dir * patrolSpeed * 1.5f;
            FaceDirection(dir.x > 0);
        }
    }

    // ─────────────────────────────────────────────
    //  ATTACK COROUTINE
    // ─────────────────────────────────────────────
    IEnumerator DoAttack()
    {
        isAttacking = true;
        attackTimer = 0f;
        if (rb != null) rb.linearVelocity = Vector2.zero;

        animator.SetTrigger(AnimAttack);

        yield return new WaitForSeconds(0.35f);
        DealDamage(attackDamage);

        yield return new WaitForSeconds(0.4f);
        isAttacking = false;
    }

    // ─────────────────────────────────────────────
    //  DEAL DAMAGE
    // ─────────────────────────────────────────────
    void DealDamage(int damage)
    {
        if (player == null) return;

        float dist = Vector2.Distance(transform.position, player.position);
        if (dist <= attackRange + 0.4f)
        {
            HeroKnight ph = player.GetComponent<HeroKnight>();
            if (ph != null)
                ph.TakeDamage(damage);
        }
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
    // ─────────────────────────────────────────────
    //  TAKE DAMAGE / DEATH
    // ─────────────────────────────────────────────
    public void TakeDamage(int damage)
    {
        if (isDead) return;
        currentHealth -= damage;

        if (healthBar != null)
            healthBar.SetHealth(currentHealth, maxHealth);

        if (currentHealth <= 0)
            StartCoroutine(DieRoutine());
        else
            StartCoroutine(HitRoutine());
    }

    IEnumerator HitRoutine()
    {
        StopPatrol();
        isHit = true;
        if (rb != null) rb.linearVelocity = Vector2.zero;
        animator.SetTrigger(AnimHit);

        yield return new WaitForSeconds(0.35f);
        isHit = false;
    }

    IEnumerator DieRoutine()
    {
        isDead = true;
        StopPatrol();
        if (rb != null) rb.linearVelocity = Vector2.zero;

        animator.SetTrigger(AnimDeath);
        DropGold();
        var col = GetComponent<Collider2D>();
        if (col) col.enabled = false;

        yield return new WaitForSeconds(1.5f);
        Destroy(gameObject);
    }

    // ─────────────────────────────────────────────
    //  HELPERS
    // ─────────────────────────────────────────────
    void FaceDirection(bool right)
    {
        if (facingRight == right) return;
        facingRight = right;
        transform.eulerAngles = new Vector3(0f, right ? 0f : 180f, 0f);
    }

    void OnDrawGizmosSelected()
    {
        Vector3 origin = Application.isPlaying ? centerPoint : transform.position;

        // Vòng tuần tra
        Gizmos.color = Color.cyan;
        DrawCircle(origin, patrolRadius);

        // Tầm phát hiện
        Gizmos.color = Color.yellow;
        DrawCircle(transform.position, detectionRange);

        // Tầm tấn công
        Gizmos.color = Color.red;
        DrawCircle(transform.position, attackRange);
    }

    void DrawCircle(Vector3 center, float radius)
    {
        int segments = 36;
        float step = 360f / segments;
        for (int i = 0; i < segments; i++)
        {
            float a1 = i * step * Mathf.Deg2Rad;
            float a2 = (i + 1) * step * Mathf.Deg2Rad;
            Gizmos.DrawLine(
                center + new Vector3(Mathf.Cos(a1), Mathf.Sin(a1)) * radius,
                center + new Vector3(Mathf.Cos(a2), Mathf.Sin(a2)) * radius
            );
        }
    }
}