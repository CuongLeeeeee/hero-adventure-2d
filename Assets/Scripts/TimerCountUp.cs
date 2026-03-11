using UnityEngine;
using TMPro;

public class TimerCountUp : MonoBehaviour
{
    private float timeElapsed = 0f;
    public TextMeshProUGUI timerText;

    void Update()
    {
        timeElapsed += Time.deltaTime;

        int minutes = Mathf.FloorToInt(timeElapsed / 60);
        int seconds = Mathf.FloorToInt(timeElapsed % 60);
        int milliseconds = Mathf.FloorToInt((timeElapsed * 100) % 100);

        timerText.text = string.Format("{0:00}:{1:00}:{2:00}",
            minutes, seconds, milliseconds);
    }
    public void ResetTimer()
    {
        timeElapsed = 0f;
    }
    public float GetTime()
    {
         return timeElapsed;
    }
}