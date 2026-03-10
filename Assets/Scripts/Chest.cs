using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class Chest : MonoBehaviour
{
    private Animator animator;
    private bool isOpened = false;

    public int quizId;

    public GameObject coinPrefab;
    public int goldDropAmount = 5;

    public GameObject questionPanel;
    public TextMeshProUGUI questionText;

    public Button[] answerButtons;
    public TextMeshProUGUI[] answerTexts;

    public MonoBehaviour playerMovement;

    private bool playerInRange = false;

    SupabaseQuizAPI api;

    Quiz currentQuiz;

    void Start()
    {
        api = FindObjectOfType<SupabaseQuizAPI>();
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        if (playerInRange && Input.GetKeyDown(KeyCode.F))
        {
            StartCoroutine(api.GetQuiz(quizId, ShowQuiz));
            isOpened = true;
        }
    }

    void ShowQuiz(Quiz quiz)
    {
        currentQuiz = quiz;

        questionPanel.SetActive(true);
        questionText.text = quiz.question;

        for (int i = 0; i < quiz.options.Length; i++)
        {
            answerTexts[i].text = quiz.options[i];

            int index = i;

            answerButtons[i].onClick.RemoveAllListeners();
            answerButtons[i].onClick.AddListener(() => CheckAnswer(quiz.options[index]));
        }

        playerMovement.enabled = false;
    }

    void CheckAnswer(string answer)
    {
        if (answer == currentQuiz.correct_answer)
        {
            Debug.Log("Correct!");

            animator.SetTrigger("Open");
            DropGold();
        }
        else
        {
            Debug.Log("Wrong!");
        }

        questionPanel.SetActive(false);
        playerMovement.enabled = true;
    }

    void DropGold()
    {
        if (coinPrefab == null) return;

        for (int i = 0; i < goldDropAmount; i++)
        {
            Vector2 randomOffset = new Vector2(
                Random.Range(-2f, 2f),
                Random.Range(0f, 0.5f)
            );

            GameObject coin = Instantiate(coinPrefab, (Vector2)transform.position + randomOffset, Quaternion.identity);
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerInRange = true;
        }
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerInRange = false;
        }
    }
}