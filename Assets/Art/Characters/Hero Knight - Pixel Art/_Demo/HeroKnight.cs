using UnityEngine;
using System.Collections;

public class HeroKnight : MonoBehaviour
{
    [SerializeField] float m_speed = 4.0f;
    [SerializeField] float m_jumpForce = 7.5f;
    [SerializeField] float m_rollForce = 6.0f;
    [SerializeField] bool m_noBlood = false;
    [SerializeField] GameObject m_slideDust;
    [SerializeField] public int m_gold = 100;
    [SerializeField] int maxHealth = 100;

    [Header("Combat")]
    [SerializeField] int m_attackDamage = 2;
    [SerializeField] float m_attackRange = 0.8f;
    [SerializeField] Vector2 m_attackOffset = new Vector2(1.0f, 0.2f);
    [SerializeField] LayerMask m_enemyLayer; // set Enemy layer trong Inspector

    private Animator m_animator;
    private Rigidbody2D m_body2d;
    private Sensor_HeroKnight m_groundSensor;
    private Sensor_HeroKnight m_wallSensorR1;
    private Sensor_HeroKnight m_wallSensorR2;
    private Sensor_HeroKnight m_wallSensorL1;
    private Sensor_HeroKnight m_wallSensorL2;
    private bool m_isWallSliding = false;
    private bool m_grounded = false;
    private bool m_rolling = false;
    private int m_facingDirection = 1;
    private int m_currentAttack = 0;
    private float m_timeSinceAttack = 0.0f;
    private float m_delayToIdle = 0.0f;
    private float m_rollDuration = 8.0f / 14.0f;
    private float m_rollCurrentTime;

    void Start()
    {
        m_animator = GetComponent<Animator>();
        m_body2d = GetComponent<Rigidbody2D>();
        m_groundSensor = transform.Find("GroundSensor").GetComponent<Sensor_HeroKnight>();
        m_wallSensorR1 = transform.Find("WallSensor_R1").GetComponent<Sensor_HeroKnight>();
        m_wallSensorR2 = transform.Find("WallSensor_R2").GetComponent<Sensor_HeroKnight>();
        m_wallSensorL1 = transform.Find("WallSensor_L1").GetComponent<Sensor_HeroKnight>();
        m_wallSensorL2 = transform.Find("WallSensor_L2").GetComponent<Sensor_HeroKnight>();
    }

