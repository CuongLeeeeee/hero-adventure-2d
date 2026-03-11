using UnityEngine;

public class DeathLine : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        HeroKnight hero = other.GetComponent<HeroKnight>();
        if (hero != null)
        {
            hero.TakeDamage(99999);
        }
    }
}