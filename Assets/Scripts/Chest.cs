using UnityEngine;

public class Chest : MonoBehaviour
{
    private Animator animator;
    private bool playerInRange = false;
    private bool isOpened = false;

    public MonoBehaviour playerMovement;
    public GameObject questionPanel;

    [Header("Drop Gold")]
    public GameObject coinPrefab;
    public int goldDropAmount = 5;

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
        DropGold();
        playerMovement.enabled = true;
    }

    void DropGold()
    {
        if (coinPrefab == null) return;

        for (int i = 0; i < goldDropAmount; i++)
        {
            Vector2 randomOffset = new Vector2(
                Random.Range(-0.5f, 0.5f),
                Random.Range(0f, 0.5f)
            );

            Instantiate(
                coinPrefab,
                (Vector2)transform.position + randomOffset,
                Quaternion.identity
            );
        }
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