using System.Collections;
using UnityEngine;

public class Crab : MonoBehaviour
{
    [Header("Patrol Settings")]
    public float patrolRange = 5f;
    public float moveSpeed = 2f;
    public float idleWaitTime = 1.5f;

    [Header("Detection Settings")]
    public float detectionRange = 4f;
    public float attackRange = 1.2f;

    [Header("Combat Settings")]
    public float attackCooldown = 1.5f;
    public float abilityCooldown = 8f;
    public int maxHealth = 60;
    public int attackDamage = 10;
    public int attack2Damage = 13;
    public int attack3Damage = 15;
    public int abilityDamage = 25;

    [Header("Drop Settings")]
    public GameObject coinPrefab;
    public int goldDropAmount = 3;

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
    private bool isAttacking = false;   // lock: đang trong 1 attack coroutine

    private Vector3 startPosition;
    private float patrolTarget;
    private bool facingRight = true;

    private float attackTimer = 0f;
    private float abilityTimer = 0f;
    private int attackComboIndex = 0;

    // Tách riêng coroutine – attackCoroutine KHÔNG bao giờ bị stop từ bên ngoài
    private Coroutine patrolCoroutine;
    private Coroutine attackCoroutine;

    // Animator hashes
    private static readonly int AnimRun = Animator.StringToHash("Run");
    private static readonly int AnimHit = Animator.StringToHash("Hit");
    private static readonly int AnimDeath = Animator.StringToHash("Death");
    private static readonly int AnimAttack = Animator.StringToHash("Attack");
    private static readonly int AnimAttack2 = Animator.StringToHash("Attack 2");
    private static readonly int AnimAttack3 = Animator.StringToHash("Attack 3");
    private static readonly int AnimAbility = Animator.StringToHash("Ability");

    // ─────────────────────────────────────────────
    void Start()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        currentHealth = maxHealth;
        startPosition = transform.position;

        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p) player = p.transform;
        }

        SetNewPatrolTarget();
        patrolCoroutine = StartCoroutine(PatrolRoutine());
    }

    void Update()
    {
        if (isDead) return;

        attackTimer += Time.deltaTime;
        abilityTimer += Time.deltaTime;

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
            if (patrolCoroutine == null)
            {
                SetNewPatrolTarget();
                patrolCoroutine = StartCoroutine(PatrolRoutine());
            }
        }
    }

    // ─────────────────────────────────────────────
    //  PATROL
    // ─────────────────────────────────────────────
    IEnumerator PatrolRoutine()
    {
        while (!isDead)
        {
            animator.SetBool(AnimRun, true);

            while (!isDead && Mathf.Abs(transform.position.x - patrolTarget) > 0.1f)
            {
                MoveTowards(patrolTarget);
                yield return null;
            }

            rb.linearVelocity = Vector2.zero;
            animator.SetBool(AnimRun, false);
            yield return new WaitForSeconds(idleWaitTime);

            SetNewPatrolTarget();
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
    void StopPatrol()
    {
        if (patrolCoroutine != null)
        {
            StopCoroutine(patrolCoroutine);
            patrolCoroutine = null;
        }
        rb.linearVelocity = Vector2.zero;
        animator.SetBool(AnimRun, false);
    }

    void SetNewPatrolTarget()
    {
        float dir = (transform.position.x >= startPosition.x) ? -1f : 1f;
        patrolTarget = startPosition.x + dir * patrolRange;
    }

    void MoveTowards(float targetX)
    {
        float dir = targetX > transform.position.x ? 1f : -1f;
        rb.linearVelocity = new Vector2(dir * moveSpeed, rb.linearVelocity.y);
        FaceDirection(dir > 0);
    }

    // ─────────────────────────────────────────────
    //  CHASE & ATTACK
    // ─────────────────────────────────────────────
    void ChaseAndAttack(float dist)
    {
        if (dist <= attackRange)
        {
            rb.linearVelocity = Vector2.zero;
            animator.SetBool(AnimRun, false);

            if (!isAttacking)
            {
                if (abilityTimer >= abilityCooldown)
                    attackCoroutine = StartCoroutine(UseAbility());
                else if (attackTimer >= attackCooldown)
                    attackCoroutine = StartCoroutine(DoAttackCombo());
            }
        }
        else
        {
            animator.SetBool(AnimRun, true);
            float dir = player.position.x > transform.position.x ? 1f : -1f;
            rb.linearVelocity = new Vector2(dir * moveSpeed * 1.4f, rb.linearVelocity.y);
            FaceDirection(dir > 0);
        }
    }

    // ─────────────────────────────────────────────
    //  ATTACK COROUTINES
    //  !! KHÔNG StopCoroutine các hàm này từ bên ngoài !!
    // ─────────────────────────────────────────────
    IEnumerator DoAttackCombo()
    {
        isAttacking = true;
        attackTimer = 0f;
        rb.linearVelocity = Vector2.zero;

        float delay;
        int dmg;

        switch (attackComboIndex % 3)
        {
            case 0:
                animator.SetTrigger(AnimAttack);
                delay = 0.4f; dmg = attackDamage;
                break;
            case 1:
                animator.SetTrigger(AnimAttack2);
                delay = 0.45f; dmg = attack2Damage;
                break;
            default:
                animator.SetTrigger(AnimAttack3);
                delay = 0.5f; dmg = attack3Damage;
                break;
        }

        attackComboIndex++;

        yield return new WaitForSeconds(delay);
        DealDamage(dmg);            // ← guaranteed được gọi

        yield return new WaitForSeconds(0.5f);
        isAttacking = false;
    }

    IEnumerator UseAbility()
    {
        isAttacking = true;
        abilityTimer = 0f;
        attackTimer = 0f;
        rb.linearVelocity = Vector2.zero;

        animator.SetTrigger(AnimAbility);

        yield return new WaitForSeconds(0.6f);
        DealDamage(abilityDamage);  // ← guaranteed được gọi

        yield return new WaitForSeconds(0.6f);
        isAttacking = false;
    }

    // ─────────────────────────────────────────────
    //  DEAL DAMAGE
    // ─────────────────────────────────────────────
    void DealDamage(int damage)
    {
        if (player == null) return;

        float dist = Vector2.Distance(transform.position, player.position);

        if (dist <= attackRange + 0.5f)
        {
            HeroKnight ph = player.GetComponent<HeroKnight>();
            if (ph != null)
            {
                ph.TakeDamage(damage);

            }
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
        StopPatrol();           // chỉ dừng patrol
        // KHÔNG stop attackCoroutine

        isHit = true;
        rb.linearVelocity = Vector2.zero;
        animator.SetTrigger(AnimHit);
        yield return new WaitForSeconds(0.4f);
        isHit = false;
    }

    IEnumerator DieRoutine()
    {
        isDead = true;
        StopPatrol();

        rb.linearVelocity = Vector2.zero;
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
        Vector3 s = transform.localScale;
        s.x *= -1;
        transform.localScale = s;
    }

    void OnDrawGizmosSelected()
    {
        Vector3 origin = Application.isPlaying ? startPosition : transform.position;

        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(origin + Vector3.left * patrolRange, origin + Vector3.right * patrolRange);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}