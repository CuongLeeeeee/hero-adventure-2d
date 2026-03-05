using UnityEngine;

public class Chest : MonoBehaviour
{
    private Animator animator;
    private bool playerInRange = false;
    private bool isOpened = false;

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        if (playerInRange && Input.GetKeyDown(KeyCode.F) && !isOpened)
        {
            animator.SetTrigger("Open");
            isOpened = true;
        }
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