using UnityEngine;

public class Dart : MonoBehaviour
{
    public float speed = 10f;
    public int damage = 1;
    public float lifeTime = 0.6f;
    private int totalDamage;

    void Start()
    {
        // Tự hủy sau một khoảng thời gian để tránh rác bộ nhớ (phi tiêu bay hụt)
        Destroy(gameObject, lifeTime);

        // Tính sát thương cộng thêm từ HeroKnight ngay khi vừa khởi tạo
        CalculateFinalDamage();
    }

    void CalculateFinalDamage()
    {
        HeroKnight player = FindFirstObjectByType<HeroKnight>();
        int bonusDamage = 0;
        if (player != null)
        {
            // Lấy m_attackDamage đã được cộng dồn từ vật phẩm trong Shop
            bonusDamage = player.m_attackDamage;
        }
        totalDamage = damage + bonusDamage;
    }

    public void Launch(int direction)
    {
        // Phóng đi theo hướng nhân vật (1 hoặc -1)
        GetComponent<Rigidbody2D>().linearVelocity = new Vector2(direction * speed, 0);

        // Lật hình ảnh phi tiêu theo hướng bay
        float baseScale = 0.5f;
        transform.localScale = new Vector3(direction * baseScale, baseScale, 1f);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // Biến để kiểm tra xem đã trúng con quái nào chưa
        bool hitAnything = false;

        // 1. Kiểm tra Golem
        GolemEnemy golem = other.GetComponent<GolemEnemy>();
        if (golem != null)
        {
            golem.TakeDamage(totalDamage);
            hitAnything = true;
        }

        // 2. Kiểm tra BatEnemy hoặc Bat (Tùy tên script mày đặt)
        else if (other.GetComponent<BatEnemy>() != null)
        {
            other.GetComponent<BatEnemy>().TakeDamage(totalDamage);
            hitAnything = true;
        }
        else if (other.GetComponent<Bat>() != null)
        {
            other.GetComponent<Bat>().TakeDamage(totalDamage);
            hitAnything = true;
        }

        // 3. Kiểm tra Skeleton
        else if (other.GetComponent<SkeletonEnemy>() != null)
        {
            other.GetComponent<SkeletonEnemy>().TakeDamage(totalDamage);
            hitAnything = true;
        }

        // 4. Kiểm tra PatrolEnemy
        else if (other.GetComponent<PatrolEnemy>() != null)
        {
            other.GetComponent<PatrolEnemy>().TakeDamage(totalDamage);
            hitAnything = true;
        }

        // 5. Kiểm tra Crab
        else if (other.GetComponent<Crab>() != null)
        {
            other.GetComponent<Crab>().TakeDamage(totalDamage);
            hitAnything = true;
        }

        // 6. Kiểm tra Slime
        else if (other.GetComponent<Slime>() != null)
        {
            other.GetComponent<Slime>().TakeDamage(totalDamage);
            hitAnything = true;
        }

        // 7. Kiểm tra Rat
        else if (other.GetComponent<Rat>() != null)
        {
            other.GetComponent<Rat>().TakeDamage(totalDamage);
            hitAnything = true;
        }

        // 8. Kiểm tra FireDemon
        else if (other.GetComponent<FireDemon>() != null)
        {
            other.GetComponent<FireDemon>().TakeDamage(totalDamage);
            hitAnything = true;
        }

        // Nếu trúng bất kỳ con nào ở trên thì biến mất phi tiêu
        if (hitAnything)
        {
            Debug.Log("Phi tiêu trúng mục tiêu! Sát thương: " + totalDamage);
            Destroy(gameObject);
        }
    }
}