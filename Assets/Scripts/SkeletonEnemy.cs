using UnityEngine;

public class SkeletonEnemy : MonoBehaviour
{
    [Header("Stats")]
    public int maxHealth = 5;
    public float moveSpeed = 2f;
    public float chaseSpeed = 4f;

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

    [Header("State")]
    public bool facingLeft = true;
    public Animator animator;

    void Start()
    {
        FaceLeft();
    }

    void Update()
    {
        if (maxHealth <= 0)
        {
            Die();
            return;
        }

        UpdateRangeState();

        if (inRange)
        {
            HandleChaseAndAttack();
        }
        else
        {
            Patrol();
        }
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
            animator.SetBool("Attack 1", true);
            Attack();
        }
    }

    void Patrol()
    {
        animator.SetBool("Attack 1", false);
        transform.Translate(Vector2.right * moveSpeed * Time.deltaTime);

        if (!IsGroundAhead())
        {
            Flip();
        }
    }

    // ===================== MOVEMENT =====================

    void ChasePlayer()
    {
        animator.SetBool("Attack 1", false);

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
        {
            FaceLeft();
        }
        else if (player.position.x > transform.position.x)
        {
            FaceRight();
        }
    }

    void Flip()
    {
        if (facingLeft)
            FaceRight();
        else
            FaceLeft();
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

    public void Attack()
    {
        Collider2D col = Physics2D.OverlapCircle(
            attackPoint.position,
            attackRadius,
            attackLayer
        );

        if (col != null && col.GetComponent<HeroKnight>() != null)
        {
            col.GetComponent<HeroKnight>().TakeDamage(3);
        }
    }

    public void TakeDamage(int damage)
    {
        if (maxHealth <= 0) return;
        maxHealth -= damage;
    }

    void Die()
    {
        Destroy(gameObject);
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
