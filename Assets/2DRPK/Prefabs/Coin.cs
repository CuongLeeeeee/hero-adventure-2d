using UnityEngine;

public class Coin : MonoBehaviour
{
    public float pickupDelay = 0.25f; // thời gian chờ trước khi nhặt
    private bool canPickup = false;

    void Start()
    {
        Invoke(nameof(EnablePickup), pickupDelay);
    }

    void EnablePickup()
    {
        canPickup = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!canPickup) return;

        HeroKnight hero = other.GetComponent<HeroKnight>();

        if (hero != null)
        {
            hero.AddGold(10);
            Destroy(gameObject);
        }
    }
}