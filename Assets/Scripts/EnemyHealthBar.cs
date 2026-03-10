using UnityEngine;
using UnityEngine.UI;

public class EnemyHealthBar : MonoBehaviour
{
    [Header("References")]
    public Image fillBar;

    private float targetFill;
    private Transform cam;

    void Start()
    {
        cam = Camera.main.transform;
        targetFill = 1f;
        fillBar.fillAmount = 1f;
    }

    void Update()
    {
        transform.rotation = cam.rotation;
        fillBar.fillAmount = targetFill;
    }

    public void SetHealth(int current, int max)
    {
        Debug.Log($"Setting health: {current}/{max}");
        targetFill = (float)current / max;
    }
}