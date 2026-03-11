using Assets.Scripts;
using UnityEngine;
using System.Collections;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Unity.AppUI.UI;
using Unity.Mathematics.Geometry;

public class HeroKnight : MonoBehaviour
{

    [SerializeField] float m_speed = 6f;
    [SerializeField] float m_jumpForce = 13f;
    [SerializeField] float m_rollForce = 13f;
    [SerializeField] bool m_noBlood = false;
    [SerializeField] GameObject m_slideDust;
    [SerializeField] public int m_gold = 0;
    [SerializeField] int maxHealth = 150;
    [SerializeField] public int currentHealth;
    [Header("Combat")]
    [SerializeField] public int m_attackDamage = 15;
    [SerializeField] public int m_dartBonusDamage = 0;   // Damage cộng thêm cho phi tiêu
    [SerializeField] float m_attackRange = 0.8f;
    [SerializeField] Vector2 m_attackOffset = new Vector2(1.0f, 0.2f);
    [SerializeField] LayerMask m_enemyLayer; // set Enemy layer trong Inspector
    [SerializeField] float attackCooldown = 0.6f;
    [SerializeField] float rollCooldown = 2.0f;
    GameManager gameManager;

    public GameObject dartPrefab; // Kéo Prefab phi tiêu vào đây trong Inspector
    private float nextFireTime = 0f; // Thời điểm được phép bắn tiếp theo
    public float fireRate = 1f; // Khoảng thời gian chờ (1 giây)

    //Phi tiêu
    [Header("Ranged Combat")]
    [SerializeField] GameObject m_dartPrefab; // Kéo Prefab phi tiêu vào đây
    [SerializeField] Transform m_launchPoint; // Điểm xuất hiện phi tiêu (ví dụ ngay tay)
    public int m_dartCount = 0; // Số lượng phi tiêu hiện có

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
    float lastAttackTime = -10f;
    float lastRollTime = -10f;
    int m_pointValue = 0;
    public Image healthBar;
    public TextMeshProUGUI cointCount;
    public TimerCountUp timer;
    private bool isDead = false;

    private float m_nextDartTime = 0f;
    public float m_dartCooldown = 1.0f;


    void Start()
    {
        m_animator = GetComponent<Animator>();
        m_body2d = GetComponent<Rigidbody2D>();
        m_groundSensor = transform.Find("GroundSensor").GetComponent<Sensor_HeroKnight>();
        m_wallSensorR1 = transform.Find("WallSensor_R1").GetComponent<Sensor_HeroKnight>();
        m_wallSensorR2 = transform.Find("WallSensor_R2").GetComponent<Sensor_HeroKnight>();
        m_wallSensorL1 = transform.Find("WallSensor_L1").GetComponent<Sensor_HeroKnight>();
        m_wallSensorL2 = transform.Find("WallSensor_L2").GetComponent<Sensor_HeroKnight>();
        gameManager = FindObjectOfType<GameManager>();
        currentHealth = maxHealth;
        m_gold = 0;
        healthBar.fillAmount = (float)currentHealth / maxHealth;
        cointCount.text = "x "+m_gold.ToString();

        if (Assets.Scripts.GameHUDManager.Instance != null)
        {
            Assets.Scripts.GameHUDManager.Instance.UpdateDartCount(m_dartCount);
        }
    }