    void Update()
    {
        m_timeSinceAttack += Time.deltaTime;

        if (m_rolling)
            m_rollCurrentTime += Time.deltaTime;
        if (m_rollCurrentTime > m_rollDuration)
            m_rolling = false;

        if (!m_grounded && m_groundSensor.State())
        {
            m_grounded = true;
            m_animator.SetBool("Grounded", m_grounded);
        }
        if (m_grounded && !m_groundSensor.State())
        {
            m_grounded = false;
            m_animator.SetBool("Grounded", m_grounded);
        }

        float inputX = Input.GetAxis("Horizontal");

        if (inputX > 0)
        {
            GetComponent<SpriteRenderer>().flipX = false;
            m_facingDirection = 1;
        }
        else if (inputX < 0)
        {
            GetComponent<SpriteRenderer>().flipX = true;
            m_facingDirection = -1;
        }

        if (!m_rolling)
            m_body2d.linearVelocity = new Vector2(inputX * m_speed, m_body2d.linearVelocity.y);

        m_animator.SetFloat("AirSpeedY", m_body2d.linearVelocity.y);

        m_isWallSliding = (m_wallSensorR1.State() && m_wallSensorR2.State()) || (m_wallSensorL1.State() && m_wallSensorL2.State());
        m_animator.SetBool("WallSlide", m_isWallSliding);

        if (Input.GetKeyDown("e") && !m_rolling)
        {
            m_animator.SetBool("noBlood", m_noBlood);
            m_animator.SetTrigger("Death");
        }
        else if (Input.GetKeyDown("q") && !m_rolling)
            m_animator.SetTrigger("Hurt");

        // Attack
        else if (Input.GetMouseButtonDown(0) && m_timeSinceAttack > 0.25f && !m_rolling)
        {
            m_currentAttack++;

            if (m_currentAttack > 3)
                m_currentAttack = 1;

            if (m_timeSinceAttack > 1.0f)
                m_currentAttack = 1;

            m_animator.SetTrigger("Attack" + m_currentAttack);
            m_timeSinceAttack = 0.0f;
        }

        else if (Input.GetMouseButtonDown(1) && !m_rolling)
        {
            m_animator.SetTrigger("Block");
            m_animator.SetBool("IdleBlock", true);
        }
        else if (Input.GetMouseButtonUp(1))
            m_animator.SetBool("IdleBlock", false);

        else if (Input.GetKeyDown(KeyCode.LeftShift) && !m_rolling && !m_isWallSliding)
        {
            m_rolling = true;
            m_rollCurrentTime = 0f;
            m_animator.SetTrigger("Roll");
            m_body2d.linearVelocity = new Vector2(m_facingDirection * m_rollForce, m_body2d.linearVelocity.y);
        }

        else if (Input.GetKeyDown(KeyCode.Space) && m_grounded && !m_rolling)
        {
            m_animator.SetTrigger("Jump");
            m_grounded = false;
            m_animator.SetBool("Grounded", m_grounded);
            m_body2d.linearVelocity = new Vector2(m_body2d.linearVelocity.x, m_jumpForce);
            m_groundSensor.Disable(0.2f);
        }

        else if (Mathf.Abs(inputX) > Mathf.Epsilon)
        {
            m_delayToIdle = 0.05f;
            m_animator.SetInteger("AnimState", 1);
        }
        else
        {
            m_delayToIdle -= Time.deltaTime;
            if (m_delayToIdle < 0)
                m_animator.SetInteger("AnimState", 0);
        }
    }

    // ====== PLAYER HIT ======
    public void AE_AttackHit()
    {
        Vector2 center = (Vector2)transform.position + new Vector2(m_attackOffset.x * m_facingDirection, m_attackOffset.y);
        Collider2D[] hits = Physics2D.OverlapCircleAll(center, m_attackRange, m_enemyLayer);

        foreach (var h in hits)
        {
            h.GetComponent<GolemEnemy>()?.TakeDamage(m_attackDamage);
            h.GetComponent<BatEnemy>()?.TakeDamage(m_attackDamage);
            h.GetComponent<SkeletonEnemy>()?.TakeDamage(m_attackDamage);
        }
    }

    void AE_SlideDust()
    {
        Vector3 spawnPosition;
        if (m_facingDirection == 1)
            spawnPosition = m_wallSensorR2.transform.position;
        else
            spawnPosition = m_wallSensorL2.transform.position;

        if (m_slideDust != null)
        {
            GameObject dust = Instantiate(m_slideDust, spawnPosition, gameObject.transform.localRotation) as GameObject;
            dust.transform.localScale = new Vector3(m_facingDirection, 1, 1);
        }
    }

    public void TakeDamage(int damage)
    {
        if (maxHealth <= 0) return;

        maxHealth -= damage;
        Debug.Log("Hero Health: " + maxHealth);

        if (maxHealth <= 0)
            m_animator.SetTrigger("Death");
        else
            m_animator.SetTrigger("Hurt");
    }

    public bool SpendGold(int amount)
    {
        if (m_gold >= amount)
        {
            m_gold -= amount;
            Debug.Log("Đã trừ " + amount + " vàng. Còn lại: " + m_gold);
            return true;
        }
        Debug.Log("Không đủ tiền!");
        return false;
    }
    public void AddGold(int amount)
    {
        m_gold += amount;
        Debug.Log("Vàng hiện tại: " + m_gold);
    }

    // Debug hitbox
    void OnDrawGizmosSelected()
    {
        Vector2 center = (Vector2)transform.position +
                         new Vector2(m_attackOffset.x * m_facingDirection, m_attackOffset.y);
        Gizmos.DrawWireSphere(center, m_attackRange);
    }
}
