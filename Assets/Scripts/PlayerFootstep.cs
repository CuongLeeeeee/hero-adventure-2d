using UnityEngine;

public class PlayerFootstep : MonoBehaviour
{
    private string currentSurface = "Ground";

    float stepTimer = 0f;
    float stepDelay = 0.7f;

    Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        // nếu player gần như đứng yên thì không phát sound
        if (Mathf.Abs(rb.linearVelocity.x) < 0.1f)
            return;

        stepTimer -= Time.deltaTime;

        if (stepTimer <= 0)
        {
            PlayFootstep();
            stepTimer = stepDelay;
        }
    }

    void PlayFootstep()
    {
        if (AudioManager.instance == null) return;

        if (currentSurface == "Grass")
            AudioManager.instance.PlayGrassStep();

        else if (currentSurface == "Water")
            AudioManager.instance.PlayWaterStep();

        else
            AudioManager.instance.PlayGroundStep();
    }

    void OnCollisionEnter2D(Collision2D col)
    {
        if (col.gameObject.CompareTag("Grass"))
            currentSurface = "Grass";

        if (col.gameObject.CompareTag("Water"))
            currentSurface = "Water";

        if (col.gameObject.CompareTag("Ground"))
            currentSurface = "Ground";
    }
}