    void Update()
    {
        if (isDead) return;

        // --- 1. CẬP NHẬT THỜI GIAN & TRẠNG THÁI ---
        m_timeSinceAttack += Time.deltaTime;
        if (m_rolling)
            m_rollCurrentTime += Time.deltaTime;
        if (m_rollCurrentTime > m_rollDuration)
            m_rolling = false;

        // --- 2. KIỂM TRA GROUNDED ---
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

        // --- 3. DI CHUYỂN & LẬT MẶT ---
        float inputX = Input.GetAxis("Horizontal");
        if (inputX > 0)
        {
            transform.localScale = new Vector3(1f, transform.localScale.y, 1f);
            m_facingDirection = 1;
        }
        else if (inputX < 0)
        {
            transform.localScale = new Vector3(-1f, transform.localScale.y, 1f);
            m_facingDirection = -1;
        }

        if (!m_rolling)
            m_body2d.linearVelocity = new Vector2(inputX * m_speed, m_body2d.linearVelocity.y);

        m_animator.SetFloat("AirSpeedY", m_body2d.linearVelocity.y);

        // --- 4. CÁC HÀNH ĐỘNG CHIẾN ĐẤU (TÁCH BIỆT CÁC IF ĐỂ KHÔNG CHẶN NHAU) ---

        // Tấn công bằng chuột trái
        if (Input.GetMouseButtonDown(0) && m_timeSinceAttack > attackCooldown && !m_rolling)
        {
            AudioManager.instance.PlaySFX(AudioManager.instance.attack);
            m_currentAttack++;
            if (m_currentAttack > 3) m_currentAttack = 1;
            if (m_timeSinceAttack > 1.0f) m_currentAttack = 1;

            m_animator.SetTrigger("Attack" + m_currentAttack);
            m_timeSinceAttack = 0.0f;
        }

        // Đỡ đòn bằng chuột phải
        if (Input.GetMouseButtonDown(1) && !m_rolling)
        {
            m_animator.SetTrigger("Block");
            m_animator.SetBool("IdleBlock", true);
        }
        else if (Input.GetMouseButtonUp(1))
        {
            m_animator.SetBool("IdleBlock", false);
        }

        // Phóng phi tiêu bằng phím S (ĐÃ THÊM COOLDOWN 1s)
        if (Input.GetKeyDown(KeyCode.S) && m_dartCount > 0 && !m_rolling && Time.time >= m_nextDartTime)
        {
            LaunchDart();
            m_nextDartTime = Time.time + m_dartCooldown; // Thiết lập mốc 1 giây sau mới được bắn tiếp
        }

        // Lăn (Roll)
        if (Input.GetKeyDown(KeyCode.LeftShift) && Time.time >= lastRollTime +rollCooldown && !m_rolling && !m_isWallSliding)
        {
            lastRollTime = Time.time;
            m_rolling = true;
            m_rollCurrentTime = 0f;
            m_animator.SetTrigger("Roll");
            m_body2d.linearVelocity = new Vector2(m_facingDirection * m_rollForce, m_body2d.linearVelocity.y);
        }

        // Nhảy (Jump)
        if (Input.GetKeyDown(KeyCode.Space) && m_grounded && !m_rolling)
        {
            m_animator.SetTrigger("Jump");
            m_grounded = false;
            m_animator.SetBool("Grounded", m_grounded);
            m_body2d.linearVelocity = new Vector2(m_body2d.linearVelocity.x, m_jumpForce);
            m_groundSensor.Disable(0.2f);
        }

        // --- 5. ANIMATION IDLE/RUN ---
        if (Mathf.Abs(inputX) > Mathf.Epsilon)
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

    void LaunchDart()
    {
        if (m_dartCount > 0)
        {
            m_dartCount--;
            Assets.Scripts.GameHUDManager.Instance.UpdateDartCount(m_dartCount);

            GameObject dartObj = Instantiate(m_dartPrefab, m_launchPoint.position, Quaternion.identity);

            // Truyền tổng damage (Damage gốc của phi tiêu + Bonus tích lũy được)
            Dart dartScript = dartObj.GetComponent<Dart>();
            if (dartScript != null)
            {
                // damageAmount là biến có sẵn trong script Dart của mày
                dartScript.damage += m_dartBonusDamage;
            }

            dartObj.transform.localScale = Vector3.one;
            dartScript.Launch(m_facingDirection);
        }
    }

    public void Launch(int direction)
    {
        GetComponent<Rigidbody2D>().linearVelocity = new Vector2(direction * m_speed, 0);
        transform.localScale = new Vector3(direction, 1, 1);
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
            h.GetComponent<PatrolEnemy>()?.TakeDamage(m_attackDamage);
            h.GetComponent<Crab>()?.TakeDamage(m_attackDamage);
            h.GetComponent<Slime>()?.TakeDamage(m_attackDamage);
            h.GetComponent<Rat>()?.TakeDamage(m_attackDamage);
            h.GetComponent<Bat>()?.TakeDamage(m_attackDamage);
            h.GetComponent<FireDemon>()?.TakeDamage(m_attackDamage);
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
        
        if (currentHealth <= 0) return; // Sửa từ maxHealth thành currentHealth

        currentHealth -= damage; // Sửa từ maxHealth -= damage
        healthBar.fillAmount = (float)currentHealth / maxHealth;
        AudioManager.instance.PlaySFX(AudioManager.instance.hurt);
        Debug.Log("Hero Health: " + currentHealth);

        if (currentHealth <= 0)
        {
            isDead = true;
            m_animator.SetTrigger("Death");
            gameManager.GameOver(CalculateScore());
            AudioManager.instance.PlaySFX(AudioManager.instance.dead);
        }
        else
        {
            m_animator.SetTrigger("Hurt");
        }
    }
    public int CalculateScore()
    {
        float time = timer.GetTime();
        float timeBonus = 2.0f - (time / 720f) * (2.0f - 0.5f);
        timeBonus = Mathf.Clamp(timeBonus, 0.5f, 2.0f);
        
        float goldBonus = m_gold / 10f;

        float healthBonus = (currentHealth <=0)? -100 : (float)currentHealth / maxHealth;

        float finalScore = m_pointValue + healthBonus + goldBonus + timeBonus;
        return finalScore > 0 ? Mathf.CeilToInt((finalScore)) : 0;
    }
    public bool SpendGold(int amount)
    {
        if (m_gold >= amount)
        {
            m_gold -= amount;
            Debug.Log("Đã trừ " + amount + " vàng. Còn lại: " + m_gold);
            cointCount.text = "x " + m_gold.ToString();
            return true;
        }
        Debug.Log("Không đủ tiền!");
        return false;
    }
    public void AddGold(int amount)
    {
        m_gold += amount;
        m_pointValue += amount;
        cointCount.text = "x " + m_gold.ToString();
        Debug.Log("Vàng hiện tại: " + m_gold);
    }
    public void VictoryGame()
    {
        gameManager.Victory(CalculateScore());
    }
    // Debug hitbox
    void OnDrawGizmosSelected()
    {
        Vector2 center = (Vector2)transform.position +
                         new Vector2(m_attackOffset.x * m_facingDirection, m_attackOffset.y);
        Gizmos.DrawWireSphere(center, m_attackRange);
    }

    public void RestoreHealth(int health)
    {
        currentHealth += health;
        healthBar.fillAmount = (float)currentHealth / maxHealth;
        Debug.Log("Đã hồi "+health +"hp");
    }

    public bool IsHealthFull()
    {
        return currentHealth >= maxHealth;
    }
    public void AddDarts(int amount)
    {
        m_dartCount += amount;  
        
        if (Assets.Scripts.GameHUDManager.Instance != null)
        {
            Assets.Scripts.GameHUDManager.Instance.UpdateDartCount(m_dartCount);
        }
    }
    public void IncreaseDamage(int amount)
    {
        m_attackDamage += amount;

        if (m_dartCount > 0)
        {
            m_dartBonusDamage += amount;
            Debug.Log("Đã tăng damage cho cả Kiếm và Phi tiêu!");
        }
        else
        {
            Debug.Log("Không có phi tiêu, thuốc chỉ tăng damage cho Kiếm!");
        }
    }
}
