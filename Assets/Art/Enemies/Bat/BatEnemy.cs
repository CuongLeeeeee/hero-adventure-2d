using UnityEngine;

public class BatEnemy : MonoBehaviour
{
    [Header("Drop Settings")]
    public GameObject coinPrefab;
    public int goldDropAmount = 4;

    [Header("Movement")]
    public float flySpeed = 1.5f;         
    public float flyRadius = 1.6f;       
    public float chaseSpeed = 2f;      

    [Header("Combat")]
    public float detectRange = 3f;    
    public float attackRange = 2.5f;      
    public float attackCooldown = 1.5f; 
    public int attackDamage = 1;       
    public int health = 2;              


    [Header("Flying Settings")]
    public float minHeightAbovePlayer = 1.5f;
    public float stopDistance = 1.5f;
    [Header("References")]
    public Animator animator;

    [Header("Sprite Settings")]
    public bool spriteDefaultFacingLeft = false;

    [Header("UI")]
    public EnemyHealthBar healthBar;

    private Transform player;
    private Vector3 startPosition;
    private float flyAngle = 0f;
    private float lastAttackTime;
    private int attackComboCount = 0;
    private bool isDead = false;
    private bool isAttacking = false;
    private int maxHealth = 2;
    void Start()
    {
        startPosition = transform.position;

        if (animator == null)
            animator = GetComponent<Animator>();

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;
    }

    void Update()
    {
        if (isDead || player == null) return;

        float distance = Vector2.Distance(transform.position, player.position);

        if (distance <= attackRange)
        {
            FacePlayer();
            HoverAbovePlayer();

            if (Time.time >= lastAttackTime + attackCooldown && !isAttacking)
                AttackPlayer();
        }
        else if (distance <= detectRange)
        {
            ChasePlayer();
        }
        else
        {
            FlyAround();
        }
    }

    void FlyAround()
    {
        flyAngle += flySpeed * Time.deltaTime;
        float x = startPosition.x + Mathf.Cos(flyAngle) * flyRadius;
        float y = startPosition.y + Mathf.Sin(flyAngle) * flyRadius * 0.5f;

        Vector3 target = new Vector3(x, y, 0);
        FaceDirection(target.x < transform.position.x ? -1 : 1);
        transform.position = Vector3.MoveTowards(transform.position, target, flySpeed * Time.deltaTime);
    }

    void ChasePlayer()
    {
        FacePlayer();

        Vector3 target = new Vector3(player.position.x, player.position.y + minHeightAbovePlayer, 0);

        float horizontalDist = Mathf.Abs(transform.position.x - player.position.x);
        if (horizontalDist <= stopDistance)
            target.x = transform.position.x;

        transform.position = Vector3.MoveTowards(transform.position, target, chaseSpeed * Time.deltaTime);
    }

    void HoverAbovePlayer()
    {
        float targetY = player.position.y + minHeightAbovePlayer;
        Vector3 pos = transform.position;
        pos.y = Mathf.MoveTowards(pos.y, targetY, chaseSpeed * Time.deltaTime);
        transform.position = pos;
    }

    void FacePlayer()
    {
        FaceDirection(player.position.x < transform.position.x ? -1 : 1);
    }

    void FaceDirection(int dir)
    {
        Vector3 scale = transform.localScale;
        scale.x = Mathf.Abs(scale.x) * (spriteDefaultFacingLeft ? -dir : dir);
        transform.localScale = scale;
    }

    void AttackPlayer()
    {
        isAttacking = true;
        lastAttackTime = Time.time;
        attackComboCount++;

        if (animator != null)
            animator.SetTrigger(attackComboCount % 2 == 1 ? "Attack1" : "Attack2");

        Invoke(nameof(DealDamage), 0.3f);
        Invoke(nameof(EndAttack), 0.5f);
    }

    void DealDamage()
    {
        if (isDead || player == null) return;

        if (Vector2.Distance(transform.position, player.position) <= attackRange * 1.3f)
        {
            HeroKnight p = player.GetComponent<HeroKnight>();
            if (p != null) p.TakeDamage(attackDamage);
        }
    }

    void EndAttack() => isAttacking = false;

    public void TakeDamage(int damage)
    {
        if (isDead) return;

        health -= damage;
        if (animator != null) animator.SetTrigger("Hurt");

        if (healthBar != null)
            healthBar.SetHealth(health, maxHealth);

        if (health <= 0) Die();
    }

    void Die()
    {
        isDead = true;
        if (animator != null) animator.SetTrigger("Die");

        var col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;
        DropGold();


        Destroy(gameObject, 1.5f);
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
        Vector3 center = Application.isPlaying ? startPosition : transform.position;

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(center, flyRadius);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
