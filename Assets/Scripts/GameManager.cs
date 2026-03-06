using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class GameManager : MonoBehaviour
{
    public Image fadeScreen;
    public TextMeshProUGUI youDiedText;

    private bool isGameOver = false;

    void Start()
    {
        fadeScreen.color = new Color(0, 0, 0, 0);
        youDiedText.gameObject.SetActive(false);
    }

    void Update()
    {

        if (isGameOver && Input.GetKeyDown(KeyCode.F))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }

    public void GameOver()
    {
        isGameOver = true;

        fadeScreen.color = new Color(0, 0, 0, 0.7f);

        youDiedText.gameObject.SetActive(true);
        youDiedText.text = "YOU DIED\nPress F to Restart";
    }
}