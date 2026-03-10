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
        fadeScreen.gameObject.SetActive(false);
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

        fadeScreen.gameObject.SetActive(true);

        youDiedText.gameObject.SetActive(true);
        youDiedText.text = "YOU DIED\nPress F to Restart";
    }
}