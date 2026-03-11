using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class GameManager : MonoBehaviour
{
    public Image deadScreen;
    public Image victorySceen;
    public TextMeshProUGUI ScoreText;

    private bool isGameOver = false;
    private bool isVictory = false;
    void Start()
    {
        if(deadScreen != null)
        {
            deadScreen.gameObject.SetActive(false);
        }
        if(victorySceen != null)
        {
            victorySceen.gameObject.SetActive(false);
        }
        ScoreText.gameObject.SetActive(false);
    }

    void Update()
    {

        if (isGameOver && Input.GetKeyDown(KeyCode.F) || isVictory && Input.GetKeyDown(KeyCode.F))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }

    public void GameOver(int score)
    {
        isGameOver = true;

        if (deadScreen != null)
        {
            deadScreen.gameObject.SetActive(true);
        }
        ScoreText.gameObject.SetActive(true);
        ScoreText.GetComponent<RectTransform>().anchoredPosition = new Vector2(-211f, 269f);
        ScoreText.fontSize = 36;
        ScoreText.text = score.ToString();
    }
    public void Victory(int score)
    {
        isVictory = true;
        if (victorySceen != null)
        {
            victorySceen.gameObject.SetActive(true);
        }
        ScoreText.gameObject.SetActive(true);
        ScoreText.GetComponent<RectTransform>().anchoredPosition = new Vector2(-100f, 465f);
        ScoreText.fontSize = 50;
        ScoreText.text = score.ToString();
    }
}