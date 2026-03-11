using UnityEngine;

public class Goal : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        HeroKnight hero = other.GetComponent<HeroKnight>();
        if (hero != null)
        {
            hero.VictoryGame();
        }
    }
}