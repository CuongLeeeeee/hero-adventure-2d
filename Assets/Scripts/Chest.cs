using UnityEngine;

public class Chest : MonoBehaviour
{
    private Animator animator;
    private bool playerInRange = false;
    private bool isOpened = false;
    public MonoBehaviour playerMovement;

    public GameObject questionPanel;

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        if (playerInRange && Input.GetKeyDown(KeyCode.F) && !isOpened)
        {
            questionPanel.SetActive(true); // mở câu hỏi
            playerMovement.enabled = false; // khóa di chuyển

            isOpened = true;
        }
    }

    public void CorrectAnswer()
    {
        questionPanel.SetActive(false);
        animator.SetTrigger("Open");
        isOpened = true;
        playerMovement.enabled = true;
    }

    public void WrongAnswer()
    {
        questionPanel.SetActive(false);
        Debug.Log("Sai rồi!");
        playerMovement.enabled = true;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerInRange = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerInRange = false;
        }
    }
}