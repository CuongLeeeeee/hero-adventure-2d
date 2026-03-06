using UnityEngine;

public class Dart : MonoBehaviour
{
    public float speed = 10f;
    public int damage = 1;
    public float lifeTime = 0.6f; // Tự hủy sau 0.6 giây nếu không trúng gì

    void Start()
    {
        // Tự động hủy sau một khoảng thời gian để tránh rác bộ nhớ
        Destroy(gameObject, lifeTime);
    }

    public void Launch(int direction)
    {
        // Bay theo hướng nhân vật đang nhìn
        GetComponent<Rigidbody2D>().linearVelocity = new Vector2(direction * speed, 0);

        // Xoay hình ảnh phi tiêu theo đúng hướng bay
        if (direction < 0)
            transform.localScale = new Vector3(-1, 1, 1);

        // Thay vì nhân với localScale, ta ép cứng nó về giá trị tuyệt đối
        // Giả sử phi tiêu gốc của mày có kích thước đẹp là 0.5f
        float baseScale = 0.5f;
        transform.localScale = new Vector3(direction * baseScale, baseScale, 1f);
    }


    void OnTriggerEnter2D(Collider2D other)
    {
        // 1. Tìm nhân vật chính để lấy chỉ số damage hiện tại
        // Dùng FindFirstObjectByType (Unity mới) hoặc FindObjectOfType (Unity cũ)
        HeroKnight player = FindFirstObjectByType<HeroKnight>();

        int bonusDamage = 0;
        if (player != null)
        {
            // Lấy sát thương hiện tại của Player (đã được cộng từ thuốc Stamina)
            bonusDamage = player.m_attackDamage;
        }

        // Tổng sát thương = Sát thương gốc + Sát thương cộng thêm
        int totalDamage = damage + bonusDamage;

        // 2. Kiểm tra va chạm với từng loại quái theo cách cũ

        // Kiểm tra Golem
        GolemEnemy golem = other.GetComponent<GolemEnemy>();
        if (golem != null)
        {
            golem.TakeDamage(totalDamage);
            Debug.Log("Phi tiêu gây: " + totalDamage + " damage cho Golem");
            Destroy(gameObject);
            return;
        }

        // Nếu mày có thêm quái khác, thêm if ở đây tương tự Golem
        /*
        SlimeEnemy slime = other.GetComponent<SlimeEnemy>();
        if (slime != null) {
            slime.TakeDamage(totalDamage);
            Destroy(gameObject);
            return;
        }
        */
    }


}
